using Envivo.Fresnel.Configuration;
using Envivo.Fresnel.Introspection.Assemblies;
using Newtonsoft.Json;
using System;

namespace Envivo.Fresnel.Introspection.Templates
{
    public class BaseTemplate : ITemplate
    {
        protected internal Lazy<XmlComments> _XmlComments;

        /// <summary>
        /// The AssemblyReader associated with this Template
        /// </summary>
        [JsonIgnore]
        public AssemblyReader AssemblyReader { get; internal set; }

        /// <summary>
        /// The Name of the Template
        /// </summary>
        public string Name { get; internal set; }

        /// <summary>
        /// The Name of the Template in a friendly format
        /// </summary>
        public string FriendlyName { get; internal set; }

        /// <summary>
        /// The fully qualified name for the Template
        /// </summary>
        /// <value></value>
        [JsonIgnore]
        public string FullName { get; internal set; }

        /// <summary>
        /// All custom Attributes defined for this Template. Values from ClassConfigurations are accessible here.
        /// </summary>
        [JsonIgnore]
        public AttributesMap Attributes { get; internal set; }

        public XmlComments XmlComments
        {
            get { return _XmlComments.Value; }
        }

        /// <summary>
        /// Determines if this Template is visible to the user
        /// </summary>
        public bool IsVisible { get; internal set; }

        public override string ToString()
        {
            return string.Concat(this.GetType().Name, ":", this.Name);
        }

        internal virtual void FinaliseConstruction()
        {
        }
    }
}