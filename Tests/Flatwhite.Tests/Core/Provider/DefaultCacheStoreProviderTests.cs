using System;
using System.Collections.Generic;
using Flatwhite.Provider;
using NSubstitute;
using NUnit.Framework;

namespace Flatwhite.Tests.Core.Provider
{
    [TestFixture]
    public class DefaultCacheStoreProviderTests
    {
        [Test]
        public void GetCacheStore_should_return_null_if_storeId_less_than_0_or_not_found()
        {
            var provider = new DefaultCacheStoreProvider();
            Assert.IsNull(provider.GetCacheStore(-1));
            Assert.IsNull(provider.GetCacheStore(1));
            Assert.IsNotNull(provider.GetCacheStore(0));
        }

        [Test]
        public void GetCacheStore_should_throw_exception_if_store_type_not_found()
        {
            var provider = new DefaultCacheStoreProvider();
            Assert.Throws<KeyNotFoundException>(() => provider.GetCacheStore(typeof(DefaultCacheStoreProviderTests)));
            Assert.IsNotNull(provider.GetCacheStore(typeof(ICacheStore)));
        }

        [Test]
        public void GetAsyncCacheStore_should_return_null_if_storeId_less_than_0_or_not_found()
        {
            var provider = new DefaultCacheStoreProvider();
            Assert.IsNull(provider.GetAsyncCacheStore(-1));
            Assert.IsNull(provider.GetAsyncCacheStore(1));
            Assert.IsNotNull(provider.GetAsyncCacheStore(0));
        }

        [Test]
        public void GetAsyncCacheStore_should_return_async_store_adaptor_if_sync_store_with_same_id_found()
        {
            var store = Substitute.For<ICacheStore>();
            store.StoreId.Returns(10);
            var provider = new DefaultCacheStoreProvider();
            provider.RegisterStore(store);
            Assert.IsNotNull(provider.GetAsyncCacheStore(10));
        }

        [Test]
        public void GetAsyncCacheStore_should_throw_exception_if_store_type_not_found()
        {
            var provider = new DefaultCacheStoreProvider();
            Assert.Throws<KeyNotFoundException>(() => provider.GetAsyncCacheStore(typeof(DefaultCacheStoreProviderTests)));
            Assert.IsNotNull(provider.GetCacheStore(typeof(IAsyncCacheStore)));
        }

        [Test]
        public void RegisterStore_should_register_both_type_and_id()
        {
            var store = Substitute.For<ICacheStore>();
            store.StoreId.Returns(10);
            var provider = new DefaultCacheStoreProvider();
            Assert.Throws<ArgumentNullException>(() => provider.RegisterStore(null));
            provider.RegisterStore(store);
            Assert.IsNotNull(provider.GetCacheStore(10));
            Assert.IsNotNull(provider.GetCacheStore(store.GetType()));
        }

        [Test]
        public void RegisterStore_should_throw_exception_if_duplicated_async_store_id()
        {
            var store = Substitute.For<ICacheStore>();
            store.StoreId.Returns(10);

            var asyncStore = Substitute.For<IAsyncCacheStore>();
            asyncStore.StoreId.Returns(10);
            var provider = new DefaultCacheStoreProvider();
            provider.RegisterAsyncStore(asyncStore);
            Assert.Throws<InvalidOperationException>(() => provider.RegisterStore(store));
        }

        [Test]
        public void RegisterAsyncStore_should_register_both_type_and_id()
        {
            var store = Substitute.For<IAsyncCacheStore>();
            store.StoreId.Returns(10);
            var provider = new DefaultCacheStoreProvider();
            Assert.Throws<ArgumentNullException>(() => provider.RegisterAsyncStore(null));
            provider.RegisterAsyncStore(store);
            Assert.IsNotNull(provider.GetAsyncCacheStore(10));
            Assert.IsNotNull(provider.GetAsyncCacheStore(store.GetType()));
        }

        [Test]
        public void RegisterAsyncStore_should_throw_exception_if_duplicated_store_id()
        {
            var store = Substitute.For<ICacheStore>();
            store.StoreId.Returns(10);
            var asyncStore = Substitute.For<IAsyncCacheStore>();
            asyncStore.StoreId.Returns(10);

            var provider = new DefaultCacheStoreProvider();
            provider.RegisterStore(store);
            Assert.Throws<InvalidOperationException>(() => provider.RegisterAsyncStore(asyncStore));
        }

        [Test]
        public void Dispose_should_clear_all_cached_stores()
        {
            var provider = new DefaultCacheStoreProvider();
            provider.Dispose();
            Assert.IsNull(provider.GetCacheStore(0));
            Assert.IsNull(provider.GetAsyncCacheStore(0));
        }
    }
}
