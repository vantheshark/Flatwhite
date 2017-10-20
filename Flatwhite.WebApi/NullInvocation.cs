using System;
using System.Reflection;

namespace Flatwhite.WebApi
{
    internal class NullInvocation : _IInvocation
    {
        public static readonly _IInvocation Instance = new NullInvocation();
        public object[] Arguments { get; }
        public Type[] GenericArguments { get; }
        public object InvocationTarget { get; }
        public MethodInfo Method { get; }
        public MethodInfo MethodInvocationTarget { get; }
        public object Proxy { get; }
        public object ReturnValue { get; set; }
        public Type TargetType { get; }
        public object GetArgumentValue(int index)
        {
            throw new NotImplementedException();
        }

        public MethodInfo GetConcreteMethod()
        {
            throw new NotImplementedException();
        }

        public MethodInfo GetConcreteMethodInvocationTarget()
        {
            throw new NotImplementedException();
        }

        public void Proceed()
        {
            throw new NotImplementedException();
        }

        public void SetArgumentValue(int index, object value)
        {
            throw new NotImplementedException();
        }
    }
}