using System.Collections.Generic;
using System.IO;
using NUnit.Framework;

namespace Flatwhite.Tests.Core
{
    [TestFixture]
    public class YamlCacheProfileProviderTests
    {
        [Test]
        public void Should_apply_profile_settingg()
        {
            var att = new OutputCacheAttribute();
            Global.CacheProfileProvider.ApplyProfileSetting(att, "Profile1-Two-Seconds");
            Assert.AreEqual(2, att.Duration);
            Assert.AreEqual(5, att.StaleWhileRevalidate);
            Assert.AreEqual(true, att.AutoRefresh);
            Assert.AreEqual("packageId", att.VaryByParam);
            Assert.AreEqual("*", att.VaryByCustom);
            Assert.AreEqual("anything-about-user", att.RevalidationKey);



            var att2 = new Flatwhite.WebApi.OutputCacheAttribute
            {
                MaxAge = 3,
                StaleWhileRevalidate = 4
            };
            Global.CacheProfileProvider.ApplyProfileSetting(att2, "Web-Profile2-Three-Seconds");
            Assert.AreEqual(3, att2.MaxAge);
            Assert.AreEqual(6, att2.StaleWhileRevalidate);
            Assert.AreEqual(false, att2.AutoRefresh);
            Assert.AreEqual("*", att2.VaryByParam);
            Assert.AreEqual("UserAgent", att2.VaryByHeader);
        }

        [Test]
        public void Shuold_throw_exception_if_syntax_invalid()
        {
            Assert.Throws<InvalidDataException>(
                () => YamlCacheProfileProvider.ReadYamlData(new List<string> {"\tKeyWithoutValue"}));
        }

        [Test]
        public void Should_ignore_irrelevant_settings()
        {
            var att = new OutputCacheAttribute
            {
                Duration = 10,
                StaleWhileRevalidate = 11,
                CacheProfile = "Profile3-With-Some-Bad-Settings"
            };
            Assert.AreEqual(10, att.Duration);
            Assert.AreEqual(11, att.StaleWhileRevalidate);

            att = new OutputCacheAttribute
            {
                Duration = 10,
                StaleWhileRevalidate = 11,
                CacheProfile = "NoneExistingProfile"
            };
            Assert.AreEqual(10, att.Duration);
            Assert.AreEqual(11, att.StaleWhileRevalidate);
        }
    }
}
