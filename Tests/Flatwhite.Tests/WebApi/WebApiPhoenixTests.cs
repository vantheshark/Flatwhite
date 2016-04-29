using System;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Flatwhite.Hot;
using Flatwhite.WebApi;
using NSubstitute;
using NUnit.Framework;

namespace Flatwhite.Tests.WebApi
{
    [TestFixture]
    public class WebApiPhoenixTests
    {
        private HttpRequestMessage _requestMessage = UnitTestHelper.GetMessage();
        private IHttpClient _client;

        [SetUp]
        public void SetUp()
        {
            Global.Init();
            WebApiExtensions._fwConfig = new FlatwhiteWebApiConfiguration();
            _requestMessage = UnitTestHelper.GetMessage();
            _client = Substitute.For<IHttpClient>();
            _client
                .SendAsync(Arg.Any<HttpRequestMessage>(), HttpCompletionOption.ResponseHeadersRead)
                .Returns(Task.FromResult(new HttpResponseMessage {StatusCode = System.Net.HttpStatusCode.OK}));
        }

        [Test]
        public async Task FireAsync_should_send_a_request_to_original_endpoint_when_loopback_is_not_set()
        {
            // Arrange
            var currentCacheItem = new WebApiCacheItem
            {
                CreatedTime = DateTime.UtcNow,
                MaxAge = 2,
                StaleWhileRevalidate = 3,
                StaleIfError = 4,
                Key = "1"
            };
            var invocation = Substitute.For<_IInvocation>();


            using (var phoenix = new WebApiPhoenixWithPublicMethods(invocation, currentCacheItem, _requestMessage))
            { 
                phoenix.HttpClient = _client;
            

                // Action
                var state = await phoenix.FireAsyncPublic();

                // Assert
                Assert.IsTrue(state is InActivePhoenix);
                _client.Received(1).Timeout = Arg.Is<TimeSpan>(x => x.TotalSeconds > 4);
                await _client
                    .Received(1)
                    .SendAsync(Arg.Is<HttpRequestMessage>(msg => msg.Properties.Count == 0 && 
                                                                 msg.Headers.CacheControl.Extensions.Any(e => e.Name == WebApiExtensions.__cacheControl_flatwhite_force_refresh) &&
                                                                 msg.RequestUri.ToString() == "http://localhost/api/method/id")
                                , HttpCompletionOption.ResponseHeadersRead);
            }
        }

        [Test]
        public async Task FireAsync_should_send_a_request_to_loopback_address()
        {
            // Arrange
            WebApiExtensions._fwConfig = new FlatwhiteWebApiConfiguration
            {
                LoopbackAddress = "http://192.188.2.1:8080"
            };
            using (var phoenix = new WebApiPhoenixWithPublicMethods(Substitute.For<_IInvocation>(), new WebApiCacheItem { Key = "1" }, _requestMessage))
            {
                phoenix.HttpClient = _client;

                // Action
                await phoenix.FireAsyncPublic();

                // Assert
                await _client
                    .Received(1)
                    .SendAsync(Arg.Is<HttpRequestMessage>(msg => msg.Properties.Count == 0 &&
                                                                 msg.Headers.CacheControl.Extensions.Any(e => e.Name == WebApiExtensions.__cacheControl_flatwhite_force_refresh) &&
                                                                 msg.RequestUri.ToString() == "http://192.188.2.1:8080/api/method/id")
                                , HttpCompletionOption.ResponseHeadersRead);
            }
        }

        [Test]
        public void FireAsync_should_throw_if_request_failed()
        {
            // Arrange
            _client
                .SendAsync(Arg.Any<HttpRequestMessage>(), HttpCompletionOption.ResponseHeadersRead)
                .Returns(Task.FromResult(new HttpResponseMessage { StatusCode = System.Net.HttpStatusCode.InternalServerError }));

            using (var phoenix = new WebApiPhoenixWithPublicMethods(Substitute.For<_IInvocation>(), new WebApiCacheItem {Key = "1"}, _requestMessage))
            { 
                phoenix.HttpClient = _client;

                // Action
                Assert.Throws<HttpRequestException>(async () => { await phoenix.FireAsyncPublic(); });
            }
        }

        //[Test]
        //public void GetTargetInstance_should_set_the_request_properties_but_HttpConfiguration()
        //{
        //    SynchronizationContext.SetSynchronizationContext(new SynchronizationContext());
        //    var currentCacheItem = new WebApiCacheItem();
        //    var invocation = Substitute.For<_IInvocation>();
        //    invocation.Arguments.Returns(new object[0]);
        //    invocation.Method.Returns(ControllerType.GetMethod(nameof(DummyController.Object), BindingFlags.Instance | BindingFlags.Public));

        //    var phoenix = new WebApiPhoenixWithPublicMethods(invocation, currentCacheItem, _requestMessage);

        //    // Action
        //    var controller = phoenix.GetTargetInstancePublic() as ApiController;

        //    // Assert
        //    Assert.IsNotNull(controller);
        //    Assert.IsNotNull(controller.Request);
        //    Assert.AreEqual(1, controller.Request.Headers.Count());
        //    Assert.AreEqual(4, controller.Request.Properties.Count()); // __created_by, ActionDescriptor, RequestContext, HttpConfiguration
        //}

