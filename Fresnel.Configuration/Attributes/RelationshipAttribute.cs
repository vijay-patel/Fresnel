using Envivo.Fresnel.Configuration;
using Envivo.Fresnel.DomainTypes.Interfaces;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace System.ComponentModel.DataAnnotations
{

    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Parameter)]
    public class RelationshipAttribute : Attribute
    {
        public RelationshipAttribute()
        {
            this.Type = RelationshipType.Has;
        }

        public RelationshipAttribute(RelationshipType type)
        {
            this.Type = type;
        }

        public RelationshipType Type { get; set; }
    }
}