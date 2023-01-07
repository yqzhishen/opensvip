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
        /// <param name="end">终止位置，通常为最后一个音符的尾部位置</param>
        /// <param name="timeStep">步长</param>
        /// <returns></returns>
        public static DsPitchParamCurve Encode(ParamCurve curve, int end, float timeStep = 0.005f)
        {
            end += 1920;
            return new DsPitchParamCurve
            {
                F0TimeStepSize = timeStep,
                PointList = EncodePointList(curve.PointList, end)
            };
        }

        private static List<DsParamNode> EncodePointList(List<Tuple<int, int>> osPointList, int end)
        {
            var validPoints = osPointList
                .Where(p => p.Item1 - 10 >= 1920 && p.Item1 + 10 < end && p.Item2 >= 0)
                .ToList();
            if (!validPoints.Any())
            {
                throw new Exception("源文件缺少音高参数，请使用 X Studio Pro 冻结音高参数后重试。");
            }
            var dsPointList = new List<DsParamNode>();
            for (var pos = 1920; pos < end; pos += 5)
            {
                dsPointList.Add(new DsParamNode
                {
                    Time = pos / 1000.0,
                    Value = ToneUtils.ToneToFreq(validPoints.ValueAt(pos) / 100.0)
                });
            }
            /*
            var posList = new List<int>();
            foreach (var point in osPointList)
            {
                posList.Add(point.Item1);
            }
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
            */
            return dsPointList;
        }
        
        private static double ValueAt(this List<Tuple<int, int>> segment, double ticks)
        {
            var leftPoint = segment.BinaryFindLast(p => p.Item1 <= ticks);
            if (leftPoint == null)
            {
                return segment.First().Item2;
            }

            var rightPoint = segment.BinaryFindFirst(p => p.Item1 > ticks);
            if (rightPoint == null)
            {
                return segment.Last().Item2;
            }

            var ratio = (ticks - leftPoint.Item1) / (rightPoint.Item1 - leftPoint.Item1);
            return (1 - ratio) * leftPoint.Item2 + ratio * rightPoint.Item2;
        }
    }
}
