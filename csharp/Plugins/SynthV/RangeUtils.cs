using System;
using System.Linq;
using OpenSvip.Library;
using SynthV.Model;

namespace Plugin.SynthV
{
    public static class RangeUtils
    {
        public static Range EditedRange(this SVParamCurve curve, double defaultValue = 0.0)
        {
            const double tolerance = 1e-6;
            var range = Range.Create();
            var points = curve.Points.ConvertAll(
                point => new Tuple<int, double>(PositionToTicks(point.Item1), point.Item2));
            if (!points.Any())
            {
                return range;
            }
            if (points.Count == 1)
            {
                return Math.Abs(points[0].Item2 - defaultValue) < tolerance ? range : Range.Create(new Tuple<int, int>(0, int.MaxValue / 2));
            }
            if (Math.Abs(points[0].Item2 - defaultValue) > tolerance)
            {
                range |= Range.Create(new Tuple<int, int>(0, points[0].Item1));
            }
            var start = points[0].Item1;
            var end = points[0].Item1;
            for (var i = 1; i < points.Count; i++)
            {
                if (Math.Abs(points[i - 1].Item2 - defaultValue) < tolerance && Math.Abs(points[i].Item2 - defaultValue) < tolerance)
                {
                    if (start < end)
                    {
                        range |= Range.Create(new Tuple<int, int>(start, end));
                    }
                    start = points[i].Item1;
                }
                else
                {
                    end = points[i].Item1;
                }
            }
            if (start < end)
            {
                range |= Range.Create(new Tuple<int, int>(start, end));
            }
            if (Math.Abs(points.Last().Item2 - defaultValue) > tolerance)
            {
                range |= Range.Create(new Tuple<int, int>(points.Last().Item1, int.MaxValue / 2));
            }
            return range;
        }

        public static Range CoverRange(this SVNote note)
        {
            return Range.Create(new Tuple<int, int>(
                PositionToTicks(note.Onset),
                PositionToTicks(note.Onset + note.Duration)));
        }

        private static int PositionToTicks(long position)
        {
            return (int) Math.Round(position / 1470000.0);
        }
    }
}