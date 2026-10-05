using System;
namespace Revive.Game
{
    public static class ScoreCalculator
    {
        public static int Apply(long current, int delta, int bonus, int maximum) => (int)Math.Max(0L, Math.Min(maximum, current + delta + bonus));
    }
}
