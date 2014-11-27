using System;
using System.Text;
using Envivo.Fresnel.Utils;
using Envivo.Fresnel.Introspection.Templates;
using System.Collections.Generic;
using Envivo.Fresnel.Configuration;
using Envivo.Fresnel.Core.ChangeTracking;

namespace Envivo.Fresnel.Core.Observers
{

    /// <summary>
    /// An Observer for a Domain Object (excluding Non-Reference values and Collections)
    /// </summary>
    public class ObjectObserver : BaseObjectObserver
    {
        private Lazy<PropertyObserverMap> _Properties;
        private Lazy<MethodObserverMap> _Methods;
        private Lazy<MethodObserverMap> _StaticMethods;
        private Lazy<ObjectTracker> _ObjectTracker;

        private PropertyObserverMapBuilder _PropertyObserverMapBuilder;
        private MethodObserverMapBuilder _MethodObserverMapBuilder;
        private AbstractChangeTrackerBuilder _ChangeTrackerBuilder;

        private bool _IsLazyLoadingAlreadyDetermined;

        /// <summary>
        ///
        /// </summary>
        /// <param name="obj">The Object to be proxied</param>
        /// <param name="objectType">The Type of the Object to be proxied</param>
        public ObjectObserver
        (
            object obj,
            Type objectType,
            ClassTemplate tClass,
            PropertyObserverMapBuilder propertyObserverMapBuilder,
            MethodObserverMapBuilder methodObserverMapBuilder,
            AbstractChangeTrackerBuilder changeTrackerBuilder
        )
            : base(obj, objectType, tClass)
        {
            _PropertyObserverMapBuilder = propertyObserverMapBuilder;
            _MethodObserverMapBuilder = methodObserverMapBuilder;
            _ChangeTrackerBuilder = changeTrackerBuilder;

            _ObjectTracker = new Lazy<ObjectTracker>(
                                () => (ObjectTracker)_ChangeTrackerBuilder.BuildForObject(this),
                                System.Threading.LazyThreadSafetyMode.ExecutionAndPublication);

            _Properties = new Lazy<PropertyObserverMap>(
                                () => _PropertyObserverMapBuilder.BuildFor(this),
                                System.Threading.LazyThreadSafetyMode.ExecutionAndPublication);

            _Methods = new Lazy<MethodObserverMap>(
                              () => _MethodObserverMapBuilder.BuildFor(this),
                              System.Threading.LazyThreadSafetyMode.ExecutionAndPublication);

            _StaticMethods = new Lazy<MethodObserverMap>(
                              () => _MethodObserverMapBuilder.BuildStaticMethodsFor(this),
                              System.Threading.LazyThreadSafetyMode.ExecutionAndPublication);
        }

        internal override void FinaliseConstruction()
        {
            base.FinaliseConstruction();

            this.CheckIfPropertiesShouldLazyLoad();
        }

        public new ClassTemplate Template
        {
            get { return (ClassTemplate)base.Template; }
        }

        public ObjectTracker ChangeTracker { get; internal set; }

        /// <summary>
        /// A set of all visible Properties for the proxied Object
        /// </summary>
        /// <value>A generic Dictionary of PropertyObservers</value>

        public PropertyObserverMap Properties
        {
            get { return _Properties.Value; }
        }

        /// <summary>
        /// A set of all visible Methods for the proxied Object
        /// </summary>
        /// <value>A generic Dictionary of MethodObservers</value>

        public MethodObserverMap Methods
        {
            get { return _Methods.Value; }
        }

        /// <summary>
        /// A set of all visible Static Methods
        /// </summary>
        /// <value>A generic Dictionary of MethodObservers</value>

        public MethodObserverMap StaticMethods
        {
            get { return _StaticMethods.Value; }
        }

        /// <summary>
        /// Returns the Member Observer for the given member name
        /// </summary>
        /// <param name="memberName"></param>
        public BaseMemberObserver this[string memberName]
        {
            get
            {
                var result = (BaseMemberObserver)
                                this.Properties.TryGetValueOrNull(memberName) ??
                                this.Methods.TryGetValueOrNull(memberName);

                if (result == null)
                {
                    throw new ArgumentOutOfRangeException();
                }

                return result;
            }
        }

