using System;
using System.Collections.Generic;
using OpenSvip.Model;

namespace OpenSvip.Library
{
    public static class ParamCurveUtils
    {
        /// <summary>
        /// 合并曲线中的部分参数点以降低参数曲线的采样率。
        /// </summary>
        /// <param name="curve">需要执行操作的参数曲线。</param>
        /// <param name="interval">设定采样间隔，单位为梯。每个采样间隔范围内的所有参数点将进行均值合并。此值为零或负值时不执行任何操作。</param>
        /// <param name="termination">设定曲线的参数间断值。间断点将不会被删除或合并。</param>
        public static ParamCurve ReduceSampleRate(this ParamCurve curve, int interval, int termination = 0)
        {
            if (interval <= 0)
            {
                return curve;
            }
            var points = curve.PointList;
            if (points.Count <= 1)
            {
                return curve;
            }

            var result = new List<Tuple<int, int>>();
            var xSum = 0;
            var ySum = 0;
            var mergeCount = 0;
            var prevPoint = points[0];
            var prevPointAdded = false;
            for (var i = 1; i < points.Count; ++i)
            {
                prevPointAdded = false;
                var currentPoint = points[i];
                if (currentPoint.Item2 == termination)
                {
                    result.Add(prevPoint);
                    prevPoint = currentPoint;
                    continue;
                }
                if (prevPoint.Item2 == termination)
                {
                    result.Add(prevPoint);
                    result.Add(currentPoint);
                    ++i;
                    if (i < points.Count)
                    {
                        prevPoint = points[i];
                    }
                    prevPointAdded = true;
                    continue;
                }
                
                var pos = prevPoint.Item1;
                do
                {
                    currentPoint = points[i];
                    xSum += prevPoint.Item1;
                    ySum += prevPoint.Item2;
                    ++mergeCount;
                    prevPoint = currentPoint;
                    ++i;
                } while (i < points.Count && points[i].Item1 < pos + interval && points[i].Item2 != termination);
                
                result.Add(new Tuple<int, int>(
                    (int) Math.Round((double) xSum / mergeCount),
                    (int) Math.Round((double) ySum / mergeCount)));
                xSum = 0;
                ySum = 0;
                mergeCount = 0;
                if (i >= points.Count)
                {
                    break;
                }
                --i;
            }
            if (!prevPointAdded)
            {
                result.Add(prevPoint);
            }
            return new ParamCurve
            {
                PointList = result
            };
        }
    }
}
