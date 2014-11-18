using System;
using System.Linq;
using System.Collections.Generic;
using Envivo.Fresnel.Introspection.Configuration;
using Envivo.Fresnel.Utils;
using System.Reflection;

namespace Envivo.Fresnel.Introspection.Templates
{

    /// <summary>
    /// A Template that represents a Object Type
    /// </summary>
    public class ClassTemplate : BaseClassTemplate
    {
        private readonly string[] _AbstractTypePrefixes = { "Abstract ", "Base ", "I " };
        private readonly string[] _AbstractTypeSuffixes = { " Base" };

        private Lazy<ConstructorInfo[]> _Constructors;
        private Lazy<FieldInfoMap> _Fields;
        private Lazy<PropertyTemplateMap> _tProperties;
        private Lazy<MethodTemplateMap> _tMethods;
        private Lazy<MethodTemplateMap> _tStaticMethods;

        private ObjectInstanceAttribute _ObjectInstanceAttr;
        private Lazy<int> _InheritanceDepth;

        private FieldInfoMapBuilder _FieldInfoMapBuilder;
        private PropertyTemplateMapBuilder _PropertyTemplateMapBuilder;
        private MethodTemplateMapBuilder _MethodTemplateMapBuilder;
        private StaticMethodTemplateMapBuilder _StaticMethodTemplateMapBuilder;
        private TrackingPropertiesIdentifier _TrackingPropertiesIdentifier;

        private Lazy<PropertyTemplate> _IdProperty;
        private Lazy<PropertyTemplate> _VersionProperty;
        private Lazy<PropertyTemplate> _AuditProperty;

        public ClassTemplate
        (
            FieldInfoMapBuilder fieldInfoMapBuilder,
            PropertyTemplateMapBuilder propertyTemplateMapBuilder,
            MethodTemplateMapBuilder methodTemplateMapBuilder,
            StaticMethodTemplateMapBuilder staticMethodTemplateMapBuilder,
            TrackingPropertiesIdentifier trackingPropertiesIdentifier
        )
            : base()
        {
            _FieldInfoMapBuilder = fieldInfoMapBuilder;
            _PropertyTemplateMapBuilder = propertyTemplateMapBuilder;
            _MethodTemplateMapBuilder = methodTemplateMapBuilder;
            _StaticMethodTemplateMapBuilder = staticMethodTemplateMapBuilder;
            _TrackingPropertiesIdentifier = trackingPropertiesIdentifier;
        }

        internal override void FinaliseConstruction()
        {
            base.FinaliseConstruction();

            this.CreateNames();

            this.DetermineInterfaces();

            _ObjectInstanceAttr = this.Attributes.Get<ObjectInstanceAttribute>();

            _InheritanceDepth = new Lazy<int>(() =>
                                DetermineInheritanceDepth(this.RealObjectType),
                                System.Threading.LazyThreadSafetyMode.ExecutionAndPublication);

            _Constructors = new Lazy<ConstructorInfo[]>(
                                () => this.RealObjectType.GetConstructors(),
                                System.Threading.LazyThreadSafetyMode.ExecutionAndPublication);

            _Fields = new Lazy<FieldInfoMap>(
                                () => _FieldInfoMapBuilder.BuildFor(this.RealObjectType),
                                System.Threading.LazyThreadSafetyMode.ExecutionAndPublication);

            _tProperties = new Lazy<PropertyTemplateMap>(
                                () => _PropertyTemplateMapBuilder.BuildFor(this),
                                System.Threading.LazyThreadSafetyMode.ExecutionAndPublication);

            _tMethods = new Lazy<MethodTemplateMap>(
                                () => _MethodTemplateMapBuilder.BuildFor(this),
                                System.Threading.LazyThreadSafetyMode.ExecutionAndPublication);

            _tStaticMethods = new Lazy<MethodTemplateMap>(
                                () => _StaticMethodTemplateMapBuilder.BuildFor(this),
                                System.Threading.LazyThreadSafetyMode.ExecutionAndPublication);

            _IdProperty = new Lazy<PropertyTemplate>(
                                () => _TrackingPropertiesIdentifier.DetermineIdProperty(this, _ObjectInstanceAttr),
                                System.Threading.LazyThreadSafetyMode.ExecutionAndPublication);

            _VersionProperty = new Lazy<PropertyTemplate>(
                                () => _TrackingPropertiesIdentifier.DetermineVersionProperty(this, _ObjectInstanceAttr),
                                System.Threading.LazyThreadSafetyMode.ExecutionAndPublication);

            _AuditProperty = new Lazy<PropertyTemplate>(
                                () => _TrackingPropertiesIdentifier.DetermineAuditProperty(this, _ObjectInstanceAttr),
                                System.Threading.LazyThreadSafetyMode.ExecutionAndPublication);
        }

