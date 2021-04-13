using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Web.Http.Controllers;
using System.Web.Http.Services;

#pragma warning disable 1591

namespace Flatwhite.WebApi
{
    [ExcludeFromCodeCoverage]
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
        public MethodInfo Method
        {
            get
            {
                // It seems the concrete type for ActionDescriptor depends on whether or not the Global Web API Services
                // contains an instance of ITraceWriter (see: Tracing in ASP.NET Web API).
                // By default, the ActionDescriptor will be of type ReflectedHttpActionDescriptor. But when tracing is enabled
                // by calling the config.EnableSystemDiagnosticsTracing() the ActionDescriptor will be wrapped inside an
                // HttpActionDescriptorTracer type instead

                ReflectedHttpActionDescriptor reflectedActionDescriptor;

                // Check whether the ActionDescriptor is wrapped in a decorator or not.
                var wrapper = _httpActionContext.ActionDescriptor as IDecorator<HttpActionDescriptor>;
                if (wrapper != null)
                {
                    reflectedActionDescriptor = wrapper.Inner as ReflectedHttpActionDescriptor;
                }
                else
                {
                    reflectedActionDescriptor = _httpActionContext.ActionDescriptor as ReflectedHttpActionDescriptor;
                }


                return reflectedActionDescriptor?.MethodInfo;
            }
        }

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