using System;

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
        private volatile bool _isOnFire;
        public IPhoenixState Reborn(Func<IPhoenixState> rebornAction)
        {
            if (_isOnFire)
            {
                return this;
            }

            _isOnFire = true;

            try
            {
                return rebornAction();
            }
            catch (Exception)
            {
                return this;
            }
        }
    }
}