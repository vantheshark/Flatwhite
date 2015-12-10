using System;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Hosting;
using Flatwhite.WebApi;
using NSubstitute;
using NUnit.Framework;

namespace Flatwhite.Tests.WebApi
{
    [TestFixture]
    public class WebApiPhoenixTests
    {
        private readonly DummyController _controllerIntance = new DummyController();
        private static readonly Type ControllerType = typeof (DummyController);
        private HttpRequestMessage _requestMessage = UnitTestHelper.GetMessage();

        [SetUp]
        public void SetUp()
        {
            Global.Init();
            WebApiExtensions._dependencyResolverActivator = new ServiceActivator();
            _requestMessage = UnitTestHelper.GetMessage();
        }

        [TestCase(nameof(DummyController.HttpActionResult), 4)]
        [TestCase(nameof(DummyController.HttpResponseMessageAsync), 4)]
        [TestCase(nameof(DummyController.HttpResponseMessage), 4)]
        [TestCase(nameof(DummyController.Object), 36)]
        [TestCase(nameof(DummyController.String), 6)]
        [TestCase(nameof(DummyController.StringAsync), 6)]
        [TestCase(nameof(DummyController.Void), -1, ExpectedException = typeof(NotSupportedException), ExpectedMessage = "void method is not supported")]
        [TestCase(nameof(DummyController.VoidAsync), -1, ExpectedException = typeof(NotSupportedException), ExpectedMessage = "async void method is not supported")]
        public async Task should_execute_the_controller_method_and_return_CacheItem(string actionMethodName, int contentLength)
        {
            // Arrange
            var currentCacheItem = new WebApiCacheItem();
            var invocation = Substitute.For<_IInvocation>();
            invocation.Arguments.Returns(new object[0]);
            invocation.Method.Returns(ControllerType.GetMethod(actionMethodName, BindingFlags.Instance | BindingFlags.Public));

            var phoenix = new WebApiPhoenixWithPublicMethods(invocation, currentCacheItem, _requestMessage, new JsonMediaTypeFormatter());

            // Action
            var result = await phoenix.InvokeAndGetBareResultPublic(_controllerIntance).ConfigureAwait(false);
            var cacheItem = await phoenix.GetCacheItemPublic(result).ConfigureAwait(false) as WebApiCacheItem;

            // Assert
            if (result == null)
            {
                Assert.IsTrue(actionMethodName == "Void" || actionMethodName == "VoidAsync");
            }
            else
            {
                Assert.IsNotNull(cacheItem);
                Assert.AreEqual(contentLength, cacheItem.Content.Length);
            }
        }

        [Test]
        public void GetTargetInstance_should_set_the_request_properties_but_HttpConfiguration()
        {
            SynchronizationContext.SetSynchronizationContext(new SynchronizationContext());
            var currentCacheItem = new WebApiCacheItem();
            var invocation = Substitute.For<_IInvocation>();
            invocation.Arguments.Returns(new object[0]);
            invocation.Method.Returns(ControllerType.GetMethod(nameof(DummyController.Object), BindingFlags.Instance | BindingFlags.Public));
            
            var phoenix = new WebApiPhoenixWithPublicMethods(invocation, currentCacheItem, _requestMessage, new JsonMediaTypeFormatter());

            // Action
            var controller = phoenix.GetTargetInstancePublic() as ApiController;

            // Assert
            Assert.IsNotNull(controller);
            Assert.IsNotNull(controller.Request);
            Assert.AreEqual(1, controller.Request.Headers.Count());
            Assert.AreEqual(4, controller.Request.Properties.Count()); // __created_by, ActionDescriptor, RequestContext, HttpConfiguration
        }

