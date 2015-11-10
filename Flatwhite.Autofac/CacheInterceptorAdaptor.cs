using System;
using System.Diagnostics;
using System.Reflection;
using Castle.DynamicProxy;

namespace Flatwhite.AutofacIntergration
{
    /// <summary>
    /// An adaptor of <see cref="_IInterceptor"/>
    /// </summary>
    [DebuggerStepThrough]
    public class CacheInterceptorAdaptor : CacheInterceptor, IInterceptor
    {
        /// <summary>
        /// Initializes an instance of <see cref="CacheInterceptorAdaptor"/>
        /// </summary>
        /// <param name="contextProvider"></param>
        /// <param name="cacheProvider"></param>
        /// <param name="cacheStrategy"></param>
        public CacheInterceptorAdaptor(IContextProvider contextProvider, ICacheProvider cacheProvider, ICacheStrategy cacheStrategy = null) 
            : base(contextProvider, cacheProvider, cacheStrategy)
        {
        }
        
        /// <summary>
        /// Intercept the dynamic proxy invocation by calling Flatwhite Intercept method
        /// </summary>
        /// <param name="invocation"></param>
        public void Intercept(IInvocation invocation)
        {
            base.Intercept(new InvocationAdaptor(invocation));
        }

        class InvocationAdaptor : _IInvocation
        {
            private readonly IInvocation _invocation;

            public InvocationAdaptor(IInvocation invocation)
            {
                _invocation = invocation;
            }

            public object[] Arguments => _invocation.Arguments;
            public Type[] GenericArguments => _invocation.GenericArguments;
            public object InvocationTarget => _invocation.InvocationTarget;
            public MethodInfo Method => _invocation.Method;
            public MethodInfo MethodInvocationTarget => _invocation.MethodInvocationTarget;
            public object Proxy => _invocation.Proxy;
            public object ReturnValue {
                get
                {
                    return _invocation.ReturnValue;
                }
                set
                {
                    _invocation.ReturnValue = value;
                }
            }
            public Type TargetType => _invocation.TargetType;
            public object GetArgumentValue(int index)
            {
                return _invocation.GetArgumentValue(index);
            }

            public MethodInfo GetConcreteMethod()
            {
                return _invocation.GetConcreteMethod();
            }

            public MethodInfo GetConcreteMethodInvocationTarget()
            {
                return _invocation.GetConcreteMethodInvocationTarget();
            }

            public void Proceed()
            {
                _invocation.Proceed();
            }

            public void SetArgumentValue(int index, object value)
            {
                _invocation.SetArgumentValue(index, value);
            }
        }
    }
}
