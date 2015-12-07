using NUnit.Framework;

namespace Flatwhite.Tests.Core
{
    [TestFixture]
    public class DefaultHashCodeGeneratorTests
    {
        [Test]
        public void Should_return_hashCode_if_object_is_not_null()
        {
            var svc = new DefaultHashCodeGenerator();
            Assert.AreEqual("null", svc.GetCode(null));
            Assert.IsNotNull(svc.GetCode("123"));
            Assert.IsNotNull(svc.GetCode(svc));
        }
    }
}
