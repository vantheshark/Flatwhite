using System;
using System.Collections.Generic;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web.Http.Results;
using Flatwhite.Hot;
using Flatwhite.Provider;
using Flatwhite.WebApi;
using NSubstitute;
using NUnit.Framework;

namespace Flatwhite.Tests.WebApi
{
    [TestFixture]
    public class FlatwhiteStatusControllerTests
    {
        public class BadObject
        {
            public string BadProperty
            {
                get
                {
                    throw new InvalidOperationException("Cannot be serialized");
                }
            }
        }

        [SetUp]
        public void SetUp()
        {
            Global.Init();
        }

        [Test]
        public async Task Store_action_should_get_all_cache_item_from_stores_that_matched_the_id()
        {
            Global.Cache.PhoenixFireCage.Add("item1", Substitute.For<Phoenix>(Substitute.For<_IInvocation>(), new CacheItem()));
            Global.Cache.PhoenixFireCage.Add("item2", Substitute.For<Phoenix>(Substitute.For<_IInvocation>(), new CacheItem()));
            Global.Cache.PhoenixFireCage.Add("item5", Substitute.For<Phoenix>(Substitute.For<_IInvocation>(), new CacheItem()));
            Global.Cache.PhoenixFireCage.Add("item6", Substitute.For<Phoenix>(Substitute.For<_IInvocation>(), new CacheItem()));

            var syncStore = Substitute.For<ICacheStore>();
            syncStore.GetAll().Returns(new List<KeyValuePair<string, object>>
            {
                new KeyValuePair<string, object>("item0", null),
                new KeyValuePair<string, object>("item1", new CacheItem {Data = "data", Key = "item1"}),
                new KeyValuePair<string, object>("item2", new CacheItem {Data = new MediaTypeHeaderValue("text/json"), Key = "item2"}),
                new KeyValuePair<string, object>("item3", new MediaTypeHeaderValue("application/xml")),
            });

            var asyncStore = Substitute.For<IAsyncCacheStore>();
            asyncStore.GetAllAsync().Returns(Task.FromResult(new List<KeyValuePair<string, object>>
            {
                new KeyValuePair<string, object>("item4", new BadObject()),
                new KeyValuePair<string, object>("item5", new WebApiCacheItem {Content = null, Key = "item5"}),
                new KeyValuePair<string, object>("item6", new WebApiCacheItem {Content = new byte[0], Key = "item4"}),
                new KeyValuePair<string, object>("item7", new MediaTypeHeaderValue("application/xml")),
            }));

            var provider = Substitute.For<ICacheStoreProvider>();
            provider.GetCacheStore(1).Returns(syncStore);
            provider.GetAsyncCacheStore(1).Returns(asyncStore);
            var controller = new FlatwhiteStatusController(provider);

            // Action
            var result = await controller.Store(1);
            var jsonResult = (JsonResult<List<FlatwhiteStatusController.CacheItemStatus>>) result;

            // Assert
            Assert.AreEqual(8, jsonResult.Content.Count);
        }

        [Test]
        public async Task Phoenix_action_should_display_all_phoenix_in_cache()
        {
            Global.Cache.PhoenixFireCage.Add("item1", Substitute.For<Phoenix>(Substitute.For<_IInvocation>(), new CacheItem { Data = "data", Key = "item1" }));
            Global.Cache.PhoenixFireCage.Add("item2", Substitute.For<Phoenix>(Substitute.For<_IInvocation>(), new CacheItem { Data = new MediaTypeHeaderValue("text/json"), Key = "item2" }));
            Global.Cache.PhoenixFireCage.Add("item5", Substitute.For<Phoenix>(Substitute.For<_IInvocation>(), new CacheItem()));
            Global.Cache.PhoenixFireCage.Add("item6", Substitute.For<Phoenix>(Substitute.For<_IInvocation>(), new CacheItem()));

            var syncStore = Substitute.For<ICacheStore>();
            syncStore.Get(Arg.Any<string>()).Returns(c => new CacheItem
            {
                Key = c.Arg<string>(),
                Data = "data"
            });

            var asyncStore = Substitute.For<IAsyncCacheStore>();
            asyncStore.GetAsync(Arg.Any<string>()).Returns(c =>
            {
                object obj = new WebApiCacheItem
                {
                    Key = c.Arg<string>(),
                    Content = new byte[0]
                };
                return Task.FromResult(obj);
            });

            var provider = Substitute.For<ICacheStoreProvider>();
            provider.GetCacheStore(Arg.Any<int>()).Returns(syncStore);
            provider.GetAsyncCacheStore(Arg.Any<int>()).Returns(asyncStore);
            var controller = new FlatwhiteStatusController(provider);

            // Action
            var result = await controller.Phoenix();
            var jsonResult = (JsonResult<List<FlatwhiteStatusController.CacheItemStatus>>)result;

            // Assert
            Assert.AreEqual(4, jsonResult.Content.Count);
        }
    }
}
