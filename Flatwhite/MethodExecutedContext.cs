using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;

namespace Flatwhite
{
    /// <summary>
    /// Contains information for the executed action.
    /// </summary>
    public class MethodExecutedContext
    {
        internal MethodExecutedContext(MethodExecutingContext executingContext)
        {
            Invocation = executingContext.Invocation;
            MethodInfo = executingContext.MethodInfo;
            InvocationContext = new Dictionary<string, object>(executingContext.InvocationContext);
            Result = executingContext.Result;
        }
        /// <summary>
        /// The MethodInfo of the executed method
        /// </summary>
        public MethodInfo MethodInfo { get; }
        /// <summary>
        /// The invocation context of the executed method
        /// </summary>
        public IDictionary<string, object> InvocationContext { get; }
        /// <summary>
        /// The result of the method
        /// </summary>
        public object Result { get; set; }

        /// <summary>
        /// The Invocation
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        internal _IInvocation Invocation { get; }

        /// <summary>
        /// Try to get a set value from InvocationContext
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="invocationContextKey"></param>
        /// <returns></returns>
        public T TryGet<T>(string invocationContextKey)
        {
            return InvocationContext.TryGetByKey<T>(invocationContextKey);
        }
    }
}