        //[Test]
        //public void GetTargetInstance_should_set_the_HttpConfiguration_if_different_to_the_one_from_controller_descriptor()
        //{
        //    // Arrange
        //    var requestContext = Substitute.For<HttpRequestContext>();
        //    requestContext.Configuration.Returns(Substitute.For<HttpConfiguration>());
        //    _requestMessage.Properties[HttpPropertyKeys.RequestContextKey] = requestContext;
        //    SynchronizationContext.SetSynchronizationContext(new SynchronizationContext());
        //    var currentCacheItem = new WebApiCacheItem();
        //    var invocation = Substitute.For<_IInvocation>();
        //    invocation.Arguments.Returns(new object[0]);
        //    invocation.Method.Returns(ControllerType.GetMethod(nameof(DummyController.Object), BindingFlags.Instance | BindingFlags.Public));

        //    var phoenix = new WebApiPhoenixWithPublicMethods(invocation, currentCacheItem, _requestMessage);

        //    // Action
        //    var controller = phoenix.GetTargetInstancePublic() as ApiController;

        //    // Assert
        //    Assert.IsNotNull(controller);
        //    Assert.IsNotNull(controller.Request);
        //    Assert.AreEqual(1, controller.Request.Headers.Count());
        //    Assert.AreEqual(5, controller.Request.Properties.Count()); // __created_by, ActionDescriptor, RequestContext, HttpConfiguration
        //}

        //[Test]
        //public void GetTargetInstance_should_set_the_HttpConfiguration_if_null()
        //{
        //    // Arrange
        //    var requestContext = Substitute.For<HttpRequestContext>();
        //    requestContext.Configuration.Returns((HttpConfiguration)null);
        //    _requestMessage.Properties[HttpPropertyKeys.RequestContextKey] = requestContext;
        //    SynchronizationContext.SetSynchronizationContext(new SynchronizationContext());
        //    var currentCacheItem = new WebApiCacheItem();
        //    var invocation = Substitute.For<_IInvocation>();
        //    invocation.Arguments.Returns(new object[0]);
        //    invocation.Method.Returns(ControllerType.GetMethod(nameof(DummyController.Object), BindingFlags.Instance | BindingFlags.Public));

        //    var phoenix = new WebApiPhoenixWithPublicMethods(invocation, currentCacheItem, _requestMessage);

        //    // Action
        //    var controller = phoenix.GetTargetInstancePublic() as ApiController;

        //    // Assert
        //    Assert.IsNotNull(controller);
        //    Assert.IsNotNull(controller.Request);
        //    Assert.AreEqual(1, controller.Request.Headers.Count());
        //    Assert.AreEqual(5, controller.Request.Properties.Count()); // __created_by, ActionDescriptor, RequestContext, HttpConfiguration
        //}


        //[TestCase(null)]
        //[TestCase(typeof(IHttpController))]
        //public void GetTargetInstance_should_throw_if_cannot_create_controller(Type controllerType)
        //{
        //    // Arrange
        //    var mockControlerDescriptor = Substitute.For<HttpControllerDescriptor>();
        //    mockControlerDescriptor.CreateController(Arg.Any<HttpRequestMessage>()).Returns(controllerType == null ? null : Substitute.For<IHttpController>());
        //    mockControlerDescriptor.Configuration = Substitute.For<HttpConfiguration>();

        //    _requestMessage.Properties[HttpPropertyKeys.HttpActionDescriptorKey] = new ReflectedHttpActionDescriptor { ControllerDescriptor = mockControlerDescriptor };
        //    var currentCacheItem = new WebApiCacheItem();
        //    var invocation = Substitute.For<_IInvocation>();
        //    invocation.Arguments.Returns(new object[0]);
        //    invocation.Method.Returns(ControllerType.GetMethod(nameof(DummyController.Object), BindingFlags.Instance | BindingFlags.Public));

        //    var phoenix = new WebApiPhoenixWithPublicMethods(invocation, currentCacheItem, _requestMessage);

        //    // Action
        //    if (controllerType == null)
        //    {
        //        Assert.Throws<Exception>(() => phoenix.GetTargetInstancePublic(), "Cannot recreate controller");
        //    }
        //    else
        //    {
        //        Assert.Throws<NotSupportedException>(() => phoenix.GetTargetInstancePublic(), "controller must be ApiController");
        //    }
        //}

        //[DebuggerStepThrough]
        //private class WebApiPhoenixWithPublicMethods : WebApiPhoenix
        //{
        //    public WebApiPhoenixWithPublicMethods(_IInvocation invocation, WebApiCacheItem cacheItem, HttpRequestMessage requestMessage)
        //        : base(invocation, cacheItem, requestMessage)
        //    {
        //    }

        //    public Task<CacheItem> GetCacheItemPublic(object response)
        //    {
        //        return GetCacheItem(response);
        //    }

        //    public Task<object> InvokeAndGetBareResultPublic(object serviceInstance)
        //    {
        //        return InvokeAndGetBareResult(serviceInstance);
        //    }

        //    public object GetTargetInstancePublic()
        //    {
        //        return GetTargetInstance();
        //    }
        //}

        [DebuggerStepThrough]
        private class WebApiPhoenixWithPublicMethods : WebApiPhoenix
        {
            public WebApiPhoenixWithPublicMethods(_IInvocation invocation, WebApiCacheItem cacheItem, HttpRequestMessage requestMessage)
                : base(invocation, cacheItem, requestMessage)
            {
            }

            public IHttpClient HttpClient { get; set; }

            protected override IHttpClient GetHttpClient()
            {
                return HttpClient;
            }

            public Task<IPhoenixState> FireAsyncPublic()
            {
                return FireAsync();
            }
        }
    }
}
