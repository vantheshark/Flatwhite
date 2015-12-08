using System.Collections.Generic;
using NUnit.Framework;

namespace Flatwhite.Tests.Core
{
    [TestFixture]
    public class ExtensionMethodsTests
    {
        [Test]
        public void TryGetByKey_should_return_default_if_type_miss_match()
        {
            var dic = new Dictionary<string, object>
            {
                ["numKey"] = 1,
                ["stringKey"] = "1"
            };
            Assert.AreEqual(null, dic.TryGetByKey<string>("numKey"));
            Assert.AreEqual(0, dic.TryGetByKey<int>("stringKey"));
        }
    }
}
