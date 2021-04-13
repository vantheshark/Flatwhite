using System.Collections.Generic;

namespace Flatwhite.Provider
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
}
