using System;
using System.Collections.Generic;
using System.Reflection;
using Envivo.Fresnel.Utils;
using Envivo.Fresnel.Introspection.Configuration;

namespace Envivo.Fresnel.Introspection.Templates
{

    public class EnumTemplateMapBuilder
    {
        private EnumTemplateBulider _EnumTemplateBulider;
        private AttributesMapBuilder _AttributesMapBuilder;

        public EnumTemplateMapBuilder
        (
            EnumTemplateBulider enumTemplateBulider,
            AttributesMapBuilder attributesMapBuilder
        )
        {
            _EnumTemplateBulider = enumTemplateBulider;
            _AttributesMapBuilder = attributesMapBuilder;
        }

        public EnumTemplateMap BuildFrom(ClassTemplate tClass, IClassConfiguration classConfiguration)
        {
            var nestedTypes = tClass.RealObjectType.GetNestedTypes(BindingFlags.Public | BindingFlags.DeclaredOnly);

            var results = new Dictionary<Type, EnumTemplate>();

            foreach (var nestedType in nestedTypes)
            {
                if (nestedType.IsEnum)
                {
                    var enumAttributes = _AttributesMapBuilder.BuildFor(nestedType, tClass.Configuration);
                    var tEnum = _EnumTemplateBulider.BuildFor(nestedType, enumAttributes);
                    results.Add(nestedType, tEnum);
                }
            }

            return new EnumTemplateMap(results);
        }

    }

}
