using System;
using System.Threading.Tasks;

namespace Flatwhite.Hot
{
    /// <summary>
    /// The phoenix is disposed, it will not do anything
    /// </summary>
    internal class DisposingPhoenix : IPhoenixState
    {
        private readonly Task _disposingTask;

        public DisposingPhoenix(Task disposingTask)
        {
            _disposingTask = disposingTask;
            disposingTask.LogErrorOnFaulted();
        }

        /// <summary>
        /// Do nothing here as the disposing action has been started
        /// </summary>
        /// <param name="rebornAction"></param>
        /// <returns></returns>
        public IPhoenixState Reborn(Func<Task<IPhoenixState>> rebornAction)
        {
            return this;
        }

        public string GetState()
        {
            return _disposingTask.IsCompleted ? "disposed" : "disposing";
        }
    }
}