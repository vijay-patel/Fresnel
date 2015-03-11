﻿using Envivo.Fresnel.Configuration;
using Envivo.Fresnel.Introspection.Templates;
using Envivo.Fresnel.UiCore.Model;

using System;
using System.ComponentModel.DataAnnotations;

namespace Envivo.Fresnel.UiCore.Model.TypeInfo
{
    public class NumberVmBuilder : IPropertyVmBuilder
    {
        public bool CanHandle(ISettableMemberTemplate template, Type actualType)
        {
            return actualType == typeof(double) ||
                   actualType == typeof(float) ||
                   actualType == typeof(decimal) ||
                   actualType == typeof(Int16) ||
                   actualType == typeof(Int32) ||
                   actualType == typeof(Int64) ||
                   actualType == typeof(byte);
        }

        public void Populate(SettableMemberVM targetVM, PropertyTemplate tProp, Type actualType)
        {
            targetVM.Info = this.CreateInfoVM(tProp.Attributes);
        }

        public void Populate(SettableMemberVM targetVM, ParameterTemplate tParam, Type actualType)
        {
            targetVM.Info = this.CreateInfoVM(tParam.Attributes);
        }

        private ITypeInfo CreateInfoVM(AttributesMap attributesMap)
        {
            var range = attributesMap.Get<RangeAttribute>();
            var decimalPlaces = attributesMap.Get<DecimalPlacesAttribute>();
            var dataType = attributesMap.Get<DataTypeAttribute>();

            var preferredControl = attributesMap.Get<UiControlHintAttribute>().PreferredUiControl;
            if (dataType.DataType == DataType.Currency)
            {
                preferredControl = UiControlType.Currency;
            }
            else if (preferredControl == UiControlType.None)
            {
                preferredControl = UiControlType.Number;
            }

            return new NumberVM()
            {
                Name = "number",
                MinValue = Convert.ToInt32(range.Minimum),
                MaxValue = Convert.ToInt32(range.Maximum),
                DecimalPlaces = decimalPlaces.Places,
                PreferredControl = preferredControl
            };
        }
    }
}