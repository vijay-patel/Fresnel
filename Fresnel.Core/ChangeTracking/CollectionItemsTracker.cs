using Envivo.Fresnel.Core.Observers;
using Envivo.Fresnel.DomainTypes;
using Envivo.Fresnel.DomainTypes.Interfaces;
using Envivo.Fresnel.Utils;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Envivo.Fresnel.Core.ChangeTracking
{
    /// <summary>
    /// Tracks changes made to a Collection
    /// </summary>
    internal class CollectionItemsTracker : IDisposable
    {
        private CollectionObserver _oCollection = null;
        private List<CollectionAdd> _Additions = new List<CollectionAdd>();
        private List<CollectionRemove> _Removals = new List<CollectionRemove>();

        internal CollectionItemsTracker(CollectionObserver oCollection)
        {
            _oCollection = oCollection;

            this.PreviousItems = new List<object>();
            this.LatestItems = new List<object>();
        }

        /// <summary>
        /// All additions made to the collection during the session
        /// </summary>
        internal IEnumerable<CollectionAdd> Additions
        {
            get { return _Additions; }
        }

        /// <summary>
        /// All removals made to the collection during the session
        /// </summary>
        internal IEnumerable<CollectionRemove> Removals
        {
            get { return _Removals; }
        }

        private IEnumerable<object> PreviousItems { get; set; }

        private IEnumerable<object> LatestItems { get; set; }

        internal void DetermineInitialState()
        {
            // NB: Use array to ensure we have a *copy* of the contents:
            this.LatestItems = _oCollection.GetItems().Cast<object>().ToArray();
        }

        internal Exception DetectChanges()
        {
            var veryLatestItems = _oCollection.GetItems().Cast<object>();

            var addedItems = veryLatestItems.Except(this.LatestItems).ToArray();
            var removedItems = this.LatestItems.Except(veryLatestItems).ToArray();

            this.PreviousItems = this.LatestItems;
            // NB: Use array to ensure we have a *copy* of the contents:
            this.LatestItems = veryLatestItems.ToArray();

            if (!addedItems.Any() && !removedItems.Any())
            {
                return new CoreException("The collection has not changed");
            }

            var addedChanges = addedItems.Select(a => new CollectionAdd()
                {
                    Sequence = SequentialIdGenerator.Next,
                    Collection = _oCollection,
                    Element = a,
                });
            _Additions.AddRange(addedChanges);

            var removedChanges = removedItems.Select(a => new CollectionRemove()
            {
                Sequence = SequentialIdGenerator.Next,
                Collection = _oCollection,
                Element = a,
            });
            _Removals.AddRange(removedChanges);

            return null;
        }

        public void Reset()
        {
            _Additions.Clear();
            _Removals.Clear();

            var items = _oCollection.GetItems();
            this.PreviousItems = this.LatestItems = items.Cast<object>();
        }

        public void Dispose()
        {
            _Additions.ClearSafely();
            _Additions = null;

            _Removals.ClearSafely();
            _Removals = null;

            _oCollection = null;
        }
    }
}