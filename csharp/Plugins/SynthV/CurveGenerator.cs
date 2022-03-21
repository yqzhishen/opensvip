using System;
using System.Collections.Generic;
using System.Linq;

namespace Plugin.SynthV
{
    public class CurveGenerator
    {
        private readonly List<Tuple<int, int>> PointList;

        private readonly Func<double, double> Interpolation;

        public CurveGenerator(IEnumerable<Tuple<int, int>> pointList, Func<double, double> interpolation)
        {
            PointList = new List<Tuple<int, int>>();
            var currentPos = -1;
            var currentSum = 0;
            var overlapCount = 0;
            foreach (var (pos, val) in pointList)
            {
                if (pos == currentPos || currentPos < 0)
                {
                    currentSum += val;
                    ++overlapCount;
                }
                else
                {
                    PointList.Add(new Tuple<int, int>(currentPos, (int) Math.Round((double) currentSum / overlapCount)));
                    currentSum = val;
                    overlapCount = 1;
                }
                currentPos = pos;
            }

            if (currentPos != -1)
            {
                PointList.Add(new Tuple<int, int>(currentPos, (int) Math.Round((double) currentSum / overlapCount)));
            }
            Interpolation = interpolation;
        }

        public List<Tuple<int, int>> GetCurve(int step, int termination)
        {
            var result = new List<Tuple<int, int>>();
            if (!PointList.Any())
            {
                result.Add(new Tuple<int, int>(-192000, termination));
                result.Add(new Tuple<int, int>(1073741823, termination));
                return result;
            }
            var prevPoint = PointList[0];
            result.Add(new Tuple<int, int>(-192000, prevPoint.Item2));
            result.Add(new Tuple<int, int>(prevPoint.Item1, prevPoint.Item2));
            foreach (var currentPoint in PointList.Skip(1))
            {
                if (prevPoint.Item2 == termination && currentPoint.Item2 == termination)
                {
                    result.Add(new Tuple<int, int>(prevPoint.Item1, termination));
                    result.Add(new Tuple<int, int>(currentPoint.Item1, termination));
                }
                else
                {
                    for (var p = prevPoint.Item1 + step; p < currentPoint.Item1; p += step)
                    {
                        var r = Interpolation((double) (p - prevPoint.Item1) / (currentPoint.Item1 - prevPoint.Item1));
                        var v = (int) Math.Round((1 - r) * prevPoint.Item2 + r * currentPoint.Item2);
                        result.Add(new Tuple<int, int>(p, v));
                    }
                    result.Add(new Tuple<int, int>(currentPoint.Item1, currentPoint.Item2));
                }
                prevPoint = currentPoint;
            }
            result.Add(new Tuple<int, int>(prevPoint.Item1, prevPoint.Item2));
            result.Add(new Tuple<int, int>(1073741823, prevPoint.Item2));
            return result;
        }
    }
}
