using Microsoft.Xna.Framework;

namespace TrexRunner.Entities
{
    public interface IDayNightCycle
    {
        int NightCount { get; }
        bool IsNight { get; }

        Color ClearColor { get; }

    }
}
