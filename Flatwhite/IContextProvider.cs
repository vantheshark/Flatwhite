using System.Collections.Generic;

namespace Flatwhite
{
    /// <summary>
    /// A provider to resolve a context which is a key/value dictionary. This could be a HttpRequestContext or a WebApi context or anything from current thread
    /// </summary>
    public interface IContextProvider
    {
        /// <summary>
        /// Get context
        /// </summary>
        /// <returns></returns>
        IDictionary<string, object> GetContext();
    }

    /// <summary>
    /// A default context provider which returns empty dictionary
    /// </summary>
    internal class EmptyContextProvider : IContextProvider
    {
        public IDictionary<string, object> GetContext()
        {
            return new Dictionary<string, object>();
        }
    }
}
