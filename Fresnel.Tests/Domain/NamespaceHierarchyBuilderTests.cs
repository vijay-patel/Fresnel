﻿using Autofac;
using Envivo.Fresnel.Bootstrap;
using Envivo.Fresnel.Introspection.Assemblies;

//  Author:
//       Vijay Patel
//
// Copyright (c) 2014 Vijay Patel
//
using NUnit.Framework;
using System.Linq;

namespace Envivo.Fresnel.Tests.Domain
{
    [TestFixture()]
    public class NamespaceHierarchyBuilderTests
    {
        [Test]
        public void ShouldCreateHierarchy()
        {
            // Arrange:
            var container = new ContainerFactory().Build();

            var hierarchyBuilder = container.Resolve<NamespaceHierarchyBuilder>();

            // Act:
            var hierarchy = hierarchyBuilder.BuildTreeFor(typeof(SampleModel.Objects.PocoObject).Assembly);

            // Assert:
            Assert.IsNotNull(hierarchy);

            var productNode = hierarchy.FindNodeByName("Product");
            Assert.AreNotEqual(0, productNode.Children.Count());

            Assert.IsTrue(productNode.Children.All(c => c.IsSubClass));
        }
    }
}