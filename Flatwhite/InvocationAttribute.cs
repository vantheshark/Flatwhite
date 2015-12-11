using System;
using System.Threading.Tasks;

namespace Flatwhite
{
    internal class InvocationAttribute : MethodFilterAttribute
    {
        private readonly _IInvocation _invocation;
        private readonly Type _taskGenericReturnType;

        public InvocationAttribute(_IInvocation invocation, Type taskGenericReturnType = null)
        {
            _invocation = invocation;
            _taskGenericReturnType = taskGenericReturnType;
        }

        public override void OnMethodExecuting(MethodExecutingContext methodExecutingContext)
        {
            _invocation.Proceed();
            methodExecutingContext.Result = _invocation.ReturnValue;
        }

        public override async Task OnMethodExecutingAsync(MethodExecutingContext actionContext)
        {
            _invocation.Proceed();

            var taskResult = _invocation.ReturnValue as Task;

            if (taskResult != null && _taskGenericReturnType != null)
            {
                actionContext.Result = await taskResult.TryGetTaskResult();
            }
            else
            {
                actionContext.Result = _invocation.ReturnValue;
            }
        }
    }
}