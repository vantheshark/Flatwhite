using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Flatwhite.Provider;

namespace Flatwhite
{
    /// <summary>
    /// The default method interceptors that resolve MVC/WebAPI style action filters to execute before and after the invocation
    /// </summary>
    public class MethodInterceptor : _IInterceptor
    {
        private static readonly MethodInfo _handleAsyncWithTypeMethod = typeof (MethodInterceptor).GetMethod(nameof(HandleAsyncWithType), BindingFlags.Instance | BindingFlags.NonPublic);
        private readonly IAttributeProvider _attributeProvider;
        private readonly IContextProvider _contextProvider;

        /// <summary>
        /// Initializes the interceptor
        /// </summary>
        /// <param name="attributeProvider"></param>
        /// <param name="contextProvider"></param>
        public MethodInterceptor(IAttributeProvider attributeProvider, IContextProvider contextProvider)
        {
            _attributeProvider = attributeProvider;
            _contextProvider = contextProvider;
        }

        /// <summary>
        /// Intercept the invocation
        /// </summary>
        /// <param name="invocation"></param>
        public void Intercept(_IInvocation invocation)
        {
            if (!invocation.Method.IsVirtual || invocation.Method.IsFinal)
            {
                invocation.Proceed();
                return;
            }

            var methodExecutingContext = new MethodExecutingContext
            {
                InvocationContext = _contextProvider.GetContext(),
                MethodInfo = invocation.Method,
                Invocation = invocation
            };
            
            var attributes = _attributeProvider.GetAttributes(invocation.Method, methodExecutingContext.InvocationContext);
            var filterAttributes = attributes.OfType<MethodFilterAttribute>().OrderBy(x => x.Order).ToList();
            var isAsync = typeof (Task).IsAssignableFrom(invocation.Method.ReturnType);

            if (isAsync)
            {
                if (invocation.Method.ReturnType.IsGenericType && invocation.Method.ReturnType.GetGenericTypeDefinition() == typeof (Task<>))
                {
                    var taskResultType = invocation.Method.ReturnType.GetGenericArguments()[0];
                    var mInfo = _handleAsyncWithTypeMethod.MakeGenericMethod(taskResultType);
                    filterAttributes.Add(new InvocationAttribute(invocation, taskResultType));
                    invocation.ReturnValue = mInfo.Invoke(this, new object[] { filterAttributes, methodExecutingContext, taskResultType });
                }
                else
                {
                    filterAttributes.Add(new InvocationAttribute(invocation));
                    invocation.ReturnValue = HandleAsync(filterAttributes, methodExecutingContext);
                }
            }
            else
            {
                filterAttributes.Add(new InvocationAttribute(invocation));
                HandleSync(filterAttributes, methodExecutingContext);
            }
        }

        private void HandleSync(List<MethodFilterAttribute> filterAttributes, MethodExecutingContext methodExecutingContext)
        {
            Exception exception = null;
            foreach (var f in filterAttributes)
            {
                try
                {
                    if (methodExecutingContext.Result == null) f.OnMethodExecuting(methodExecutingContext);
                }
                catch (Exception ex)
                {
                    exception = ex;
                    break;
                }
            }

            if (exception != null)
            {
                HandleException(new MethodExceptionContext(exception, methodExecutingContext));
                exception = null;
            }

            filterAttributes.Reverse();
            var methodExecutedContext = new MethodExecutedContext(methodExecutingContext);

            foreach (var f in filterAttributes)
            {
                try
                {
                    f.OnMethodExecuted(methodExecutedContext);
                }
                catch (Exception ex)
                {
                    exception = ex;
                    break;
                }
            }

            if (exception != null)
            {
                HandleException(new MethodExceptionContext(exception, methodExecutedContext));
            }
            if (methodExecutedContext.Result != null && typeof(void) != methodExecutedContext.Invocation.Method.ReturnType)
            {
                methodExecutedContext.Invocation.ReturnValue = methodExecutedContext.Result;
            }
        }

