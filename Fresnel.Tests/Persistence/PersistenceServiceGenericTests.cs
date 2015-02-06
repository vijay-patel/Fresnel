﻿using Autofac;
using Envivo.Fresnel.Bootstrap;
using Envivo.Fresnel.Core.Observers;
using Envivo.Fresnel.Core.Persistence;
using Envivo.Fresnel.Introspection;
using Envivo.Fresnel.Introspection.IoC;
using Envivo.Fresnel.SampleModel.Objects;
using Envivo.Fresnel.UiCore.Commands;
using Envivo.Fresnel.UiCore.Controllers;
using Envivo.Fresnel.UiCore.Model;
using Fresnel.SampleModel.Persistence;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Envivo.Fresnel.Tests.Persistence
{
    [TestFixture()]
    public class PersistenceServiceGenericTests
    {
        [Test]
        public void ShouldCreateCompleteAggregate()
        {
            // Arrange:
            var customDependencyModules = new Autofac.Module[] { new CustomDependencyModule() };
            var container = new ContainerFactory().Build(customDependencyModules);

            var engine = container.Resolve<Core.Engine>();
            engine.RegisterDomainAssembly(typeof(SampleModel.IDummy).Assembly);

            var persistenceService = container.Resolve<IPersistenceService>();

            // Act:
            var poco = persistenceService.CreateObject<PocoObject>();
            poco.ID = Guid.NewGuid();
            poco.AddSomeChildObjects();

            var savedChanges = persistenceService.SaveChanges();

            // Assert:
            Assert.IsNotNull(poco);
            Assert.IsTrue(savedChanges > 1);
        }

        [Test]
        public void ShouldModifyChildCollection()
        {
            // Arrange:
            var customDependencyModules = new Autofac.Module[] { new CustomDependencyModule() };
            var container = new ContainerFactory().Build(customDependencyModules);

            var engine = container.Resolve<Core.Engine>();
            engine.RegisterDomainAssembly(typeof(SampleModel.IDummy).Assembly);

            var persistenceService = container.Resolve<IPersistenceService>();

            // Act:
            var poco = persistenceService.CreateObject<PocoObject>();
            poco.ID = Guid.NewGuid();
            poco.AddSomeChildObjects();

            // Step 1:
            var savedChanges1 = persistenceService.SaveChanges();

            // Step 2:
            poco.ChildObjects.Remove(poco.ChildObjects.First());
            var savedChanges2 = persistenceService.SaveChanges();

            // Assert:
            Assert.IsTrue(savedChanges1 > 1);
            Assert.IsTrue(savedChanges2 > 0);

            var persistedPoco = persistenceService.GetObject<PocoObject>(poco.ID);
            var differences = poco.ChildObjects.Except(persistedPoco.ChildObjects);
            Assert.AreEqual(0, differences.Count());
        }

        [Test]
        public void ShouldCreateBiDirectionalLinks()
        {
            // Arrange:
            var customDependencyModules = new Autofac.Module[] { new CustomDependencyModule() };
            var container = new ContainerFactory().Build(customDependencyModules);

            var engine = container.Resolve<Core.Engine>();
            engine.RegisterDomainAssembly(typeof(SampleModel.IDummy).Assembly);

            var persistenceService = container.Resolve<IPersistenceService>();

            // Act:
            var master = persistenceService.CreateObject<MasterObject>();
            master.ID = Guid.NewGuid();

            var detail = persistenceService.CreateObject<DetailObject>();
            detail.ID = Guid.NewGuid();

            master.Children.Add(detail);

            var savedChanges = persistenceService.SaveChanges();

            // Assert:
            Assert.AreEqual(3, savedChanges);

            var persistedA = persistenceService.GetObject<MasterObject>(master.ID);
            var persistedB = persistenceService.GetObject<DetailObject>(detail.ID);

            Assert.IsTrue(persistedA.Children.Contains(persistedB));
            Assert.AreEqual(persistedB.Parent, persistedA);
        }

        [Test]
        public void ShouldGetAll()
        {
            // Arrange:
            var customDependencyModules = new Autofac.Module[] { new CustomDependencyModule() };
            var container = new ContainerFactory().Build(customDependencyModules);

            var engine = container.Resolve<Core.Engine>();
            engine.RegisterDomainAssembly(typeof(SampleModel.IDummy).Assembly);

            var persistenceService = container.Resolve<IPersistenceService>();

            for (var i = 0; i < 5; i++)
            {
                var poco = persistenceService.CreateObject<PocoObject>();
                poco.ID = Guid.NewGuid();
                poco.AddSomeChildObjects();

                var savedChanges = persistenceService.SaveChanges();
            }

            // Act:
            var pocos = persistenceService.GetObjects<PocoObject>();

            // Assert:
            Assert.AreNotEqual(0, pocos.OfType<PocoObject>().Count());
        }

    }
}