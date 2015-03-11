using Envivo.Fresnel.Configuration;
using Envivo.Fresnel.DomainTypes.Interfaces;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace System.ComponentModel.DataAnnotations
{

    /// <summary>
    /// This object owns (Composition) the properties content/s
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Parameter)]
    public class DisplayBooleanAttribute : Attribute
    {
        public DisplayBooleanAttribute()
        {
            this.TrueName = "Yes";
            this.FalseName = "Yes";
        }

        public string TrueName { get; set; }

        public string FalseName { get; set; }
    }
}