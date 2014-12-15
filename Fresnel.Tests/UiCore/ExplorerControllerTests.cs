﻿//  Author:
//       Vijay Patel
//
// Copyright (c) 2014 Vijay Patel
//
using NUnit.Framework;
using Autofac;
using System;
using System.Linq;
using Envivo.Fresnel.Introspection.Templates;
using Envivo.Fresnel;
using Envivo.Fresnel.Bootstrap;
using Envivo.Fresnel.Configuration;
using Envivo.Fresnel.Introspection;
using Envivo.Fresnel.Introspection.Assemblies;
using System.Reflection;
using System.Collections.Generic;
using Envivo.Fresnel.Core.Proxies;
using Envivo.Fresnel.Proxies;
using System.ComponentModel;
using Envivo.Fresnel.DomainTypes;
using Envivo.Fresnel.UiCore.Controllers;
using Envivo.Fresnel.UiCore.Objects;

namespace Envivo.Fresnel.Tests.Proxies
{
    [TestFixture()]
    public class ExplorerControllerTests
    {

        [Test()]
        public void ShouldReturnObjectProperty()
        {
            // Arrange:
            var container = new ContainerFactory().Build();
            var proxyCache = container.Resolve<ProxyCache>();
            var controller = container.Resolve<ExplorerController>();

            var poco = new SampleModel.Objects.PocoObject();
            poco.ID = Guid.NewGuid();
            var pocoProxy = proxyCache.GetProxy(poco);

            var propertyVM = new PropertyVM()
            {
                 ObjectID = poco.ID,
                 PropertyName = "ChildObjects"
            };

            // Act:
            var getResult = controller.GetObjectProperty(propertyVM);

            // Assert:
            Assert.IsTrue(getResult.Passed);
            Assert.IsNotNull(getResult.ReturnValue);
        }

    }

}
