using Json2DiffSinger.Core.Models;
using OpenSvip.Model;
using System;
using System.Collections.Generic;

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
            return new List<DsParamNode>();
        }
    }
}
