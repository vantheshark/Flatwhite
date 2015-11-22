using System;
using System.Reflection;
using Castle.DynamicProxy;

namespace Flatwhite.AutofacIntergration
{
    /// <summary>
    /// Adapt from Castle Dynamic proxy IInvocation to Flatwhite _IInvocation
    /// </summary>
    internal class InvocationAdaptor : _IInvocation
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
        public object ReturnValue
        {
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