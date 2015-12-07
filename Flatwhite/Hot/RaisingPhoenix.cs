using System;
using System.Threading.Tasks;

namespace Flatwhite.Hot
{
    /// <summary>
    /// Represent a raising phoenix. Calling reborn many time will not actually do anything until the method reborn complete and it will return <see cref="AlivePhoenix"/>
    /// </summary>
    internal class RaisingPhoenix : IPhoenixState
    {
        /// <summary>
        /// true if the phoenix is executing rebornAction. This is to avoid many call on method Reborn many time
        ///  </summary>

        private Task<IPhoenixState> _backGround;
        public IPhoenixState Reborn(Func<IPhoenixState> rebornAction)
        {
            if (_backGround == null || _backGround.Status == TaskStatus.Faulted || _backGround.Status == TaskStatus.Canceled)
            {
                _backGround = Task.Run(rebornAction);
                _backGround.ContinueWith(t => { /* To not make it bubble up the exception */}, TaskContinuationOptions.OnlyOnFaulted); 
            }
            
            if (_backGround.Status == TaskStatus.RanToCompletion && _backGround.Result != null)
            {
                return _backGround.Result.Reborn(rebornAction);
            }
            

            return this;
            
        }
    }
}