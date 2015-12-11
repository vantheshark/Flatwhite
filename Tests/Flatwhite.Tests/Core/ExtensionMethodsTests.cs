using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using NSubstitute;
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

        [TestCase(typeof(void))]
        [TestCase(typeof(Task))]
        [TestCase(typeof(Task<Task>))]
        public void CheckMethodForCacheSupported_should_reject_none_supported_types(Type type)
        {
            var methodInfo = Substitute.For<MethodInfo>();
            methodInfo.ReturnType.Returns(type);
            bool isAsync;
            Assert.Throws<NotSupportedException>(() => methodInfo.CheckMethodForCacheSupported(out isAsync));
        }

        [TestCase(typeof(int), ExpectedResult = false)]
        [TestCase(typeof(object), ExpectedResult = false)]
        [TestCase(typeof(Task<int>), ExpectedResult = true)]
        public bool CheckMethodForCacheSupported_should_set_output_async_param(Type type)
        {
            var methodInfo = Substitute.For<MethodInfo>();
            methodInfo.ReturnType.Returns(type);
            bool isAsync;
            methodInfo.CheckMethodForCacheSupported(out isAsync);
            return isAsync;
        }
    }
}