        private void CreateNames()
        {
            // Generic objects (usually collections) need to be uniquely identifiable:
            foreach (var genericArg in this.RealObjectType.GetGenericArguments())
            {
                this.Name += "_" + genericArg.Name;
            }

            this.FullName = string.Concat(this.RealObjectType.Namespace, ".", this.RealObjectType.Name);

            // Users shouldn't need to know about Abstract or Interfaces types, so let's fix that:
            if (this.RealObjectType.IsAbstract || this.RealObjectType.IsInterface)
            {
                this.FriendlyName = this.CreateFriendlyNameForAbstractTypes();
            }
        }

        private string CreateFriendlyNameForAbstractTypes()
        {
            var stringToReplace = _AbstractTypePrefixes.SingleOrDefault(p => this.FriendlyName.StartsWith(p));
            if (stringToReplace.IsEmpty())
            {
                stringToReplace = _AbstractTypeSuffixes.SingleOrDefault(p => this.FriendlyName.EndsWith(p));
            }

            if (stringToReplace.IsNotEmpty())
            {
                var result = string.Concat(this.FriendlyName.Replace(stringToReplace, string.Empty), " Types");
                return result;
            }

            return null;
        }

        private void DetermineInterfaces()
        {
            this.IsEntity = this.RealObjectType.IsEntity();
            this.IsValueObject = this.RealObjectType.IsValueObject();
            this.IsAggregateRoot = this.RealObjectType.IsAggregateRoot();
            this.IsCloneable = this.RealObjectType.IsDerivedFrom<ICloneable>();

            this.HasErrorInfo = this.RealObjectType.IsDataErrorInfo();
            this.IsValidatable = this.RealObjectType.IsValidatable();
        }

        /// <summary>
        /// Returns the Property used to retrieve the Domain Object's unique ID
        /// </summary>
        public PropertyTemplate IdProperty { get; private set; }

        /// <summary>
        /// Returns the Property used to retrieve the object's Version
        /// </summary>
        public PropertyTemplate VersionProperty { get; private set; }


        /// <summary>
        /// Returns the Property used to retrieve the object's IAudit details
        /// </summary>
        public PropertyTemplate AuditProperty { get; private set; }

        /// <summary>
        /// Determines if the Domain Object can be persisted to a repository
        /// </summary>
        public virtual bool IsPersistable
        {
            get
            {
                return _ObjectInstanceAttr.IsPersistable &&
                        this.IdProperty != null;
            }
        }

        /// <summary>
        /// Determins if the Domain Object has an ID property
        /// </summary>
        public bool IsTrackable
        {
            get { return this.IdProperty != null; }
        }

        /// <summary>
        /// Determines if the Domain Object implements the IEntity interface
        /// </summary>
        public bool IsEntity { get; private set; }

        /// <summary>
        /// Determines if the Domain Object implements the IValueObject interface
        /// </summary>
        public bool IsValueObject { get; private set; }

        /// <summary>
        /// Determines if the Domain Object implements the IAggregateRoot interface
        /// </summary>
        public bool IsAggregateRoot { get; private set; }

        /// <summary>
        /// Determines if the Domain Object implements the ICloneable interface
        /// </summary>
        public bool IsCloneable { get; private set; }

        /// <summary>
        /// Determines if the Domain Object has an IsValid() method
        /// </summary>
        public bool IsValidatable { get; private set; }

        /// <summary>
        /// Determines if the Domain Object implements IDataErrorInfo
        /// </summary>
        public bool HasErrorInfo { get; private set; }

