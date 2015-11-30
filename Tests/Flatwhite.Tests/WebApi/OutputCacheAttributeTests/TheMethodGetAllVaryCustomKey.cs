using NUnit.Framework;

namespace Flatwhite.Tests.WebApi.OutputCacheAttributeTests
{
    [TestFixture]
    public class TheMethodGetAllVaryCustomKey
    {
        [Test]
        public void Should_combine_VaryByCustom_and_VaryByHeader()
        {
            // Arrange
            var att = new Flatwhite.WebApi.OutputCacheAttribute
            {
                VaryByCustom = "query.query1, query.query2",
                VaryByHeader = "UserAgent, CacheControl.Public"
            };
            // Action
            Assert.AreEqual("query.query1, query.query2, headers.UserAgent, headers.CacheControl.Public", att.GetAllVaryCustomKey());
            
        }
    }
}
