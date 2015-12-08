using System.Web.Http.Controllers;
using Flatwhite.WebApi;
using NUnit.Framework;

namespace Flatwhite.Tests.WebApi.OutputCacheAttributeTests
{
    [TestFixture]
    public class TheMethodGetInvocation
    {
        [Test]
        public void Should_return_WebApiInvocation()
        {
            // Arrange
            var att = new OutputCacheAttributeWithPublicMethods();
            // Action
            Assert.That(att.GetInvocationPublic(new HttpActionContext()) is WebApiInvocation);
        }
    }
}
