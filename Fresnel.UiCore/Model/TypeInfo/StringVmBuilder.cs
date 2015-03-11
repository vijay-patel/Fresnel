﻿using Envivo.Fresnel.Configuration;
using Envivo.Fresnel.Introspection.Templates;
using Envivo.Fresnel.UiCore.Model;

using System;
using System.ComponentModel.DataAnnotations;

namespace Envivo.Fresnel.UiCore.Model.TypeInfo
{
    public class StringVmBuilder : IPropertyVmBuilder
    {
        public bool CanHandle(ISettableMemberTemplate template, Type actualType)
        {
            return actualType == typeof(char) ||
                   actualType == typeof(string);
        }

        public void Populate(SettableMemberVM targetVM, PropertyTemplate tProp, Type actualType)
        {
            var tClass = tProp.InnerClass;

            targetVM.Info = this.CreateInfoVM(tProp.Attributes, actualType);
        }

        public void Populate(SettableMemberVM targetVM, ParameterTemplate tParam, Type actualType)
        {
            var tClass = tParam.InnerClass;

            targetVM.Info = this.CreateInfoVM(tParam.Attributes, actualType);
        }

        private ITypeInfo CreateInfoVM(AttributesMap attributesMap, Type actualType)
        {
            var minLength = attributesMap.Get<MinLengthAttribute>();
            var maxLength = attributesMap.Get<MaxLengthAttribute>();
            var displayFormat = attributesMap.Get<DisplayFormatAttribute>();
            var preferredControl = attributesMap.Get<UiControlHintAttribute>().PreferredUiControl;
            if (preferredControl == UiControlType.None)
            {
                preferredControl = UiControlType.Text;
            }

            return new StringVM()
            {
                Name = "string",
                MinLength = minLength.Length,
                MaxLength = actualType == typeof(char) ? 1 : maxLength.Length,
                EditMask = displayFormat.DataFormatString,
                PreferredControl = preferredControl
            };
        }
    }
}