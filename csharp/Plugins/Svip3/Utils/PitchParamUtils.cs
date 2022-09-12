using Google.Protobuf.Collections;
using OpenSvip.Model;
using System;
using System.Collections.Generic;
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
                int offset = pattern.RealPos;
                curves.Add(DecodePatternPitchParam(pattern.EditedPitchLine, offset));
            }
            var pitchParamCurve = new ParamCurve();
            pitchParamCurve.PointList.Add(new Tuple<int, int>(-192000, -100));
            pitchParamCurve.PointList.AddRange(ParamCurveUtils.Merge(curves).PointList);
            pitchParamCurve.PointList.Add(new Tuple<int, int>(1073741823, -100));
            return pitchParamCurve;
        }
        private static ParamCurve DecodePatternPitchParam(RepeatedField<LineParamNode> nodes, int offset)
        {
            var curve = new ParamCurve();
            foreach (var node in nodes)
            {
                int pos = node.Pos + offset + TimeSignatureListUtils.FirstBarLength;
                int value = node.Value != -1.0f
                    ? (int)(node.Value * 100 - 50)
                    : -100;
                curve.PointList.Add(new Tuple<int, int>(pos, value));
            }
            return curve;
        }
    }
}
