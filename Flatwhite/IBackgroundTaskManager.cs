using System;
using System.Threading.Tasks;

namespace Flatwhite
{
    /// <summary>
    /// Provide method to run and manage tasks in background
    /// https://github.com/StephenCleary/AspNetBackgroundTasks/blob/master/src/AspNetBackgroundTasks/BackgroundTaskManager.cs
    /// </summary>
    public interface IBackgroundTaskManager
    {
        /// <summary>
        /// Executes an asynchronous background operation, registering it with ASP.NET.
        /// </summary>
        /// <param name="operation">The background operation.</param>
        void Run(Func<Task> operation);
    }

    internal class DefaultBackgroundTaskManager : IBackgroundTaskManager
    {
        public void Run(Func<Task> operation)
        {
            Task.Run(operation);
        }
    }
}
