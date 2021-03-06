using Envivo.Fresnel.DomainTypes.Interfaces;
using Envivo.Fresnel.Introspection;
using Envivo.Fresnel.Introspection.Templates;
using Envivo.Fresnel.Utils;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Envivo.Fresnel.Core.Observers
{
    /// <summary>
    /// Returns Observers for .NET objects & values
    /// </summary>
    public class ObserverRetriever
    {
        private ObserverCache _ObserverCache;
        private Lazy<IPersistenceService> _PersistenceService;

        public ObserverRetriever
        (
            ObserverCache observerCache,
            Lazy<IPersistenceService> persistenceService
        )
        {
            _ObserverCache = observerCache;
            _PersistenceService = persistenceService;
        }

        public ObjectObserver GetObserverById(Guid id)
        {
            var result = _ObserverCache.GetObserverById(id);
            return result;
        }

        /// <summary>
        /// Returns an ObjectObserver from the cache for the given Domain object
        /// </summary>
        /// <param name="obj"></param>
        public BaseObjectObserver GetObserver(object obj)
        {
            var result = _ObserverCache.GetObserver(obj);

            this.UpdatePersistentStatus(result as ObjectObserver); 
            
            return result;
        }

        /// <summary>
        /// Returns an Observer from the cache for the given object Type.
        /// </summary>
        /// <param name="obj">The Object to be observed</param>
        /// <param name="objectType">The Type of the Object to be observed</param>
        public BaseObjectObserver GetObserver(object obj, Type objectType)
        {
            var result = _ObserverCache.GetObserver(obj, objectType);

            this.UpdatePersistentStatus(result as ObjectObserver);

            return result;
        }

        /// <summary>
        /// Returns an Observer for a Domain Service. Note that DomainServices are stateless, therefore must be injected whenever this Observer is used.
        /// </summary>
        /// <param name="domainServiceType"></param>
        /// <returns></returns>
        public BaseObjectObserver GetServiceObserver(Type domainServiceType, IDomainService domainService)
        {
            var result = _ObserverCache.GetServiceObserver(domainServiceType);

            // As the DomainService is 'instance per request', we need to associate it with the pinned observer:
            result.SetRealObject(domainService);

            return result;
        }

        public BaseObjectObserver GetValueObserver(string value, Type valueType)
        {
            var result = _ObserverCache.GetValueObserver(value, valueType);
            return result;
        }

        public IEnumerable<ObjectObserver> GetAllObservers()
        {
            return _ObserverCache.GetAllObservers();
        }

        private void UpdatePersistentStatus(ObjectObserver oObject)
        {
            if (oObject == null)
                return;

            if (oObject.ChangeTracker.IsPersistent)
                // Looks like we've already determined the status:
                return;

            if (!_PersistenceService.Value.IsTypeRecognised(oObject.Template.RealType))
                return;

            if (_PersistenceService.Value.IsPersistent(oObject.ID, oObject.RealObject))
            {
                oObject.MarkAsPersistent();
            }
            else
            {
                oObject.MarkAsTransient();
            }
        }
    }
}