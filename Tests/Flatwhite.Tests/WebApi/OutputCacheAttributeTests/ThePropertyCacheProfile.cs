using NUnit.Framework;
using NSubstitute;

namespace Flatwhite.Tests.WebApi.OutputCacheAttributeTests
{
    [TestFixture]
    public class ThePropertyCacheProfile
    {
        [Test]
        public void Should_set_profile_setting()
        {
            // Arrange
            var provider = Substitute.For<IOutputCacheProfileProvider>();
            Global.CacheProfileProvider = provider;


            // Action
            var att = new Flatwhite.WebApi.OutputCacheAttribute { CacheProfile = "someProfile" };

            // Assert
            provider.Received(1).ApplyProfileSetting(att, "someProfile");
            Assert.AreEqual("someProfile", att.CacheProfile);
        }
    }
}
