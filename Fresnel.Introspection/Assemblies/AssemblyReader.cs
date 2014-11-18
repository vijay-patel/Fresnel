using System;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;
using System.Xml;
using System.ComponentModel;
using Envivo.Fresnel.Introspection.Templates;
using Envivo.Fresnel.Utils;
using Envivo.Fresnel.Introspection.Configuration;

namespace Envivo.Fresnel.Introspection.Assemblies
{

    /// <summary>
    /// Used to interrogate a .NET assembly for Object classes
    /// </summary>
    /// <remarks>
    /// We've sacrificed some code readability for optimisations
    /// </remarks>
    public class AssemblyReader : IDisposable
    {
        private Dictionary<Type, IClassTemplate> _TemplateMap = new Dictionary<Type, IClassTemplate>();
        private XmlDocument _ClassStructureXml = new XmlDocument();

        private AbstractClassTemplateBuilder _AbstractClassTemplateBuilder;

        //public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Creates and caches Class Templates for the given Assembly
        /// </summary>
        /// <param name="assembly">The Assembly whose details are to be read</param>

        public AssemblyReader
        (
            AbstractClassTemplateBuilder abstractClassTemplateBuilder
        )
        {
            _AbstractClassTemplateBuilder = abstractClassTemplateBuilder;
        }

        /// <summary>
        /// The Assembly associated with this reader
        /// </summary>
        public Assembly Assembly { get; internal set; }

        public ConfigurationMap ConfigurationMap { get; internal set; }

        public AssemblyDocsReader AssemblyDocsReader { get; internal set; }

        public bool IsFrameworkAssembly { get; internal set; }

        //public bool AreInfrastructureServicesEnabled { get; private set; }

        /// <summary>
        /// Returns TRUE if Domain Classes were found in this Assembly (as opposed to Infrastructure Service classes)
        /// </summary>
        public bool ContainsDomainClasses
        {
            get { return _TemplateMap.Count > 0; }
        }

        /// <summary>
        /// An XML representation of the class structure of the Domain Assembly
        /// </summary>
        /// <value>An XmlDocument containing the class structure</value>

        public XmlDocument ClassStructureXml
        {
            get { return _ClassStructureXml; }
        }

        /// <summary>
        /// Returns the codebase of the Assembly as a file path (not a URI)
        /// </summary>
        /// <param name="assembly"></param>

        public string GetAssemblyLocation()
        {
            var path = this.Assembly.CodeBase.Replace(@"file:///", string.Empty);
            return path.Replace(@"/", @"\");
        }

        /// <summary>
        /// Returns a Template from the cache for the given object Type. A new Template is created if one does not already exist in the cache.
        /// </summary>
        /// <param name="classType">The Type of the Object</param>
        /// <returns>A Template for the given object Type</returns>
        /// <remarks>
        /// </remarks>
        public IClassTemplate GetTemplate(Type classType)
        {
            var tClass = _TemplateMap.TryGetValueOrNull(classType);

            if (tClass == null)
            {
                var classConfiguration = this.ConfigurationMap.GetClassConfiguration(classType);
                tClass = _AbstractClassTemplateBuilder.CreateTemplate(classType, classConfiguration);
                ((ClassTemplate)tClass).AssemblyReader = this;
                _TemplateMap.Add(classType, tClass);
            }

            return tClass;
        }

        public IClassTemplate GetTemplate<TClass>()
            where TClass : class
        {
            return this.GetTemplate(typeof(TClass));
        }

        public IEnumerable<ClassTemplate> GetTemplates()
        {
            var results = _TemplateMap.OfType<ClassTemplate>().ToArray();
            return results;
        }

        /// <summary>
        /// Returns the number of known Templates for the associated Assembly
        /// </summary>
        public int TemplateCount
        {
            get { return _TemplateMap.Count; }
        }

        public override string ToString()
        {
            return this.Assembly.ToString();
        }

        public void Dispose()
        {
            this.Assembly = null;
            _TemplateMap = null;
            _ClassStructureXml = null;
            this.AssemblyDocsReader = null;
        }

    }

}
