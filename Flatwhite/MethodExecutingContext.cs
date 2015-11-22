using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;

namespace Flatwhite
{
    /// <summary>
    /// Contains information for the executing action.
    /// </summary>
    public class MethodExecutingContext
    {
        /// <summary>
        /// The MethodInfo of the executing method
        /// </summary>
        public MethodInfo MethodInfo { get; internal set; }
        /// <summary>
        /// The invocation context of the executing method
        /// </summary>
        public IDictionary<string, object> InvocationContext { get; internal set; }
        /// <summary>
        /// The result of the method
        /// </summary>
        public object Result { get; set; }

        /// <summary>
        /// The Invocation
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public _IInvocation Invocation { get; internal set; }
    }
}