        /// <summary>
        /// The collection of available public Constructors
        /// </summary>
        public ConstructorInfo[] Constructors
        {
            get { return _Constructors.Value; }
        }

        public ConstructorInfo DefaultConstructor
        {
            get
            {
                return _Constructors.Value.SingleOrDefault(c => !c.GetParameters().Any());
            }
        }

        /// <summary>
        /// Determines if an Object can be instantiated with zero arguments
        /// </summary>
        public bool HasDefaultConstructor
        {
            get { return DefaultConstructor != null; }
        }

        /// <summary>
        /// The collection of Fields within the Object
        /// </summary>
        internal FieldInfoMap Fields
        {
            get { return _Fields.Value; }
        }

        /// <summary>
        /// The collection of PropertyTemplates associated with the Object
        /// </summary>
        public PropertyTemplateMap Properties
        {
            get { return _tProperties.Value; }
        }

        /// <summary>
        /// The collection of MethodTemplates associated with the Object
        /// </summary>
        public MethodTemplateMap Methods
        {
            get { return _tMethods.Value; }
        }

        /// <summary>
        /// The collection of MethodTemplates of the static methods in the Object Type
        /// </summary>
        public MethodTemplateMap StaticMethods
        {
            get { return _tStaticMethods.Value; }
        }

        /// <summary>
        /// Determines if an Object can be instantiated from the associated Class
        /// </summary>
        public bool IsCreatable
        {
            get
            {
                return _ObjectInstanceAttr.IsCreatable &&
                        !this.RealObjectType.IsAbstract &&
                        this.HasDefaultConstructor;
            }
        }

        /// <summary>
        /// Returns TRUE if the class has a constructor that accepts the given argument
        /// </summary>
        /// <param name="constructorArgType"></param>
        
        public bool CanBeCreatedWith(Type constructorArgType)
        {
            if (constructorArgType == null)
            {
                // We're only interested in the default constructor:
                return this.HasDefaultConstructor;
            }
            else
            {
                // Find a matching ctor:
                foreach (var ctor in _Constructors.Value)
                {
                    var parameters = ctor.GetParameters();

                    if ((parameters.Length == 1) && constructorArgType.IsDerivedFrom(parameters[0].ParameterType))
                    {
                        // We've found a match:
                        return true;
                    }
                }

                return false;
            }
        }

        //
        //        /// <summary>
        //        /// All of the relationships that this Object has to other Domain Objects
        //        /// </summary>
        //        public RelationshipCollection Relationships
        //        {
        //            get
        //            {
        //                if (_Relationships == null)
        //                {
        //                    _Relationships = new RelationshipCollection(this);
        //                }
        //                return _Relationships;
        //            }
        //            //internal set { _Relationships = value; }
        //        }
        //
        //        /// <summary>
        //        /// Returns TRUE if a Relationship exists with the given Target class
        //        /// </summary>
        //        /// <param name="tTargetClass"></param>
        //        
        //        /// <remarks>Humane interface</remarks>
        //        public bool HasRelationshipWith(ClassTemplate tTargetClass)
        //        {
        //            // Find a Relationship in the Target class that points back to this Property's class:
        //            var result = tTargetClass.Relationships.Any(r => r.TargetClass == this);
        //            return result;
        //        }
        //
        //        /// <summary>
        //        /// Returns TRUE if a Relationship exists with the given Target property
        //        /// </summary>
        //        /// <param name="tTargetProperty"></param>
        //        
        //        /// <remarks>Humane interface</remarks>
        //        public bool HasRelationshipWith(PropertyTemplate tTargetProperty)
        //        {
        //            // Find a Relationship in the Target property's class that points back to this Property:
        //            var result = tTargetProperty.OuterClass.Relationships.Any(r => r.TargetClass == tTargetProperty.OuterClass);
        //            return result;
        //        }

        /// <summary>
        /// Returns the depth of the Object class in an inheritance tree
        /// </summary>
        public int InheritanceDepth
        {
            get { return _InheritanceDepth.Value; }
        }

        private static int DetermineInheritanceDepth(Type objectType)
        {
            var result = 0;
            var superType = objectType.BaseType;
            while (superType != null)
            {
                result++;
                superType = superType.BaseType;
            }

            return result;
        }

    }
}
