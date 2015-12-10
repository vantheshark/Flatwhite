using System;
using System.Threading.Tasks;

namespace Flatwhite.Hot
{
    /// <summary>
    /// The state is inactive, it can die or reborn
    /// </summary>
    internal class InActivePhoenix : IPhoenixState
    {
        private readonly DateTime _createdTime;

        public InActivePhoenix()
        {
            _createdTime = DateTime.UtcNow;
        }

        /// <summary>
        /// Change the state to RaisingPhoenix and call methdo Reborn
        /// </summary>
        /// <param name="rebornAction"></param>
        /// <returns></returns>
        public IPhoenixState Reborn(Func<Task<IPhoenixState>> rebornAction)
        {
            IPhoenixState phoenixState = new RaisingPhoenix();
            return phoenixState.Reborn(rebornAction);
        }

        public string GetState()
        {
            return $"inactive for {DateTime.UtcNow.Subtract(_createdTime).TotalSeconds:N1} seconds";
        }
    }
}