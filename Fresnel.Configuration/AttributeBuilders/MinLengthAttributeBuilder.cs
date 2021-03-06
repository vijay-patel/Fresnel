using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Envivo.Fresnel.Utils;
using System.Collections.Generic;

namespace Envivo.Fresnel.Configuration
{
    public class MinLengthAttributeBuilder : IMissingAttributeBuilder
    {
        public bool CanHandle(Type attributeType)
        {
            return attributeType == typeof(MinLengthAttribute);
        }

        public Attribute BuildFrom(Type parentClass, Type templateType, IEnumerable<Attribute> templateAttributes)
        {
            var result = new MinLengthAttribute(0);
            return result;
        }
    }
}