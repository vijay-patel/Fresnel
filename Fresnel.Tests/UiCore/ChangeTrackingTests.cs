﻿using Autofac;
using Envivo.Fresnel.Bootstrap;
using Envivo.Fresnel.Core.Observers;
using Envivo.Fresnel.Introspection;
using Envivo.Fresnel.UiCore.Commands;
using Envivo.Fresnel.UiCore.Controllers;
using Envivo.Fresnel.UiCore.Objects;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Envivo.Fresnel.Tests.Proxies
{
    [TestFixture()]
    public class ProxyChangeTrackingTests
    {
        [Test()]
        public void ShouldReturnCollectionAdditions()
        {
            // Arrange:
            var container = new ContainerFactory().Build();
            var observerCache = container.Resolve<ObserverCache>();
            var templateCache = container.Resolve<TemplateCache>();
            var controller = container.Resolve<ExplorerController>();

            var poco = new SampleModel.Objects.PocoObject();
            poco.ID = Guid.NewGuid();
            var oObject = observerCache.GetObserver(poco) as ObjectObserver;

            // This ensures the Collection can be tracked:
            var getRequest = new GetPropertyRequest()
            {
                ObjectID = poco.ID,
                PropertyName = "ChildObjects"
            };
            var getResult = controller.GetObjectProperty(getRequest);

            // Act:
            var invokeRequest = new InvokeMethodRequest()
            {
                ObjectID = poco.ID,
                MethodName = "AddSomeChildObjects",
            };

            var invokeResult1 = controller.InvokeMethod(invokeRequest);

            var invokeResult2 = controller.InvokeMethod(invokeRequest);

            var invokeResult3 = controller.InvokeMethod(invokeRequest);

            // Assert:
            // Each call performs 3 new additions:
            Assert.AreEqual(3, invokeResult1.Modifications.CollectionAdditions.Count());
            Assert.AreEqual(3, invokeResult2.Modifications.CollectionAdditions.Count());
            Assert.AreEqual(3, invokeResult3.Modifications.CollectionAdditions.Count());
        }

        [Test()]
        public void ShouldReturnNewlyCreatedObservers()
        {
            // Arrange:
            var container = new ContainerFactory().Build();
            var observerCache = container.Resolve<ObserverCache>();
            var templateCache = container.Resolve<TemplateCache>();
            var controller = container.Resolve<ExplorerController>();

            var poco = new SampleModel.Objects.PocoObject();
            poco.ID = Guid.NewGuid();
            var oObject = observerCache.GetObserver(poco) as ObjectObserver;

            // This ensures the Collection can be tracked:
            var getRequest = new GetPropertyRequest()
            {
                ObjectID = poco.ID,
                PropertyName = "ChildObjects"
            };
            var getResult = controller.GetObjectProperty(getRequest);

            // Act:
            var invokeRequest = new InvokeMethodRequest()
            {
                ObjectID = poco.ID,
                MethodName = "AddSomeChildObjects",
            };

            var invokeResult = controller.InvokeMethod(invokeRequest);

            // Assert:
            Assert.AreEqual(3, invokeResult.Modifications.NewObjects.Count());
        }

        [Test()]
        public void ShouldReturnPopulatedCollectionProperty()
        {
            // Arrange:
            var container = new ContainerFactory().Build();
            var observerCache = container.Resolve<ObserverCache>();
            var templateCache = container.Resolve<TemplateCache>();
            var controller = container.Resolve<ExplorerController>();

            var poco = new SampleModel.Objects.PocoObject();
            poco.ID = Guid.NewGuid();
            var oObject = observerCache.GetObserver(poco) as ObjectObserver;

            poco.AddSomeChildObjects();

            // Act:
            var request = new GetPropertyRequest()
            {
                ObjectID = poco.ID,
                PropertyName = "ChildObjects"
            };

            var getResult = controller.GetObjectProperty(request);

            // Assert:
            Assert.IsTrue(getResult.Passed);

            var collectionVM = getResult.ReturnValue as CollectionVM;
            Assert.IsNotNull(collectionVM);

            Assert.AreNotEqual(0, collectionVM.Items.Count());
        }

        [Test()]
        public void ShouldSetNonReferenceProperties()
        {
            // Arrange:
            var container = new ContainerFactory().Build();
            var observerCache = container.Resolve<ObserverCache>();
            var templateCache = container.Resolve<TemplateCache>();
            var controller = container.Resolve<ExplorerController>();

            var poco = new SampleModel.BasicTypes.MultiType();
            var oObject = observerCache.GetObserver(poco) as ObjectObserver;

            // Act:
            var requests = new List<SetPropertyRequest>()
            {
                new SetPropertyRequest() { ObjectID = oObject.ID, PropertyName = "A_Char", NonReferenceValue = "X" },
                new SetPropertyRequest() { ObjectID = oObject.ID, PropertyName = "A_Double", NonReferenceValue = "123.45" },
                new SetPropertyRequest() { ObjectID = oObject.ID, PropertyName = "An_Int", NonReferenceValue = "1234" },
                new SetPropertyRequest() { ObjectID = oObject.ID, PropertyName = "A_String", NonReferenceValue = "ABC123" },
            };

            foreach (var request in requests)
            {
                var setResult = controller.SetProperty(request);
                Assert.AreEqual(1, setResult.Modifications.PropertyChanges.Count());
            }
        }

        [Test()]
        public void ShouldReturnPropertyModifications()
        {
            // Arrange:
            var container = new ContainerFactory().Build();
            var observerCache = container.Resolve<ObserverCache>();
            var templateCache = container.Resolve<TemplateCache>();
            var controller = container.Resolve<ExplorerController>();

            var poco = new SampleModel.Objects.PocoObject();
            poco.ID = Guid.NewGuid();
            var oObject = observerCache.GetObserver(poco) as ObjectObserver;

            // Act:
            var request = new SetPropertyRequest()
            {
                ObjectID = poco.ID,
                PropertyName = "NormalText",
                NonReferenceValue = "1234"
            };

            var setResult = controller.SetProperty(request);

            // Assert:
            Assert.AreEqual(1, setResult.Modifications.PropertyChanges.Count());
        }

        [Test]
        public void ShouldDetectIntraPropertyCalls()
        {
            // Arrange:
            var container = new ContainerFactory().Build();
            var observerCache = container.Resolve<ObserverCache>();
            var templateCache = container.Resolve<TemplateCache>();
            var controller = container.Resolve<ExplorerController>();

            var poco = new SampleModel.BasicTypes.TextValues();
            poco.ID = Guid.NewGuid();
            var oObject = observerCache.GetObserver(poco) as ObjectObserver;

            // Act:
            var request = new SetPropertyRequest()
            {
                ObjectID = poco.ID,
                PropertyName = "TextWithMaximumSize",
                NonReferenceValue = "1234"
            };

            var setResult = controller.SetProperty(request);

            // Assert:
            // All of the text properties are bound to the same value:
            Assert.AreEqual(7, setResult.Modifications.PropertyChanges.Count());
        }
    }
}