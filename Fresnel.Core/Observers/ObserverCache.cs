using Envivo.Fresnel.Introspection;
using System;
using System.Collections.Generic;
using System.Linq;
using Envivo.Fresnel.Utils;
using Envivo.Fresnel.Introspection.Templates;
using Envivo.Fresnel.DomainTypes.Interfaces;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Envivo.Fresnel.Core.Observers
{

    /// <summary>
    /// Returns Observers for .NET objects & values
    /// </summary>
    public class ObserverCache
    {
        private ConditionalWeakTable<object, ObjectObserver> _ObjectMap = new ConditionalWeakTable<object, ObjectObserver>();
        private Dictionary<object, NonReferenceObserver> _NonReferenceMap = new Dictionary<object, NonReferenceObserver>();

        private TemplateCache _TemplateCache;
        private AbstractObserverBuilder _AbstractObserverBuilder;
        private ObjectIdResolver _ObjectIdResolver;

        private NullObserver _NullObserver;

        public ObserverCache
        (
            TemplateCache templateCache,
            AbstractObserverBuilder abstractObserverBuilder,
            ObjectIdResolver objectIdResolver,
            NullObserver nullObserver
        )
        {
            if (templateCache == null)
                throw new ArgumentNullException("templateCache");

            if (abstractObserverBuilder == null)
                throw new ArgumentNullException("abstractObserverBuilder");

            _TemplateCache = templateCache;
            _AbstractObserverBuilder = abstractObserverBuilder;
            _ObjectIdResolver = objectIdResolver;
            _NullObserver = nullObserver;
        }

        /// <summary>
        /// Returns an ObjectObserver from the cache for the given Domain object
        /// </summary>
        /// <param name="obj"></param>
        public BaseObjectObserver GetObserver(object obj)
        {
            if (obj == null)
                throw new ArgumentNullException("obj");

            var objectType = obj.GetType();
            return this.GetObserver(obj, objectType);
        }

        /// <summary>
        /// Returns an Observer from the cache for the given object Type.
        /// </summary>
        /// <param name="obj">The Object to be observed</param>
        /// <param name="objectType">The Type of the Object to be observed</param>
        public BaseObjectObserver GetObserver(object obj, Type objectType)
        {
            if (obj == null)
                return _NullObserver;

            var result = this.GetCachedObserver(obj, objectType);
            if (result == null)
            {
                result = CreateAndCacheObserver(obj, objectType);
            }
            else
            {
                Debug.WriteLine(string.Concat("Found Observer for ", result.Template.Name, " with ID ", result.ID));
            }

            return result;
        }

        private BaseObjectObserver GetCachedObserver(object obj, Type objectType)
        {
            var template = _TemplateCache.GetTemplate(objectType);
            var tClass = template as ClassTemplate;
            if (tClass != null)
            {
                ObjectObserver match = null;
                _ObjectMap.TryGetValue(obj, out match);
                return match;
            }
            else
            {
                return _NonReferenceMap.TryGetValueOrNull(obj);
            }
        }

        private BaseObjectObserver CreateAndCacheObserver(object obj, Type objectType)
        {
            if (obj == null)
                return _NullObserver;

            var template = _TemplateCache.GetTemplate(objectType);
            var tClass = template as ClassTemplate;

            if (tClass != null)
            {
                Debug.WriteLine(string.Concat("Creating Observer for ", tClass.Name, " with hash code ", obj.GetHashCode()));

                var result = (ObjectObserver)_AbstractObserverBuilder.BuildFor(obj, template.RealType);
                _ObjectMap.Add(obj, result);
                MergeObjectsWithSameId(obj, result);
                return result;
            }

            if (tClass == null)
            {
                var result = (NonReferenceObserver)_AbstractObserverBuilder.BuildFor(obj, template.RealType);
                _NonReferenceMap.Add(obj, result);
                return result;
            }

            return null;
        }

        private void MergeObjectsWithSameId(object obj, ObjectObserver oObject)
        {
            if (object.ReferenceEquals(obj, oObject.RealObject))
                return;

            // We've been given a *different* Object with the same ID??!!

            System.Diagnostics.Debug.WriteLine("ReferenceEquals failed for " + oObject.DebugID);

            //// We should update the cached Object from the given instance.
            //// Note that we don't just replace the Object, as that would break 
            //// existing references to other Domain Objects in the graph:
            //if (oParent.IsCollection)
            //{
            //    _ObjectMerger.MergeCollections(obj, oParent.RealObject);
            //}
            //else
            //{
            //    _ObjectMerger.MergeValues(obj, oParent.RealObject);
            //}
        }
    }

}
