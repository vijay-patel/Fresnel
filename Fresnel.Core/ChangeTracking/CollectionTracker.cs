using Envivo.Fresnel.Core.Observers;
using Envivo.Fresnel.Utils;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Envivo.Fresnel.Core.ChangeTracking
{
    /// <summary>
    /// Tracks changes made to a Collection
    /// </summary>
    public class CollectionTracker : ObjectTracker
    {
        private List<CollectionAdd> _AddedItems = new List<CollectionAdd>();
        private List<CollectionRemove> _RemovedItems = new List<CollectionRemove>();

        private CollectionItemsTracker _CollectionItemsTracker;

        public CollectionTracker
            (
            OuterObjectsIdentifier outerObjectsIdentifier,
            CollectionObserver oCollection
            )
            : base(outerObjectsIdentifier, oCollection)
        {
        }

        internal override void FinaliseConstruction()
        {
            base.FinaliseConstruction();

            _CollectionItemsTracker = new CollectionItemsTracker((CollectionObserver)_oObject);
            _CollectionItemsTracker.DetermineInitialState();
        }

        public IEnumerable<CollectionAdd> AddedItems
        {
            get { return _AddedItems; }
        }

        public IEnumerable<CollectionRemove> RemovedItems
        {
            get { return _RemovedItems; }
        }

        internal void MarkAsAdded(ObjectObserver oAddedItem)
        {
            var latestChange = new CollectionAdd
            {
                Sequence = SequentialIdGenerator.Next,
                Collection = (CollectionObserver)_oObject,
                Element = oAddedItem,
            };

            _AddedItems.Add(latestChange);
            this.AddDirtyObject(oAddedItem);
        }

        internal void MarkAsRemoved(ObjectObserver oRemovedItem)
        {
            if (oRemovedItem.ChangeTracker.IsTransient)
            {
                // If the transient item was previously added, the database doesn't need to know about it:
                var previouslyAddedItem = _AddedItems.SingleOrDefault(e => e.Element == oRemovedItem);
                if (previouslyAddedItem != null)
                {
                    _AddedItems.Remove(previouslyAddedItem);
                }
                this.RemoveFromDirtyObjectGraph(oRemovedItem);
            }
            else
            {
                var latestChange = new CollectionRemove
                {
                    Sequence = SequentialIdGenerator.Next,
                    Collection = (CollectionObserver)_oObject,
                    Element = oRemovedItem,
                };

                _RemovedItems.Add(latestChange);
                this.AddDirtyObject(oRemovedItem);
            }
        }

        public override bool IsDirty
        {
            get
            {
                if (_AddedItems.Any() || _RemovedItems.Any())
                    return true;

                return base.IsDirty;
            }
            set
            {
                throw new InvalidOperationException();
            }
        }

        internal override void RemoveFromDirtyObjectGraph(ObjectObserver oObject)
        {
            //System.Diagnostics.Debug.WriteLine("Dirty graph remove : " + oObject.DebugID + " from " + _oObject.DebugID);
            base.RemoveFromDirtyObjectGraph(oObject);

            var previouslyAddedItem = _AddedItems.SingleOrDefault(e => e.Element == oObject);
            if (previouslyAddedItem != null)
            {
                _AddedItems.Remove(previouslyAddedItem);
            }

            var previouslyRemovedItem = _RemovedItems.SingleOrDefault(e => e.Element == oObject);
            if (previouslyRemovedItem != null)
            {
                _RemovedItems.Remove(previouslyRemovedItem);
            }
        }

        public override void DetectChanges()
        {
            //// Skip If this belongs to a property that has NOT been lazy loaded yet:
            //if (_oObject.OuterProperties.All(p => p.IsLazyLoadPending))
            //    return;

            base.DetectChanges();

            if (_CollectionItemsTracker != null &&
                _CollectionItemsTracker.DetectChanges() == null)
            {
                // What to do here?

                //// NB: Use the property setter, so that the dirty status is cascaded:
                //this.IsDirty = true;
            }
        }

        internal override void ResetDirtyFlags()
        {
            base.ResetDirtyFlags();

            _AddedItems.Clear();
            _RemovedItems.Clear();
        }

        public IEnumerable<CollectionAdd> GetCollectionAdditionsSince(long startedAt)
        {
            var results = _CollectionItemsTracker.Additions
                            .Where(a => a.Sequence > startedAt) // Skip/Take might be quicker
                            .ToArray();

            return results;
        }

        public IEnumerable<CollectionRemove> GetCollectionRemovalsSince(long startedAt)
        {
            var results = _CollectionItemsTracker.Removals
                            .Where(a => a.Sequence > startedAt) // Skip/Take might be quicker
                            .ToArray();

            return results;
        }

        public override void Dispose()
        {
            _AddedItems.ClearSafely();
            _AddedItems = null;

            _RemovedItems.ClearSafely();
            _RemovedItems = null;

            _CollectionItemsTracker.DisposeSafely();
            _CollectionItemsTracker = null;

            base.Dispose();
        }
    }
}