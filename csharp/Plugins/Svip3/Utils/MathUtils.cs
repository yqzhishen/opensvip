using System;

namespace FlutyDeer.Svip3Plugin.Utils
{
    public static class MathUtils
    {
        public static double ToLinearVolume(double gain)
        {
            return gain >= 0
                ? Math.Min(gain / (20 * Math.Log10(4)) + 1.0, 2.0)
                : Math.Pow(10, gain / 20.0);
        }

        public static float ToDecibelVolume(double volume)
        {
            if (volume > 0)
                return (float)Math.Max(20 * Math.Log10(volume > 0.01 ? volume : 0.01), -70.0);
            else
                return -70;//-infinity
        }
    }
}
