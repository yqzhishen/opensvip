using System;

namespace SynthV.Param
{
    public static class Interpolation
    {
        public static Func<double, double> LinearInterpolation()
        {
            return x => x;
        }

        public static Func<double, double> CubicInterpolation()
        {
            return x => x * x * (3 - 2 * x);
        }

        public static Func<double, double> CosineInterpolation()
        {
            return x => (1 - Math.Cos(Math.PI * x)) / 2;
        }

        public static Func<double, double> SigmoidInterpolation(double k)
        {
            return x => 1 / (1 + Math.Exp(k * (-2 * x + 1)));
        }
    }
}
