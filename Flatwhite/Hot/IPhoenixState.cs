using System;
using System.Threading.Tasks;

namespace Flatwhite.Hot
{
    /// <summary>
    /// A state of a phoenix to control how to reborn/die. I'm using State pattern here
    /// </summary>
    public interface IPhoenixState
    {
        /// <summary>
        /// Reborn the phoenix
        /// </summary>
        /// <param name="rebornAction"></param>
        /// <returns></returns>
        IPhoenixState Reborn(Func<Task<IPhoenixState>> rebornAction);
    }
}