﻿using Envivo.Fresnel.UiCore.Model;

using System.Collections.Generic;
using T4TS;

namespace Envivo.Fresnel.UiCore.Model.Changes
{
    [TypeScriptInterface]
    public class ModificationsVM
    {
        public IEnumerable<ObjectVM> NewObjects { get; set; }

        public IEnumerable<PropertyChangeVM> PropertyChanges { get; set; }

        public IEnumerable<ObjectTitleChangeVM> ObjectTitleChanges { get; set; }

        public IEnumerable<CollectionElementVM> CollectionAdditions { get; set; }

        public IEnumerable<CollectionElementVM> CollectionRemovals { get; set; }

        public IEnumerable<ParameterChangeVM> MethodParameterChanges { get; set; }

        public IEnumerable<ObjectVM> SavedObjects { get; set; }
    }
}