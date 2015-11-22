using System.Diagnostics;
using Castle.DynamicProxy;
using Flatwhite.Provider;

namespace Flatwhite.AutofacIntergration
{
    /// <summary>
    /// An adaptor of <see cref="IInterceptor"/>
    /// </summary>
    [DebuggerStepThrough]
    public class MethodInterceptorAdaptor : MethodInterceptor, IInterceptor
    {
        /// <summary>
        /// Initializes an instance of <see cref="MethodInterceptorAdaptor"/>
        /// </summary>
        /// <param name="contextProvider"></param>
        /// <param name="attributeProvider"></param>
        public MethodInterceptorAdaptor(IAttributeProvider attributeProvider, IContextProvider contextProvider) 
            : base(attributeProvider, contextProvider)
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
    }
}
