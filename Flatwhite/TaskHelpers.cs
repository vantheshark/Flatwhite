using System;
using System.Threading.Tasks;

namespace Flatwhite
{
    /// <summary>
    /// Copied from System.Threading.Tasks.TaskHelpers
    /// </summary>
    internal static class TaskHelpers
    {
        private struct AsyncVoid
        {
        }
        internal static readonly Task DefaultCompleted = Task.FromResult(default(AsyncVoid));

        internal static Task FromError(Exception exception)
        {
            return FromError<AsyncVoid>(exception);
        }
        internal static Task<TResult> FromError<TResult>(Exception exception)
        {
            var tcs = new TaskCompletionSource<TResult>();
            tcs.SetException(exception);
            return tcs.Task;
        }
    }
}