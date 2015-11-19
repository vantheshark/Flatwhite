using System;
using System.Collections.Generic;
using System.Linq;

namespace Flatwhite
{
    /// <summary>
    /// Use this attribute to decorate on a method where you want to revalidate a specific cache entry after a method is invoked
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public class RevalidateAttribute : Attribute
    {
        /// <summary>
        /// List of keys
        /// </summary>
        public List<string> Keys { get; }

        /// <summary>
        /// Initializes a <see cref="RevalidateAttribute" /> with a list of revalidation keys
        /// </summary>
        /// <param name="keys"></param>
        public RevalidateAttribute(params string[] keys)
        {
            Keys = keys.ToList();
        }
    }
}