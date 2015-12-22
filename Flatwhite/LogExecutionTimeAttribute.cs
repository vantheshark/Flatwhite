using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.Remoting.Messaging;
using System.Text;

namespace Flatwhite
{
    /// <summary>
    /// Use this attribute to decorate on a method where you want to print execution time to <see cref="Global.Logger"/>
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public class LogExecutionTimeAttribute : MethodFilterAttribute
    {
        private const string Key = "__flatwhite:traceContexts";

        /// <summary>
        /// Initializes in instance of PrintCallingTimeAttribute with minimum Order to make it run first
        /// </summary>
        public LogExecutionTimeAttribute()
        {
            Order = int.MinValue;
        }

        private class TraceContext
        {
            public int CallLevel { get; }
            public Stopwatch Timer { get; }
            public bool IsNested { get; }
            public StringBuilder Info { get; }

            public TraceContext()
            {
                CallLevel = 0;
                Timer = Stopwatch.StartNew();
                IsNested = false;
                Info = new StringBuilder();
            }

            public TraceContext(TraceContext parent)
            {
                CallLevel = parent.CallLevel + 1;
                Timer = Stopwatch.StartNew();
                IsNested = true;
                Info = parent.Info;
            }
        }

        /// <summary>
        /// Check if there is a parent trace, create a new trace and push to the stack which is stored in the CallContext
        /// </summary>
        /// <param name="methodExecutingContext"></param>
        public override void OnMethodExecuting(MethodExecutingContext methodExecutingContext)
        {
            var traces = GetTraceContexts();
            var trace = traces.Count > 0 ? new TraceContext(traces.Peek()) : new TraceContext();
            traces.Push(trace);
        }

        private static Stack<TraceContext> GetTraceContexts()
        {
            var traces = CallContext.LogicalGetData(Key) as Stack<TraceContext>;
            if (traces == null)
            {
                traces = new Stack<TraceContext>();
                CallContext.LogicalSetData(Key, traces);
            }
            return traces;
        }

        /// <summary>
        /// Pop the current trace and print info
        /// </summary>
        /// <param name="methodExecutedContext"></param>
        public override void OnMethodExecuted(MethodExecutedContext methodExecutedContext)
        {
            var traces = GetTraceContexts();
            var trace = traces.Pop();
            trace.Timer.Stop();
            try
            {
                var invocation = methodExecutedContext.Invocation;
                trace.Info.Insert(0, $"{" ".PadLeft(trace.CallLevel * 4)} {(invocation.Method.DeclaringType ?? invocation.TargetType).FullName}.{invocation.Method.Name} : {trace.Timer.ElapsedMilliseconds}ms\r\n");
            }
            finally
            {
                if (!trace.IsNested)
                {
                    Global.Logger.Info("\r\n" + trace.Info);
                }
            }
        }
    }
}