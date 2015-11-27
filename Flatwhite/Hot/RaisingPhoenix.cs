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
            if (_backGround != null && _backGround.Status != TaskStatus.RanToCompletion)
            {
                return this;
            }

            try
            {
                if (_backGround == null)
                {
                    _backGround = Task.Run(rebornAction);
                }

                if (_backGround.Status == TaskStatus.RanToCompletion)
                {
                    return _backGround.Result.Reborn(rebornAction);
                }

                return this;
            }
            catch (Exception)
            {
                return this;
            }
        }
    }
}