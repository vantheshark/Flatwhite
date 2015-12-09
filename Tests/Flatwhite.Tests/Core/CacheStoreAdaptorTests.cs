using System;
using System.Threading.Tasks;
using NSubstitute;
using NUnit.Framework;

namespace Flatwhite.Tests.Core
{
    [TestFixture]
    public class CacheStoreAdaptorTests
    {
        private ICacheStore _cacheStore;
        private CacheStoreAdaptor _adaptor;

        [SetUp]
        public void Setup()
        {
            _cacheStore = Substitute.For<ICacheStore>();
            _cacheStore.StoreId.Returns(10);
            _adaptor = new CacheStoreAdaptor(_cacheStore);
            Assert.AreEqual(10, _adaptor.StoreId);
        }

        [Test]
        public async Task Should_call_sync_cacheStore_method()
        {
            var time = DateTimeOffset.Now;
            _adaptor.Set("_key", "value", time);
            _cacheStore.Received(1).Set("_key", "value", time);

            _adaptor.Get("_key2");
            _cacheStore.Received(1).Get("_key2");

            _adaptor.Remove("_key3");
            _cacheStore.Received(1).Remove("_key3");

            _adaptor.Contains("_key3");
            _cacheStore.Received(1).Contains("_key3");

            _adaptor.GetAll();
            _cacheStore.Received(1).GetAll();

            await _adaptor.SetAsync("key", "value", time);
            _cacheStore.Received(1).Set("key", "value", time);

            await _adaptor.RemoveAsync("key2");
            _cacheStore.Received(1).Remove("key2");

            await _adaptor.GetAsync("key3");
            _cacheStore.Received(1).Get("key3");

            await _adaptor.ContainsAsync("key3");
            _cacheStore.Received(1).Contains("key3");

            await _adaptor.GetAllAsync();
            _cacheStore.Received(2).GetAll();
        }
    }
}
