using NUnit.Framework;

namespace Flatwhite.Tests.WebApi.OutputCacheAttributeTests
{
    [TestFixture]
    public class TheMethodHashCacheKey
    {
        [Test]
        public void Should_return_md5_hashed_string()
        {
            // Arrange
            var att = new OutputCacheAttributeWithPublicMethods();
            // Action
            Assert.AreEqual("CE27B55B7504602FB088FEA4DBCEE15A", att.HashCacheKeyPublic("CacheKey"));
        }
    }
}
