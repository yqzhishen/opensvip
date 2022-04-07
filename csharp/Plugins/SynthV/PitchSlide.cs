using System;

namespace SynthV.Param
{
    public class PitchSlide
    {
        public readonly double MaxInterTimeInSecs;

        public readonly double MaxInterTimePercent;

        private readonly Func<double, double> InterFunc;
        
        private PitchSlide(double maxInterTimeInSecs, double maxInterTimePercent, Func<double, double> interFunc)
        {
            MaxInterTimeInSecs = maxInterTimeInSecs;
            MaxInterTimePercent = maxInterTimePercent;
            InterFunc = interFunc;
        }

        public double Apply(double value)
        {
            return InterFunc(value);
        }

        public static PitchSlide CosineSlide()
        {
            return new PitchSlide(0.05, 0.1,
                Interpolation.CosineInterpolation());
        }

        public static PitchSlide CubicSlide()
        {
            return new PitchSlide(0.05, 0.1,
                Interpolation.CubicInterpolation());
        }

        public static PitchSlide SigmoidSlide()
        {
            return new PitchSlide(0.075, 0.48,
                Interpolation.SigmoidInterpolation(5.5));
        }

        public static PitchSlide CustomSlide(
            double maxInterTimeInSecs,
            double maxInterTimePercent,
            Func<double, double> interFunc)
        {
            return new PitchSlide(maxInterTimeInSecs, maxInterTimePercent, interFunc);
        }
    }
}
