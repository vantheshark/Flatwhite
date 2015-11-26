using System;

namespace Flatwhite.Hot
{
    /// <summary>
    /// The phoenix is disposed, it will not do anything
    /// </summary>
    internal class DisposingPhoenix : IPhoenixState
    {
        private readonly Action _disposeAction;

        public DisposingPhoenix(Action disposeAction)
        {
            _disposeAction = disposeAction;
        }

        /// <summary>
        /// Will call dispose action when Reborn as it is disposing Phoenix
        /// </summary>
        /// <param name="rebornAction"></param>
        /// <returns></returns>
        public IPhoenixState Reborn(Func<IPhoenixState> rebornAction)
        {
            _disposeAction();
            return this;
        }
    }
}