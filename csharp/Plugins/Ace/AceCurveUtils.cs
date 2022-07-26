using System;
using System.Collections.Generic;
using System.Linq;
using AceStdio.Model;
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
        /// Min-Max 标准化至 [-radius, radius] 区间。
        /// </summary>
        public static List<AceParamCurve> Normalize(this List<AceParamCurve> curves, double radius)
        {
            if (!curves.Any())
            {
                return curves;
            }
            var min = curves.Min(curve => curve.Values.Min());
            var max = curves.Max(curve => curve.Values.Max());
            return Math.Abs(max - min) < 1e-6
                ? curves.ConvertAll(curve => curve.Transform(x => 0))
                : curves.ConvertAll(curve => curve.Transform(x => radius * (2 * (x - min) / (max - min) - 1)));
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