        /// <summary>
        /// Returns TRUE if this Object is owned by another Object.
        /// Ownership is defined as a Composite relationship.
        /// </summary>
        internal bool IsAlreadyOwned()
        {
            foreach (var oCollection in this.OuterCollections)
            {
                foreach (var oProp in oCollection.OuterProperties)
                {
                    var tProp = oProp.Template;
                    if (tProp.IsCollection)
                    {
                        var relationship = tProp.Attributes.Get<CollectionPropertyAttribute>().Relationship;
                        if (relationship != ManyRelationship.OwnsMany)
                            continue;
                    }
                    else
                    {
                        var relationship = tProp.Attributes.Get<ObjectPropertyAttribute>().Relationship;
                        if (relationship != SingleRelationship.OwnsA)
                            continue;
                    }

                    return true;
                }
            }

            // Do we have any properties marked as 'Parent' that point to the given Object?
            foreach (var oProp in this.OuterProperties)
            {
                var tProp = oProp.Template;
                var relationship = tProp.Attributes.Get<ObjectPropertyAttribute>().Relationship;
                if (relationship != SingleRelationship.OwnedBy)
                    continue;

                return true;
            }

            return false;
        }


        /// <summary>
        /// Determines if the associated Object is new, and allows reading of all properties
        /// </summary>
        internal void CheckIfPropertiesShouldLazyLoad()
        {
            if (_IsLazyLoadingAlreadyDetermined)
                return;

            if (this.ChangeTracker.IsNewInstance)
            {
                this.MakePropertyValuesImmediatelyAvailable();
            }

            _IsLazyLoadingAlreadyDetermined = true;
        }

        /// <summary>
        /// Makes all Object/List properties instantly available (subject to the Persistor's lazy-load mechanism)
        /// </summary>
        internal void MakePropertyValuesImmediatelyAvailable()
        {
            foreach (var oProp in this.Properties.ForObjects)
            {
                oProp.IsLazyLoadPending = false;
            }

            _IsLazyLoadingAlreadyDetermined = true;
        }

        /// <summary>
        /// Resets all Object/List properties so that they are forced to load again
        /// </summary>
        internal void MakePropertiesLazyLoad()
        {
            if (this.ChangeTracker.IsNewInstance)
            {
                this.MakePropertyValuesImmediatelyAvailable();
            }
            else
            {
                var tClass = this.Template;
                foreach (var oProp in this.Properties.ForObjects)
                {
                    oProp.ResetLazyLoadStatus(tClass.IsPersistable);
                }
            }

            _IsLazyLoadingAlreadyDetermined = false;
        }

        ///// <summary>
        ///// Returns TRUE if this Entity is owned by the given Entity.
        ///// Ownership is defined as a Composite relationship.
        ///// </summary>
        ///// <param name="oParent"></param>

        //internal bool IsOwnedBy(ObjectObserver oParent)
        //{
        //    // Do we have any properties marked as 'Parent' that point to the given entity?
        //    foreach (var oProp in this.Properties.ForObjects)
        //    {
        //        var tParentClass = oProp.Template;
        //        var tProp = oProp.Template;

        //        if (tParentClass.RealObjectType.IsDerivedFrom(tProp.PropertyType) == false)
        //            continue;

        //        var relationship = tProp.Attributes.Get<ObjectPropertyAttribute>().Relationship;
        //        if (relationship != SingleRelationship.OwnedBy)
        //            continue;

        //        if (object.Equals(oParent.RealObject, oProp.GetValueByForce()))
        //            return true;
        //    }

        //    return false;
        //}

        ///// <summary>
        ///// Returns TRUE if this Object has an owner of the given Type
        ///// Ownership is defined as a Composite relationship.
        ///// </summary>
        ///// <param name="ownerType"></param>

        //internal bool HasOwnerOfType(Type ownerType)
        //{
        //    // Do we have any properties marked as 'Parent' that point to the given Object?
        //    foreach (var oProp in this.Properties.ForObjects)
        //    {
        //        var tProp = oProp.Template;
        //        var relationship = tProp.Attributes.Get<ObjectPropertyAttribute>().Relationship;
        //        if (relationship != SingleRelationship.OwnedBy)
        //            continue;

