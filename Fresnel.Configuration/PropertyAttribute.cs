using System;

namespace Envivo.Fresnel.Configuration
{
    /// <summary>
    /// Attributes for a Property
    /// </summary>

    [Serializable()]
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Parameter | AttributeTargets.Enum, AllowMultiple = true)]
    public class PropertyAttribute : MemberAttribute
    {
        private bool _IsGeneratedByPersistenceStore;

        public PropertyAttribute()
        {
            this.BackingFieldName = string.Empty;
            this.CanRead = true;
            this.CanWrite = true;
            this.CanPersist = true;
            this.IsGeneratedByPersistenceStore = false;
            this.UseOptimisticLock = true;
        }

        public bool IsRequired { get; set; }

        /// <summary>
        /// Determines if the Property can be modified only once (for initialisation purposes).
        /// This is similar to the "ReadOnly" variable declaration.
        /// </summary>
        /// <value></value>
        public bool IsWriteOnce { get; set; }

        /// <summary>
        /// Determines if the value can be read in the UI.
        /// This is usually used to override the scope of the associated Property.
        /// </summary>
        /// <value></value>

        public bool CanRead { get; set; }

        /// <summary>
        /// Determines if the value can be updated in the UI.
        /// This is usually used to override the scope of the associated Property.
        /// </summary>
        /// <value></value>

        public bool CanWrite { get; set; }

        /// <summary>
        /// Determines whether the property value can be saved to the underlying persistence store.
        /// Any property that isn't persisted should be declared with 'CanPersist=False'.
        /// </summary>
        /// <value></value>

        public bool CanPersist { get; set; }

        /// <summary>
        /// The name of the backing field that corresponds with the Property
        /// </summary>
        public string BackingFieldName { get; set; }

        /// <summary>
        /// Determines whether this property is generated by the persistence store (eg auto-incrementing or computed values).
        /// This will also mark the property as read-only.
        /// </summary>
        /// <value></value>

        public bool IsGeneratedByPersistenceStore
        {
            get { return _IsGeneratedByPersistenceStore; }
            set
            {
                _IsGeneratedByPersistenceStore = value;
                this.CanWrite = (_IsGeneratedByPersistenceStore == false);
            }
        }

        /// <summary>
        /// Determines if optimistic locking is use on the parent Object when this property is modified
        /// </summary>
        public bool UseOptimisticLock { get; set; }

        /// <summary>
        /// The preferred control for viewing and editing the enum value
        /// </summary>
        public InputControlTypes PreferredInputControl { get; set; }
    }
}