using Envivo.Fresnel.Configuration;
using Envivo.Fresnel.Core.ChangeTracking;
using Envivo.Fresnel.Introspection.Templates;
using Envivo.Fresnel.Utils;
using Newtonsoft.Json;
using System;
using System.ComponentModel.DataAnnotations;

namespace Envivo.Fresnel.Core.Observers
{
    /// <summary>
    /// An Observer for a Domain Object (excluding Non-Reference values and Collections)
    /// </summary>
    public class ObjectObserver : BaseObjectObserver
    {
        private Lazy<PropertyObserverMap> _Properties;
        private Lazy<MethodObserverMap> _Methods;
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
                                () => (ObjectTracker)_ChangeTrackerBuilder.BuildFor(this),
                                System.Threading.LazyThreadSafetyMode.ExecutionAndPublication);

            _Properties = new Lazy<PropertyObserverMap>(
                                () => _PropertyObserverMapBuilder.BuildFor(this),
                                System.Threading.LazyThreadSafetyMode.ExecutionAndPublication);

            _Methods = new Lazy<MethodObserverMap>(
                              () => _MethodObserverMapBuilder.BuildFor(this),
                              System.Threading.LazyThreadSafetyMode.ExecutionAndPublication);
        }

        [JsonIgnore]
        public new ClassTemplate Template
        {
            get { return (ClassTemplate)base.Template; }
        }

        public ObjectTracker ChangeTracker
        {
            get { return _ObjectTracker.Value; }
        }

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
                    var relationship = tProp.Attributes.Get<RelationshipAttribute>();
                    if (relationship.Type == RelationshipType.Owns)
                    {
                        return true;
                    }
                }
            }

            // Do we have any properties marked as 'Parent' that point to the given Object?
            foreach (var oProp in this.OuterProperties)
            {
                var tProp = oProp.Template;
                var relationship = tProp.Attributes.Get<RelationshipAttribute>();
                if (relationship.Type == RelationshipType.OwnedBy)
                {
                    return true;
                }
            }

            return false;
        }

        public void MarkAsTransient()
        {
            this.ChangeTracker.IsTransient = true;
            this.CheckIfPropertiesShouldLazyLoad();
        }

        public void MarkAsPersistent()
        {
            this.ChangeTracker.IsTransient = false;
            this.CheckIfPropertiesShouldLazyLoad();
        }

        /// <summary>
        /// Determines if the associated Object is new, and allows reading of all properties
        /// </summary>
        private void CheckIfPropertiesShouldLazyLoad()
        {
            if (_IsLazyLoadingAlreadyDetermined)
                return;

            // If an object can't be loaded from a persistence store, it doesn't support lazy-loading.
            // If an object only exists in memory, it's properties should be immediately available.
            if (!this.Template.IsPersistable || 
                 this.ChangeTracker.IsTransient)
            {
                this.MakePropertyValuesImmediatelyAvailable();
            }

            _IsLazyLoadingAlreadyDetermined = true;
        }

        /// <summary>
        /// Makes all Object/List properties instantly available (subject to the Persistor's lazy-load mechanism)
        /// </summary>
        private void MakePropertyValuesImmediatelyAvailable()
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
            if (this.ChangeTracker.IsTransient)
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

        public override void Dispose()
        {
            if (_Methods != null && _Methods.IsValueCreated)
            {
                _Methods.Value.Dispose();
            }
            _Methods = null;

            if (_Properties != null && _Properties.IsValueCreated)
            {
                _Properties.Value.Dispose();
            }
            _Properties = null;

            base.Dispose();
        }

    }
}