        //        if (ownerType.IsDerivedFrom(oProp.RealObjectType) == false)
        //            continue;

        //        if (oProp.GetValueByForce() != null)
        //            return true;
        //    }

        //    return false;
        //}

        internal override void SetRealObject(object obj)
        {
            //if (object.ReferenceEquals(this.Object, Object)) return;
            if (this.RealObject == obj)
                return;

            base.SetRealObject(obj);

            // Cascade the new Object to all members:
            foreach (var oProperty in this.Properties.Values)
            {
                oProperty.SetRealObject(obj);
            }
            foreach (var oMethod in this.Methods.Values)
            {
                oMethod.SetRealObject(obj);
            }
        }

        ///// <summary>
        ///// Returns the Validator for the associated Object
        ///// </summary>
        //internal ObjectValidator Validator
        //{
        //    get
        //    {
        //        if (_ObjectValidator == null)
        //        {
        //            lock (s_LockObject)
        //            {
        //                if (_ObjectValidator == null)
        //                {
        //                    _ObjectValidator = new ObjectValidator(this);
        //                }
        //            }
        //        }
        //        return _ObjectValidator;
        //    }
        //}

        //public ObjectObserver Clone()
        //{
        //    var clone = this.Template.Clone(this.RealObject, false);
        //    clone.SetId(DomainTypes.Utils.GuidFactory.NewSequentialGuid());
        //    clone.SetAudit(null);

        //    var oClone = this.Session.GetObjectObserver(clone);

        //    // Make sure the clone can be edited before the user saves it:
        //    oClone.ChangeTracker.IsNewInstance = true;

        //    return oClone;
        //}

        /// <summary>
        /// Determines whether the Observer needs to be persisted.
        /// </summary>


        ///// <remarks> NB. This will perform a brute-force check, which is slightly slower. Use ChangeTracker.IsDirty for performance.</remarks>
        //public override bool IsDirty
        //{
        //    get
        //    {
        //        // We might have connected objects that were modified by consumer code, so check the entire graph:
        //        var graphIterator = new ObjectGraphIterator();
        //        foreach (var oObj in graphIterator.GetObjects(this, ObjectGraphIterator.TraversingOptions.Basic))
        //        {
        //            oObj.ChangeTracker.DetectChanges();
        //        }

        //        return this.ChangeTracker.IsDirty;
        //    }
        //    // TODO: Do we need to add a setter, so that NotifyPropertyChange events can be raised?
        //    //       If so, the ChangeTracker must force this value to change
        //}

        ///// <summary>
        ///// Determines if this Observer can be disposed
        ///// </summary>

        //internal IAssertion IsReadyToDispose()
        //{
        //    if (_UI != null && _UI.CanDispose == false)
        //    {
        //        var message = string.Concat(this.DebugID, " cannot be disposed because it's in use");
        //        return Assertion.Fail(message);
        //    }

        //    //-----
        //    var oOuterObjects = this.GetOuterObjects(99);
        //    foreach (var oOuterObject in oOuterObjects)
        //    {
        //        if (oOuterObject._UI != null && oOuterObject._UI.CanDispose == false)
        //        {
        //            string message = string.Concat(this.DebugID, " cannot be disposed because ", oOuterObject.DebugID, " is in use");
        //            return Assertion.Fail(message);
        //        }
        //    }

        //    return Assertion.Pass();
        //}

        public override void Dispose()
        {
            if (_Methods.IsValueCreated)
            {
                _Methods.Value.ClearSafely();
            }
            _Methods = null;

            if (_Properties.IsValueCreated)
            {
                _Properties.Value.ClearSafely();
            }
            _Properties = null;

            if (_StaticMethods.IsValueCreated)
            {
                _StaticMethods.Value.ClearSafely();
            }
            _StaticMethods = null;

            //_ObjectValidator.DisposeSafely();
            //_ObjectValidator = null;

            base.Dispose();
        }

    }
}
