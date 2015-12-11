using System.Diagnostics;
using System.Reflection;

namespace Flatwhite.Tests.Core
{
    [DebuggerStepThrough]
    public class OutputCacheAttributeWithPublicMethods : OutputCacheAttribute
    {
        public void CreatePhoenixPublic(_IInvocation invocation, CacheItem cacheItem)
        {
            var methodInfo = typeof(OutputCacheAttribute).GetMethod("CreatePhoenix", BindingFlags.Instance | BindingFlags.NonPublic);
            methodInfo.Invoke(this, new object[] { invocation, cacheItem });
        }
    }
}
