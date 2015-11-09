using System;
using System.Collections.Generic;
using System.Reflection;

namespace Flatwhite
{
    /// <summary>
    /// An provider to get all attributes decorated on method or declarative type
    /// </summary>
    public interface IAttributeProvider
    {
        /// <summary>
        /// Get all attributes decorated on method or declarative type
        /// </summary>
        /// <param name="methodInfo"></param>
        /// <param name="invocationContext"></param>
        /// <returns></returns>
        IEnumerable<Attribute> GetAttributes(MethodInfo methodInfo, IDictionary<string, object> invocationContext);
    }
}