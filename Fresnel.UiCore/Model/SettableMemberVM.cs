﻿using Envivo.Fresnel.UiCore.Model.TypeInfo;
using System;
using T4TS;

namespace Envivo.Fresnel.UiCore.Model
{
    /// <summary>
    /// Represents a Property or Parameter Value in the UI
    /// </summary>
    [TypeScriptInterface]
    public class SettableMemberVM : BaseViewModel
    {
        public Guid? ObjectID { get; set; }

        public int Index { get; set; }

        public string InternalName { get; set; }

        public bool IsRequired { get; set; }

        public bool IsLoaded { get; set; }

        public bool IsObject { get; set; }

        public bool IsCollection { get; set; }

        public bool IsNonReference { get; set; }

        public ITypeInfo Info { get; set; }

        public ValueStateVM State { get; set; }
    }
}