        [Test]
        public void GetTargetInstance_should_set_the_HttpConfiguration_if_different_to_the_one_from_controller_descriptor()
        {
            // Arrange
            var requestContext = Substitute.For<HttpRequestContext>();
            requestContext.Configuration.Returns(Substitute.For<HttpConfiguration>());
            _requestMessage.Properties[HttpPropertyKeys.RequestContextKey] = requestContext;
            SynchronizationContext.SetSynchronizationContext(new SynchronizationContext());
            var currentCacheItem = new WebApiCacheItem();
            var invocation = Substitute.For<_IInvocation>();
            invocation.Arguments.Returns(new object[0]);
            invocation.Method.Returns(ControllerType.GetMethod(nameof(DummyController.Object), BindingFlags.Instance | BindingFlags.Public));

            var phoenix = new WebApiPhoenixWithPublicMethods(invocation, currentCacheItem, _requestMessage, new JsonMediaTypeFormatter());

            // Action
            var controller = phoenix.GetTargetInstancePublic() as ApiController;

            // Assert
            Assert.IsNotNull(controller);
            Assert.IsNotNull(controller.Request);
            Assert.AreEqual(1, controller.Request.Headers.Count());
            Assert.AreEqual(5, controller.Request.Properties.Count()); // __created_by, ActionDescriptor, RequestContext, HttpConfiguration
        }

        [Test]
        public void GetTargetInstance_should_set_the_HttpConfiguration_if_null()
        {
            // Arrange
            var requestContext = Substitute.For<HttpRequestContext>();
            requestContext.Configuration.Returns((HttpConfiguration)null);
            _requestMessage.Properties[HttpPropertyKeys.RequestContextKey] = requestContext;
            SynchronizationContext.SetSynchronizationContext(new SynchronizationContext());
            var currentCacheItem = new WebApiCacheItem();
            var invocation = Substitute.For<_IInvocation>();
            invocation.Arguments.Returns(new object[0]);
            invocation.Method.Returns(ControllerType.GetMethod(nameof(DummyController.Object), BindingFlags.Instance | BindingFlags.Public));

            var phoenix = new WebApiPhoenixWithPublicMethods(invocation, currentCacheItem, _requestMessage, new JsonMediaTypeFormatter());

            // Action
            var controller = phoenix.GetTargetInstancePublic() as ApiController;

            // Assert
            Assert.IsNotNull(controller);
            Assert.IsNotNull(controller.Request);
            Assert.AreEqual(1, controller.Request.Headers.Count());
            Assert.AreEqual(5, controller.Request.Properties.Count()); // __created_by, ActionDescriptor, RequestContext, HttpConfiguration
        }


        [TestCase(null)]
        [TestCase(typeof(IHttpController))]
        public void GetTargetInstance_should_throw_if_cannot_create_controller(Type controllerType)
        {
            // Arrange
            var mockControlerDescriptor = Substitute.For<HttpControllerDescriptor>();
            mockControlerDescriptor.CreateController(Arg.Any<HttpRequestMessage>()).Returns(controllerType == null ? null : Substitute.For<IHttpController>());
            mockControlerDescriptor.Configuration = Substitute.For<HttpConfiguration>();

            _requestMessage.Properties[HttpPropertyKeys.HttpActionDescriptorKey] = new ReflectedHttpActionDescriptor { ControllerDescriptor = mockControlerDescriptor };
            var currentCacheItem = new WebApiCacheItem();
            var invocation = Substitute.For<_IInvocation>();
            invocation.Arguments.Returns(new object[0]);
            invocation.Method.Returns(ControllerType.GetMethod(nameof(DummyController.Object), BindingFlags.Instance | BindingFlags.Public));

            var phoenix = new WebApiPhoenixWithPublicMethods(invocation, currentCacheItem, _requestMessage, new JsonMediaTypeFormatter());

            // Action
            if (controllerType == null)
            {
                Assert.Throws<Exception>(() => phoenix.GetTargetInstancePublic(), "Cannot recreate controller");
            }
            else
            {
                Assert.Throws<NotSupportedException>(() => phoenix.GetTargetInstancePublic(), "controller must be ApiController");
            }
        }

        [DebuggerStepThrough]
        private class WebApiPhoenixWithPublicMethods : WebApiPhoenix
        {
            public WebApiPhoenixWithPublicMethods(_IInvocation invocation, WebApiCacheItem cacheItem, HttpRequestMessage requestMessage, MediaTypeFormatter mediaTypeFormatter = null)
                : base(invocation, cacheItem, requestMessage, mediaTypeFormatter)
            {
            }

            public Task<CacheItem> GetCacheItemPublic(object response)
            {
                return GetCacheItem(response);
            }

            public Task<object> InvokeAndGetBareResultPublic(object serviceInstance)
            {
                return InvokeAndGetBareResult(serviceInstance);
            }

            public object GetTargetInstancePublic()
            {
                return GetTargetInstance();
            }
        }
    }
}
