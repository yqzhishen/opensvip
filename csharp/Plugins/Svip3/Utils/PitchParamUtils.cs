using Google.Protobuf.Collections;
using OpenSvip.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using Xstudio.Proto;

namespace FlutyDeer.Svip3Plugin.Utils
{
    public static class PitchParamUtils
    {
        public static ParamCurve Decode(RepeatedField<SingingPattern> patterns)
        {
            var curves = new List<ParamCurve>();
            foreach (var pattern in patterns)
            {
                curves.Add(DecodePatternCurve(pattern));
            }
            var pitchParamCurve = new ParamCurve();
            pitchParamCurve.PointList.Add(new Tuple<int, int>(-192000, -100));
            pitchParamCurve.PointList.AddRange(ParamCurveUtils.Merge(curves).PointList);
            pitchParamCurve.PointList.Add(new Tuple<int, int>(1073741823, -100));
            return pitchParamCurve;
        }

        private static ParamCurve DecodePatternCurve(SingingPattern pattern)
        {
            var curve = new ParamCurve();
            int offset = pattern.RealPos + TimeSignatureListUtils.FirstBarLength;
            var range = PatternUtils.GetVisiableRange(pattern);
            var visiableNodes = pattern.EditedPitchLine.Where(n => n.Pos + offset >= range.Item1 + TimeSignatureListUtils.FirstBarLength
                                                                   && n.Pos + offset <= range.Item2 + TimeSignatureListUtils.FirstBarLength);
            foreach (var node in visiableNodes)
            {
                int pos = node.Pos + offset;
                int value = node.Value != -1.0f
                    ? (int)(node.Value * 100 - 50)
                    : -100;
                curve.PointList.Add(new Tuple<int, int>(pos, value));
            }
            return curve;
        }
    }
}
