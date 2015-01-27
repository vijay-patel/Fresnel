﻿using Envivo.Fresnel.Core.ChangeTracking;
using Envivo.Fresnel.Core.Observers;
using Envivo.Fresnel.Introspection;
using System;
using System.Linq;

namespace Envivo.Fresnel.Core.Commands
{
    public class InvokeMethodCommand
    {
        private DirtyObjectNotifier _DirtyObjectNotifier;
        private ObserverCache _ObserverCache;
        private ObserverCacheSynchroniser _ObserverCacheSynchroniser;
        private Fresnel.Introspection.Commands.InvokeMethodCommand _InvokeCommand;
        private RealTypeResolver _RealTypeResolver;

        public InvokeMethodCommand
            (
            ObserverCache observerCache,
            ObserverCacheSynchroniser observerCacheSynchroniser,
            DirtyObjectNotifier dirtyObjectNotifier,
            Fresnel.Introspection.Commands.InvokeMethodCommand invokeCommand,
            RealTypeResolver realTypeResolver
            )
        {
            _ObserverCache = observerCache;
            _ObserverCacheSynchroniser = observerCacheSynchroniser;
            _DirtyObjectNotifier = dirtyObjectNotifier;
            _InvokeCommand = invokeCommand;
            _RealTypeResolver = realTypeResolver;
        }

        public BaseObjectObserver Invoke(MethodObserver oMethod, object targetObject)
        {
            if (oMethod.Parameters.AreRequired &&
                !oMethod.Parameters.IsComplete)
            {
                throw new ArgumentException("One or more Parameters has not been set for this method");
            }

            // TODO: Check permissions

            try
            {
                var args = oMethod.Parameters.Values.Select(p => p.Value);

                // NB: Always use TargetObject instead of oMethod.OuterObject.RealObject
                //     to ensure proxied members are intercepted:
                var result = _InvokeCommand.Invoke(targetObject, oMethod.Template, args);

                if (result == null)
                    return null;

                var resultType = _RealTypeResolver.GetRealType(result);
                var oResult = _ObserverCache.GetObserver(result, resultType);
                return oResult;
            }
            finally
            {
                // Make sure we know of any changes in the object graph:
                _ObserverCacheSynchroniser.SyncAll();

                // Reset the parameters so that the method doesn't accidentally get invoked twice in succession:
                oMethod.Parameters.Reset();
            }
        }
    }
}