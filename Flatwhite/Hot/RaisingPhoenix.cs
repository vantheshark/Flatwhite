using System;

namespace Flatwhite.Hot
{
    /// <summary>
    /// Represent a raising phoenix. Calling reborn many time will not actually do anything until the method reborn complete and it will return <see cref="AlivePhoenix"/>
    /// </summary>
    internal class RaisingPhoenix : IPhoenixState
    {
        private bool _isReborn;
        public IPhoenixState Reborn(Func<IPhoenixState> rebornAction)
        {
            if (_isReborn)
            {
                return this;
            }

            _isReborn = true;

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