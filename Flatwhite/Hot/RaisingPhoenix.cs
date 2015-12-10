using System;
using System.Threading.Tasks;

namespace Flatwhite.Hot
{
    /// <summary>
    /// Represent a raising phoenix. Calling reborn many time will not actually do anything until the method reborn complete and it will return the value returned from rebornAction
    /// </summary>
    internal class RaisingPhoenix : IPhoenixState
    {
        /// <summary>
        /// true if the phoenix is executing rebornAction. This is to avoid many call on method Reborn many time
        ///  </summary>

        private Task<IPhoenixState> _backGroundTask;

        public IPhoenixState Reborn(Func<Task<IPhoenixState>> rebornAction)
        {
            if (_backGroundTask == null || 
                _backGroundTask.Status == TaskStatus.Faulted || 
                _backGroundTask.Status == TaskStatus.Canceled || 
                _backGroundTask.Status == TaskStatus.RanToCompletion && _backGroundTask.Result == null)
            {
                _backGroundTask?.Dispose();
                _backGroundTask = rebornAction();
                _backGroundTask.LogErrorOnFaulted();
            }
            
            if (_backGroundTask.Status == TaskStatus.RanToCompletion && _backGroundTask.Result != null)
            {
                return _backGroundTask.Result;
            }

            return this;
        }

        public string GetState()
        {
            return 
                _backGroundTask == null || 
                _backGroundTask.Status == TaskStatus.WaitingForActivation ||
                _backGroundTask.Status == TaskStatus.WaitingForChildrenToComplete ||
                _backGroundTask.Status == TaskStatus.WaitingToRun
                
                ? "wait to raise" 
                : "raising";
        }
    }
}