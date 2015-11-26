using System;

namespace Flatwhite.Hot
{
    /// <summary>
    /// The state is alive, it can die or reborn
    /// </summary>
    internal class AlivePhoenix : IPhoenixState
    {
        /// <summary>
        /// Change the state to RaisingPhoenix and call methdo Reborn
        /// </summary>
        /// <param name="rebornAction"></param>
        /// <returns></returns>
        public IPhoenixState Reborn(Func<IPhoenixState> rebornAction)
        {
            IPhoenixState phoenixState = new RaisingPhoenix();
            return phoenixState.Reborn(rebornAction);
        }
    }
}