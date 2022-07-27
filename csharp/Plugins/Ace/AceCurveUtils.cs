using System;
using System.Collections.Generic;
using System.Linq;
using AceStdio.Model;
using LinqStatistics;
using OpenSvip.Library;

namespace AceStdio.Utils
{
    public static class AceCurveUtils
    {
        public static List<AceParamCurve> Plus(
            this List<AceParamCurve> self,
            List<AceParamCurve> others,
            double defaultValue,
            Func<double, double> transform)
        {
            if (!others.Any())
            {
                return self;
            }

            if (!self.Any())
            {
                return others.ConvertAll(curve => curve.Transform(transform));
            }

            var resultRanges = Range.Create(self
                    .Concat(others)
                    .Select(curve => new Tuple<int, int>(curve.Offset, curve.Offset + curve.Values.Count))
                    .ToArray())
                .SubRanges();
            var resultCurveList = new List<AceParamCurve>();
            foreach (var (start, end) in resultRanges)
            {
                var resultCurve = new AceParamCurve
                {
                    Offset = start,
                    Values = new List<double>(new double[end - start])
                };
                foreach (var selfCurve in self.Where(curve => curve.Offset >= start && curve.Offset < end))
                {
                    var index = selfCurve.Offset - start;
                    foreach (var value in selfCurve.Values)
                    {
                        resultCurve.Values[index++] = value;
                    }
                }

                foreach (var anotherCurve in others.Where(curve => curve.Offset >= start && curve.Offset < end))
                {
                    var index = anotherCurve.Offset - start;
                    foreach (var value in anotherCurve.Values)
                    {
                        if (resultCurve.Values[index] == 0)
                        {
                            resultCurve.Values[index] = defaultValue;
                        }

                        resultCurve.Values[index] += transform(value);
                        ++index;
                    }
                }

                resultCurveList.Add(resultCurve);
            }

            return resultCurveList;
        }

        /// <summary>
        /// 剔除符合 predicate 条件的参数点。
        /// </summary>
        public static List<AceParamCurve> Exclude(this List<AceParamCurve> curves, Func<double, bool> predicate)
        {
            var result = new List<AceParamCurve>();
            foreach (var curve in curves)
            {
                var buffer = new List<double>();
                var pos = curve.Offset;
                foreach (var value in curve.Values)
                {
                    ++pos;
                    if (predicate(value))
                    {
                        if (buffer.Any())
                        {
                            result.Add(new AceParamCurve
                            {
                                Offset = pos - buffer.Count,
                                Values = buffer
                            });
                            buffer = new List<double>();
                        }
                    }
                    else
                    {
                        buffer.Add(value);
                    }
                }

                if (buffer.Any())
                {
                    result.Add(new AceParamCurve
                    {
                        Offset = pos - buffer.Count,
                        Values = buffer
                    });
                }
            }

            return result;
        }

        /// <summary>
        /// Z-Score 标准化并缩放标准差为 d、平移均值为 b。
        /// </summary>
        public static List<AceParamCurve> ZScoreNormalize(this List<AceParamCurve> curves, double d = 1, double b = 0)
        {
            if (!curves.Any())
            {
                return curves;
            }

            var points = curves.Aggregate(
                    (IEnumerable<double>) Array.Empty<double>(),
                    (current, next) => current.Concat(next.Values))
                .ToArray();
            var miu = points.Average();
            var sigma = points.StandardDeviationP();
            return curves.ConvertAll(curve => curve.Transform(x => (x - miu) / sigma * d + b));
        }

        /// <summary>
        /// Min-Max 标准化至 [-r+b, r+b] 区间。
        /// </summary>
        public static List<AceParamCurve> MinMaxNormalize(this List<AceParamCurve> curves, double r = 1, double b = 0)
        {
            if (!curves.Any())
            {
                return curves;
            }

            var minmax = curves
                .Aggregate(
                    (IEnumerable<double>) Array.Empty<double>(),
                    (current, next) => current.Concat(next.Values))
                .MinMax();
            var min = minmax.Min;
            var max = minmax.Max;
            return Math.Abs(max - min) < 1e-3
                ? curves.ConvertAll(curve => curve.Transform(x => 0))
                : curves.ConvertAll(curve => curve.Transform(x => r * (2 * (x - min) / (max - min) - 1) + b));
        }

        private static AceParamCurve Transform(this AceParamCurve curve, Func<double, double> valueTransform)
        {
            return new AceParamCurve
            {
                Offset = curve.Offset,
                Values = curve.Values.ConvertAll(x => valueTransform(x))
            };
        }
    }
}