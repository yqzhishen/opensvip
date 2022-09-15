using Json2DiffSinger.Core.Models;
using OpenSvip.Model;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Json2DiffSinger.Utils
{
    /// <summary>
    /// 用于转换音高参数的工具。
    /// </summary>
    public static class PitchParamUtils
    {
        /// <summary>
        /// 将 OpenSvip 的音高曲线转换成 ds 的音高曲线
        /// </summary>
        /// <param name="curve">音高曲线</param>
        /// <param name="timeStep">步长</param>
        /// <returns></returns>
        public static DsPitchParamCurve Encode(ParamCurve curve, float timeStep = 0.005f)
        {
            return new DsPitchParamCurve
            {
                F0TimeStepSize = timeStep,
                PointList = EncodePointList(curve.PointList)
            };
        }

        private static List<DsParamNode> EncodePointList(List<Tuple<int, int>> osPointList)
        {
            var posList = new List<int>();
            foreach (var point in osPointList)
            {
                posList.Add(point.Item1);
            }
            var dsPointList = new List<DsParamNode>();
            int curveEndPos = -1;
            try
            {
                curveEndPos = osPointList.Last(p => p.Item1 != 0).Item1;
            }
            catch
            {

            }
            if (curveEndPos < 0)
            {
                return null;
            }
            for (int samplePos = 1920; samplePos <= curveEndPos; samplePos += 5)
            {
                //float pitch = GetValueAt(samplePos, osPointList, posList) / 100.0f;
                float pitch = (float)CurveSegmentUtils.GetValueFromSegment(osPointList, samplePos);
                float freq = ToneUtils.ToneToFreq(pitch);
                dsPointList.Add(new DsParamNode
                {
                    Time = samplePos,
                    Value = freq
                });
            }
            return dsPointList;
        }

        private static int GetValueAt(int samplePos, List<Tuple<int, int>> osPointList, List<int> posList)
        {
            var satisfiedPoints = osPointList.Where(p => p.Item1 == samplePos && p.Item2 != -100);
            if (satisfiedPoints.Count() > 0)
            {
                return satisfiedPoints.First().Item2;
            }
            else
            {
                var leftPoint = osPointList.Where(p => p.Item1 < samplePos && p.Item2 != -100).Last();
                var rightPoint = osPointList.Where(p => p.Item1 > samplePos && p.Item2 != -100).First();
                int lp = leftPoint.Item1;
                int lv = leftPoint.Item2;
                int rp = rightPoint.Item1;
                int rv = rightPoint.Item2;
                return lv + (rv - lv) * (samplePos - lp) / (rp - lp);
            }
        }
    }
}
