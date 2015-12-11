using System;
using System.Threading.Tasks;

namespace Flatwhite.Hot
{
    /// <summary>
    /// Represent a raising phoenix. Calling reborn many time will not actually do anything until the method reborn complete and it will return the value returned from rebornAction
    /// </summary>
    internal class RaisingPhoenix : IPhoenixState
    {
        private IPhoenixState _newPhoenixState;
        private bool _startable = true;
        /// <summary>
        /// true if the phoenix is executing rebornAction. This is to avoid many call on method Reborn many time
        ///  </summary>


        public IPhoenixState Reborn(Func<Task<IPhoenixState>> rebornAction)
        {
            if (_newPhoenixState != null)
            {
                return _newPhoenixState;
            }

            if (!_startable)
            {
                return this;
            }

            _startable = false;
            Func<Task<IPhoenixState>> wrapper = async () =>
            {
                try
                {
                    var t = rebornAction();
                    _newPhoenixState = await t;
                }
                catch
                {
                    _startable = true;
                }

                return _newPhoenixState;
            };
            Global.BackgroundTaskManager.Run(wrapper);
            
            return this;
        }

        public string GetState()
        {
            return _startable 
                ? "wait to raise"
                : "raising";
        }
    }
}