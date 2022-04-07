using System;
using System.Collections.Generic;
using System.Linq;

namespace SynthV.Param
{
    public class CurveGenerator : ParamExpression
    {
        public List<Tuple<int, int>> PointList { get; }

        private readonly Func<double, double> Interpolation;

        private readonly int BaseValue;

        public CurveGenerator(IEnumerable<Tuple<int, int>> pointList, Func<double, double> interpolation, int baseValue = 0)
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
            BaseValue = baseValue;
        }

        public override int ValueAtTicks(int ticks)
        {
            if (!PointList.Any())
            {
                return BaseValue;
            }
            var index = PointList.FindLastIndex(point => point.Item1 <= ticks);
            if (index == -1)
            {
                return PointList[0].Item2;
            }
            if (index == PointList.Count - 1)
            {
                return PointList.Last().Item2;
            }
            var r = Interpolation(
                (double) (ticks - PointList[index].Item1) / (PointList[index + 1].Item1 - PointList[index].Item1));
            return (int) Math.Round((1 - r) * PointList[index].Item2 + r * PointList[index + 1].Item2);
        }

        public List<Tuple<int, int>> GetConvertedCurve(int step)
        {
            var result = new List<Tuple<int, int>>();
            if (!PointList.Any())
            {
                result.Add(new Tuple<int, int>(-192000, BaseValue));
                result.Add(new Tuple<int, int>(1073741823, BaseValue));
                return result;
            }
            var prevPoint = PointList[0];
            result.Add(new Tuple<int, int>(-192000, prevPoint.Item2));
            result.Add(new Tuple<int, int>(prevPoint.Item1, prevPoint.Item2));
            foreach (var currentPoint in PointList.Skip(1))
            {
                if (prevPoint.Item2 == BaseValue && currentPoint.Item2 == BaseValue)
                {
                    result.Add(new Tuple<int, int>(prevPoint.Item1, BaseValue));
                    result.Add(new Tuple<int, int>(currentPoint.Item1, BaseValue));
                }
                else
                {
                    for (var p = prevPoint.Item1 + step; p < currentPoint.Item1; p += step)
                    {
                        var r = Interpolation((double) (p - prevPoint.Item1) / (currentPoint.Item1 - prevPoint.Item1));
                        var v = (int) Math.Round((1 - r) * prevPoint.Item2 + r * currentPoint.Item2);
                        result.Add(new Tuple<int, int>(p, v));
                    }
                }
                prevPoint = currentPoint;
            }
            result.Add(new Tuple<int, int>(prevPoint.Item1, prevPoint.Item2));
            result.Add(new Tuple<int, int>(1073741823, prevPoint.Item2));
            return result;
        }
    }
}
