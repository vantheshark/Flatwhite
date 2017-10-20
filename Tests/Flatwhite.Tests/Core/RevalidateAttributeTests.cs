using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Autofac;
using Flatwhite.AutofacIntergration;
using Flatwhite.Tests.Stubs;
using NSubstitute;
using NUnit.Framework;

namespace Flatwhite.Tests.Core
{
    [TestFixture]
    public class RevalidateAttributeTests
    {
        private MethodExecutedContext _methodExecutedContext;
        [SetUp]
        public void SetUp()
        {
            Global.Init();
            Global.CacheStoreProvider.RegisterStore(new NoneExpireCacheStore());
            var invocation = Substitute.For<_IInvocation>();
            invocation.Method.Returns(typeof(RevalidateAttributeTests).GetMethod(nameof(SetUp)));
            _methodExecutedContext = new MethodExecutedContext(new MethodExecutingContext
            {
                InvocationContext = new Dictionary<string, object>(),
                Invocation = invocation
            });            
        }

        [Test]
        public async Task OnMethodExecutedAsync_should_call_RevalidateCachesAsync_on_global()
        {
            var att = new RevalidateAttribute
            {
                KeyFormats = { "user", "book"}
            };
            string abc = "";
            //Global.RevalidateEvent += x =>
            //{
            //    abc += x;
            //};
            await att.OnMethodExecutedAsync(_methodExecutedContext);

            Assert.AreEqual(8, abc.Length);

        }

        [Test]
        public async Task OnMethodExecutedAsync_should_do_nothing_if_RevalidateEvent_doesnot_have_subscriptions()
        {
            var att = new RevalidateAttribute
            {
                KeyFormats = { "user", "book" }
            };
            string abc = "";
            
            await att.OnMethodExecutedAsync(_methodExecutedContext);

            Assert.IsEmpty(abc);
        }


        [Test]
        public async Task Should_revalidate_relevant_cache_item_only()
        {
            var mockObj = Substitute.For<IUserService>();
            mockObj.GetById(Arg.Any<Guid>()).Returns(c => (object)c.Arg<Guid>());

            var builder = new ContainerBuilder();
            builder.RegisterModule(new FlatwhiteCoreModule());
            builder
                .RegisterInstance(mockObj)
                .As<IUserService>()
                .EnableInterceptors();

            var container = builder.Build();

            var interceptedSvc = container.Resolve<IUserService>();
            var id1 = Guid.NewGuid();
            var id2 = Guid.NewGuid();

            for (var i = 1; i < 100; i++)
            {
                interceptedSvc.GetById(id1);
                interceptedSvc.GetById(id2);
            }

            await interceptedSvc.DisableUserAsync(id2).ConfigureAwait(false);
            await Task.Delay(1000);
            for (var i = 1; i < 100; i++)
            {
                interceptedSvc.GetById(id1);
            }

            interceptedSvc.GetById(id2);

            mockObj.Received(2).GetById(Arg.Is<Guid>(x => x == id2));
            mockObj.Received(1).GetById(Arg.Is<Guid>(x => x == id1));            
        }
    }
}
