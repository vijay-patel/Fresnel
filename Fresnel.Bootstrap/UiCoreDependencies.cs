﻿using Autofac;
using Envivo.Fresnel.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Envivo.Fresnel.Bootstrap
{
    public class UiCoreDependencies : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterTypes(this.GetSingleInstanceTypes())
                    .SingleInstance()
                    .PropertiesAutowired(PropertyWiringOptions.AllowCircularDependencies);

            builder.RegisterTypes(this.GetPerDependencyInstanceTypes())
                    .InstancePerDependency()
                    .PropertiesAutowired(PropertyWiringOptions.AllowCircularDependencies);

            builder.RegisterType<SystemClock>().As<IClock>()
                    .SingleInstance()
                    .PropertiesAutowired(PropertyWiringOptions.AllowCircularDependencies);

        }

        private Type[] GetSingleInstanceTypes()
        {
            return new Type[] { 
                typeof(Fresnel.UiCore.Commands.GetClassHierarchyCommand),
                typeof(Fresnel.UiCore.Commands.CreateCommand),
                typeof(Fresnel.UiCore.Commands.GetPropertyCommand),
                typeof(Fresnel.UiCore.Commands.InvokeMethodCommand),
                typeof(Fresnel.UiCore.Commands.ModificationsBuilder),

                typeof(Fresnel.UiCore.Classes.ClassItemBuilder),
                typeof(Fresnel.UiCore.Classes.NamespacesBuilder),

                typeof(Fresnel.UiCore.Types.BooleanVmBuilder),
                typeof(Fresnel.UiCore.Types.DateTimeVmBuilder),
                typeof(Fresnel.UiCore.Types.EnumVmBuilder),
                typeof(Fresnel.UiCore.Types.NumberVmBuilder),
                typeof(Fresnel.UiCore.Types.StringVmBuilder),
                typeof(Fresnel.UiCore.Types.ObjectSelectionVmBuilder),
                typeof(Fresnel.UiCore.Types.UnknownVmBuilder),

                typeof(Fresnel.UiCore.Controllers.SessionController),
                typeof(Fresnel.UiCore.SessionVmBuilder),
            };
        }

        private Type[] GetPerDependencyInstanceTypes()
        {
            return new Type[] { 
                typeof(Fresnel.UiCore.Controllers.ToolboxController),
                typeof(Fresnel.UiCore.Controllers.ExplorerController),
                typeof(Fresnel.UiCore.Controllers.TestController),

                typeof(Fresnel.UiCore.Objects.AbstractObjectVMBuilder),
                typeof(Fresnel.UiCore.Objects.AbstractPropertyVmBuilder),
                typeof(Fresnel.UiCore.Objects.MethodVmBuilder),
            };
        }

    }
}
