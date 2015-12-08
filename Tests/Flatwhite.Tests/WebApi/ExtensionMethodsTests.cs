using System.Collections.Specialized;
using Flatwhite.WebApi;
using NUnit.Framework;

namespace Flatwhite.Tests.WebApi
{
    [TestFixture]
    public class ExtensionMethodsTests
    {
        [Test]
        public void ToDictionary_can_convert_to_dictionary_from_NameValueCollection()
        {
            var nameValue = new NameValueCollection
            {
                {"KEY1", "Value1" },
                {"Key2", "VALUE2" },
                {"kEy3", "vALuE3" }
            };

            var dic = nameValue.ToDictionary();

            Assert.AreEqual("value1", dic["key1"]);
            Assert.AreEqual("value2", dic["key2"]);
            Assert.AreEqual("value3", dic["key3"]);
        }
    }
}