        /// <summary>
        /// Handle exception, throw
        /// </summary>
        /// <param name="exceptionContext"></param>
        private void HandleException(MethodExceptionContext exceptionContext)
        {
            var attributes = _attributeProvider.GetAttributes(exceptionContext.Invocation.Method, exceptionContext.InvocationContext);
            var exceptionFilterAttributes = attributes.OfType<ExceptionFilterAttribute>().ToList();
            if (exceptionFilterAttributes.Count == 0)
            {
                throw exceptionContext.Exception;
            }

            foreach (var f in exceptionFilterAttributes)
            {
                try
                {
                    if (!exceptionContext.Handled)
                    {
                        f.OnException(exceptionContext);
                    }
                }
                catch (Exception ex)
                {
                    throw new AggregateException(ex.Message, ex, exceptionContext.Exception);
                }
            }

            if (!exceptionContext.Handled)
            {
                throw exceptionContext.Exception;
            }
        }

        /// <summary>
        /// This will be called via Reflection
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="filterAttributes"></param>
        /// <param name="methodExecutingContext"></param>
        /// <param name="taskResultType"></param>
        /// <returns></returns>
        private async Task<TResult> HandleAsyncWithType<TResult>(List<MethodFilterAttribute> filterAttributes, MethodExecutingContext methodExecutingContext, Type taskResultType)
        {
            Exception exception = null;
            foreach (var f in filterAttributes)
            {
                try
                {
                    if (methodExecutingContext.Result == null)
                    {
                        await f.OnMethodExecutingAsync(methodExecutingContext).ConfigureAwait(false);
                    }
                }
                catch (Exception ex)
                {
                    exception = ex;
                    break;
                }
            }

            if (exception != null)
            {
                await HandleExceptionAsync(new MethodExceptionContext(exception, methodExecutingContext));
                exception = null;
            }

            filterAttributes.Reverse();
            var methodExecutedContext = new MethodExecutedContext(methodExecutingContext);

            foreach (var f in filterAttributes)
            {
                try
                {
                    await f.OnMethodExecutedAsync(methodExecutedContext).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    exception = ex;
                    break;
                }
            }

            if (exception != null)
            {
                await HandleExceptionAsync(new MethodExceptionContext(exception, methodExecutedContext));
            }

            return methodExecutedContext.Result != null ? (TResult)methodExecutedContext.Result : default(TResult);
        }

        private async Task HandleAsync(List<MethodFilterAttribute> filterAttributes, MethodExecutingContext methodExecutingContext)
        {
            Exception exception = null;
            foreach (var f in filterAttributes)
            {
                try
                {
                    await f.OnMethodExecutingAsync(methodExecutingContext).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    exception = ex;
                    break;
                }
            }

            if (exception != null)
            {
                await HandleExceptionAsync(new MethodExceptionContext(exception, methodExecutingContext));
                exception = null;
            }

            filterAttributes.Reverse();
            var methodExecutedContext = new MethodExecutedContext(methodExecutingContext);

            foreach (var f in filterAttributes)
            {
                try
                {
                    await f.OnMethodExecutedAsync(methodExecutedContext).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    exception = ex;
                    break;
                }
            }

            if (exception != null)
            {
                await HandleExceptionAsync(new MethodExceptionContext(exception, methodExecutedContext));
            }
        }

        private async Task HandleExceptionAsync(MethodExceptionContext exceptionContext)
        {
            var attributes = _attributeProvider.GetAttributes(exceptionContext.Invocation.Method, exceptionContext.InvocationContext);
            var exceptionFilterAttributes = attributes.OfType<ExceptionFilterAttribute>().ToList();
            if (exceptionFilterAttributes.Count == 0)
            {
                throw exceptionContext.Exception;
            }

            foreach (var f in exceptionFilterAttributes)
            {
                try
                {
                    if (!exceptionContext.Handled)
                    {
                        await f.OnExceptionAsync(exceptionContext).ConfigureAwait(false);
                    }
                }
                catch (Exception ex)
                {
                    throw new AggregateException(ex.Message, ex, exceptionContext.Exception);
                }
            }

            if (!exceptionContext.Handled)
            {
                throw exceptionContext.Exception;
            }
        }
    }
}
