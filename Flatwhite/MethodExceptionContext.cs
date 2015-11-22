using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;

namespace Flatwhite
{
    /// <summary>
    /// Represents an exception and the contextual data associated with it when exception was caught.
    /// </summary>
    public class MethodExceptionContext
    {
        private readonly Action<object> _updateResult;
        /// <summary>
        /// Initializes the exception context with current <see cref="MethodExecutingContext" />
        /// </summary>
        /// <param name="exception"></param>
        /// <param name="executingContext"></param>
        public MethodExceptionContext(Exception exception, MethodExecutingContext executingContext)
        {
            Invocation = executingContext.Invocation;
            MethodInfo = executingContext.MethodInfo;
            InvocationContext = executingContext.InvocationContext;
            Exception = exception;
            _updateResult = r => executingContext.Result = r;
        }

        /// <summary>
        /// Initializes the exception context with current <see cref="MethodExecutedContext" />
        /// </summary>
        /// <param name="exception"></param>
        /// <param name="executedContext"></param>
        public MethodExceptionContext(Exception exception, MethodExecutedContext executedContext)
        {
            Invocation = executedContext.Invocation;
            MethodInfo = executedContext.MethodInfo;
            InvocationContext = executedContext.InvocationContext;
            Exception = exception;

            _result = executedContext.Result;
            _updateResult = r => executedContext.Result = r;
        }

        /// <summary>
        /// The MethodInfo of the executing method
        /// </summary>
        public MethodInfo MethodInfo { get; }
        /// <summary>
        /// The invocation context of the executing method
        /// </summary>
        public IDictionary<string, object> InvocationContext { get; }

        private object _result;
        /// <summary>
        /// The result of the method
        /// </summary>
        public object Result {
            get { return _result; }
            set
            {
                _result = value;
                _updateResult(value);
            }
        }

        /// <summary>
        /// The result of the method
        /// </summary>
        public Exception Exception { get; set; }

        /// <summary>
        /// Indicate whether the excepton was handled
        /// </summary>
        public bool Handled { get; set; }

        /// <summary>
        /// The invocation
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public _IInvocation Invocation { get; }
    }
}