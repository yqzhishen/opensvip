using System;

namespace Plugin.SynthV
{
    public class PitchInterpolation
    {
        public readonly double MaxInterTimeInSecs;

        public readonly double MaxInterTimePercent;

        private readonly Func<double, double> InterFunc;
        
        public PitchInterpolation(double maxInterTimeInSecs, double maxInterTimePercent, Func<double, double> interFunc)
        {
            MaxInterTimeInSecs = maxInterTimeInSecs;
            MaxInterTimePercent = maxInterTimePercent;
            InterFunc = interFunc;
        }

        public double Apply(double value)
        {
            return InterFunc(value);
        }

        public static PitchInterpolation CosineInterpolation()
        {
            return new PitchInterpolation(0.05, 0.1,
                x => (1 - Math.Cos(Math.PI * x)) / 2);
        }

        public static PitchInterpolation PolyInterpolation()
        {
            return new PitchInterpolation(0.05, 0.1,
                x => x * x * (3 - 2 * x));
        }

        public static PitchInterpolation SigmoidInterpolation()
        {
            return new PitchInterpolation(0.075, 0.5,
                x => 1 / (1 + Math.Exp(5.5 * (-2 * x + 1))));
        }
    }
}
