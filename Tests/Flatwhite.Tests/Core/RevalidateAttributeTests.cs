using System.Collections.Generic;
using System.Threading.Tasks;
using NUnit.Framework;

namespace Flatwhite.Tests.Core
{
    [TestFixture]
    public class RevalidateAttributeTests
    {
        [Test]
        public async Task OnMethodExecutedAsync_should_call_RevalidateCachesAsync_on_global()
        {
            var att = new RevalidateAttribute
            {
                Keys = { "user", "book"}
            };
            string abc = "";
            Global.RevalidateEvent += x =>
            {
                abc += x;
            };
            await att.OnMethodExecutedAsync(new MethodExecutedContext(new MethodExecutingContext
            {
                InvocationContext = new Dictionary<string, object>()
            }));

            Assert.AreEqual(8, abc.Length);

        }

        [Test]
        public async Task OnMethodExecutedAsync_should_do_nothing_if_RevalidateEvent_doesnot_have_subscriptions()
        {
            var att = new RevalidateAttribute
            {
                Keys = { "user", "book" }
            };
            string abc = "";
            
            await att.OnMethodExecutedAsync(new MethodExecutedContext(new MethodExecutingContext
            {
                InvocationContext = new Dictionary<string, object>()
            }));

            Assert.IsEmpty(abc);
        }
    }
}
