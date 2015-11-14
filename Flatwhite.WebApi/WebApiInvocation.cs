using System;
using System.Linq;
using System.Reflection;
using System.Web.Http.Controllers;
#pragma warning disable 1591

namespace Flatwhite.WebApi
{
    public class WebApiInvocation : _IInvocation
    {
        private readonly HttpActionContext _httpActionContext;

        public WebApiInvocation(HttpActionContext httpActionContext)
        {
            _httpActionContext = httpActionContext;
        }

        public object[] Arguments => _httpActionContext.ActionArguments.Values.Select(v => v).ToArray();
        public Type[] GenericArguments => new Type[0];
        public object InvocationTarget => _httpActionContext.ControllerContext.Controller;
        public MethodInfo Method => ((ReflectedHttpActionDescriptor) _httpActionContext.ActionDescriptor).MethodInfo;

        public MethodInfo MethodInvocationTarget => Method;

        public object Proxy => null;
        public object ReturnValue { get; set; }
        public Type TargetType => _httpActionContext.ControllerContext.ControllerDescriptor.ControllerType;
        public object GetArgumentValue(int index)
        {
            return Arguments[index];
        }

        public MethodInfo GetConcreteMethod()
        {
            throw new NotSupportedException();
        }

        public MethodInfo GetConcreteMethodInvocationTarget()
        {
            throw new NotSupportedException();
        }

        public void Proceed()
        {
            throw new NotSupportedException();
        }

        public void SetArgumentValue(int index, object value)
        {
            throw new NotSupportedException();
        }
    }
}