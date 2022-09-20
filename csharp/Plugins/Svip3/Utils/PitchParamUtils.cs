using FlutyDeer.Svip3Plugin.Model;
using OpenSvip.Library;
using OpenSvip.Model;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FlutyDeer.Svip3Plugin.Utils
{
    public static class PitchParamUtils
    {
        #region Decoding
        public static ParamCurve Decode(List<Xs3SingingPattern> patterns)
        {
            var curves = new List<ParamCurve>();
            foreach (var pattern in patterns)
            {
                curves.Add(DecodePatternCurve(pattern));
            }
            var curve = new ParamCurve();
            curve.PointList.Add(new Tuple<int, int>(-192000, -100));
            curve.PointList.AddRange(ParamCurveUtils.Merge(curves).PointList);
            curve.PointList.Add(new Tuple<int, int>(1073741823, -100));
            return curve;
        }

        private static ParamCurve DecodePatternCurve(Xs3SingingPattern pattern)
        {
            var curve = new ParamCurve();
            int offset = pattern.OriginalStartPosition + TimeSignatureListUtils.FirstBarLength;
            var range = PatternUtils.GetVisiableRange(pattern);
            var visiableNodes = pattern.PitchParam.Where(n => n.Position + offset >= range.Item1 + TimeSignatureListUtils.FirstBarLength
                                                                   && n.Position + offset <= range.Item2 + TimeSignatureListUtils.FirstBarLength);
            foreach (var node in visiableNodes)
            {
                int pos = node.Position + offset;
                int val = (node.Value != -1.0f)
                    ? (int)(node.Value * 100 - 50)
                    : -100;
                curve.PointList.Add(new Tuple<int, int>(pos, val));
            }
            return curve;
        }

        #endregion

        #region Encoding

        public static List<Xs3ParamPoint> Encode(ParamCurve curve)
        {
            int offset = TimeSignatureListUtils.FirstBarLength;
            var points = new List<Xs3ParamPoint>();
            var splitedCurves = curve.SplitIntoSegments(-100);
            foreach (var segment in splitedCurves)
            {
                if (!segment.Any())
                    continue;
                int prevPos = 0;
                bool leftInterrupt = false;
                foreach (var point in segment)
                {
                    if (point.Item1 == prevPos)
                        continue;
                    if (point.Item1 < offset)
                    {
                        continue;
                    }
                    else
                    {
                        if (!leftInterrupt)
                        {
                            points.Add(new Xs3ParamPoint
                            {
                                Position = (point.Item1 - offset),
                                Value = -1
                            });
                            leftInterrupt = true;
                        }
                    }
                    int pos = point.Item1 - offset;
                    float val = (point.Item2 + 50) / 100.0f;
                    points.Add(new Xs3ParamPoint
                    {
                        Position = pos,
                        Value = val
                    });
                    prevPos = point.Item1;
                }
                var lastPoint = segment.Last();
                points.Add(new Xs3ParamPoint
                {
                    Position = (lastPoint.Item1 - offset),
                    Value = -1
                });
            }
            return points;
        }

        #endregion
    }
}
