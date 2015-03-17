﻿using Envivo.Fresnel.Core.Observers;
using Envivo.Fresnel.Core.Permissions;
using Envivo.Fresnel.Introspection;
using Envivo.Fresnel.Introspection.Templates;
using Envivo.Fresnel.UiCore.Model;
using Envivo.Fresnel.UiCore.Model.TypeInfo;
using Envivo.Fresnel.Utils;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Envivo.Fresnel.UiCore
{
    public class AbstractParameterVmBuilder
    {
        private IEnumerable<IPropertyVmBuilder> _Builders;
        private UnknownVmBuilder _UnknownVmBuilder;

        public AbstractParameterVmBuilder
            (
            IEnumerable<IPropertyVmBuilder> builders
            )
        {
            _Builders = builders;
            _UnknownVmBuilder = builders.OfType<UnknownVmBuilder>().Single();
        }

        public SettableMemberVM BuildFor(ParameterTemplate tParam)
        {
            var valueType = tParam.InnerClass.RealType;
            var actualType = valueType.IsNullableType() ?
                               valueType.GetGenericArguments()[0] :
                               valueType;

            var paramVM = new SettableMemberVM()
            {
                IsVisible = true,
                Name = tParam.FriendlyName,
                InternalName = tParam.Name,
                Description = tParam.XmlComments.Summary,
                State = this.CreateStateFor(tParam),
                IsCollection = tParam.IsCollection,
            };

            paramVM.IsNonReference = tParam.IsNonReference;
            paramVM.IsCollection = tParam.IsCollection;
            paramVM.IsObject = !paramVM.IsNonReference && !paramVM.IsCollection;

            var vmBuilder = _Builders.SingleOrDefault(s => s.CanHandle(tParam, actualType)) ?? _UnknownVmBuilder;
            vmBuilder.Populate(paramVM, tParam, actualType);

            return paramVM;
        }

        private ValueStateVM CreateStateFor(ParameterTemplate tParam)
        {
            return new ValueStateVM()
            {
                ValueType = tParam.ParameterType.Name,
                Set = new InteractionPoint()
                {
                    IsEnabled = true,
                    IsVisible = true
                }
            };
        }

    }
}