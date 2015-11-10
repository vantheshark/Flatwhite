using System;
using System.Reflection;
// ReSharper disable InconsistentNaming
#pragma warning disable 1591

namespace Flatwhite
{
    /// <summary>
    /// Copy from Castle.Core.IInterceptor
    /// </summary>
    public interface _IInterceptor
    {
        /// <summary>
        /// Intercept the invocation
        /// </summary>
        /// <param name="invocation"></param>
        void Intercept(_IInvocation invocation);
    }

    /// <summary>
    /// Copy from Castle.Core.Invocation
    /// </summary>
    public interface _IInvocation
    {
        //
        // Summary:
        //     Gets the arguments that the Castle.DynamicProxy._IInvocation.Method has been invoked
        //     with.
        object[] Arguments { get; }

        Type[] GenericArguments { get; }

        object InvocationTarget { get; }

        MethodInfo Method { get; }

        MethodInfo MethodInvocationTarget { get; }

        object Proxy { get; }

        object ReturnValue { get; set; }

        Type TargetType { get; }

        object GetArgumentValue(int index);

        MethodInfo GetConcreteMethod();

        MethodInfo GetConcreteMethodInvocationTarget();

        void Proceed();

        void SetArgumentValue(int index, object value);
    }
}