using NUnit.Framework;

namespace Flatwhite.Tests.Core
{
    [TestFixture]
    public class ToStringHashCodeGeneratorTests
    {
        [Test]
        public void Should_return_string_value_if_object_is_not_null()
        {
            var svc = new ToStringHashCodeGenerator();
            Assert.AreEqual("null", svc.GetCode(null));
            Assert.AreEqual("123", svc.GetCode("123"));
            Assert.AreEqual("123", svc.GetCode(123));
            Assert.AreEqual("Flatwhite.ToStringHashCodeGenerator", svc.GetCode(svc));
        }
    }
}
