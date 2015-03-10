﻿using Envivo.Fresnel.Configuration;
using Envivo.Fresnel.Introspection.Templates;
using Envivo.Fresnel.UiCore.Model;

using System;

namespace Envivo.Fresnel.UiCore.Model.TypeInfo
{
    public class DateTimeVmBuilder : IPropertyVmBuilder
    {
        private readonly DateTime _epoch = new DateTime(1970, 1, 1);

        public bool CanHandle(ISettableMemberTemplate template, Type actualType)
        {
            return actualType == typeof(DateTime) ||
                   actualType == typeof(DateTimeOffset);
        }

        public void Populate(SettableMemberVM targetVM, PropertyTemplate tProp, Type actualType)
        {
            var attr = tProp.Configurations.Get<DateTimeConfiguration>();

            targetVM.Info = this.CreateInfoVM(attr);
        }

        public void Populate(SettableMemberVM targetVM, ParameterTemplate tParam, Type actualType)
        {
            var attr = tParam.Configurations.Get<DateTimeConfiguration>();

            targetVM.Info = this.CreateInfoVM(attr);
        }

        private ITypeInfo CreateInfoVM(DateTimeConfiguration attr)
        {
            return new DateTimeVM()
            {
                Name = "datetime",
                CustomFormat = attr.CustomFormat,
                PreferredControl = attr.PreferredInputControl != UiControlType.None ?
                                   attr.PreferredInputControl :
                                   UiControlType.DateTimeLocal
            };
        }
    }
}