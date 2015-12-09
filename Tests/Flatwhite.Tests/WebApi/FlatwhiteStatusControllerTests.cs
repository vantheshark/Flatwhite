using System.Collections.Generic;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web.Http.Results;
using Flatwhite.Provider;
using Flatwhite.WebApi;
using NSubstitute;
using NUnit.Framework;

namespace Flatwhite.Tests.WebApi
{
    [TestFixture]
    public class FlatwhiteStatusControllerTests
    {
        [Test]
        public async Task Store_action_should_get_all_cache_item_from_stores_that_matched_the_id()
        {
            var syncStore = Substitute.For<ICacheStore>();
            syncStore.GetAll().Returns(new List<KeyValuePair<string, object>>
            {
                new KeyValuePair<string, object>("item1", new CacheItem {Data = "data"}),
                new KeyValuePair<string, object>("item2", new CacheItem {Data = new MediaTypeHeaderValue("text/json")}),
                new KeyValuePair<string, object>("item2", new MediaTypeHeaderValue("application/xml")),
            });

            var asyncStore = Substitute.For<IAsyncCacheStore>();
            asyncStore.GetAllAsync().Returns(Task.FromResult(new List<KeyValuePair<string, object>>
            {
                new KeyValuePair<string, object>("item1", new WebApiCacheItem {Content = new byte[0]}),
                new KeyValuePair<string, object>("item2", new WebApiCacheItem {Content = new byte[0]}),
                new KeyValuePair<string, object>("item3", new MediaTypeHeaderValue("application/xml")),
            }));

            var provider = Substitute.For<ICacheStoreProvider>();
            provider.GetCacheStore(1).Returns(syncStore);
            provider.GetAsyncCacheStore(1).Returns(asyncStore);
            var controller = new FlatwhiteStatusController(provider);

            // Action
            var result = await controller.Store(1);
            var jsonResult = (JsonResult<List<FlatwhiteStatusController.CacheItemStatus>>) result;

            // Assert
            Assert.AreEqual(6, jsonResult.Content.Count);
        }
    }
}
