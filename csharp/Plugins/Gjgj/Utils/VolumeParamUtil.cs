using System;
using System.Collections.Generic;
using System.Linq;
using Gjgj.Model;
using OpenSvip.Framework;
using OpenSvip.Library;
using OpenSvip.Model;

namespace Plugin.Gjgj
{
    public class VolumeParamUtil
    {
        /// <summary>
        /// 返回演唱轨的音量参数曲线。
        /// </summary>
        /// <param name="singingTrack">原始演唱轨。</param>
        /// <returns></returns>
        public List<GjVolumeParamPoint> EncodeVolumeParam(SingingTrack singingTrack, int paramSampleInterval)
        {
            List<GjVolumeParamPoint> gjVolumeParam = new List<GjVolumeParamPoint>();
            try
            {
                List<int> timeBuffer = new List<int>();
                List<double> valueBuffer = new List<double>();
                int time;
                double valueOrigin;
                double value;
                int lastTime = 0;
                ParamCurve paramCurve = ParamCurveUtils.ReduceSampleRate(singingTrack.EditedParams.Volume, paramSampleInterval);
                for (int index = 1; index < paramCurve.PointList.Count - 1; index++)
                {
                    time = GetVolumeParamPointTime(index, paramCurve);
                    valueOrigin = GetOriginalVolumeParamPointValue(index, paramCurve);
                    value = GetVolumeParamPointValue(valueOrigin);

                    if (lastTime != time)
                    {
                        if (valueOrigin != 0)
                        {
                            timeBuffer.Add(time);
                            valueBuffer.Add(value);
                        }
                        else
                        {
                            if (timeBuffer.Count == 0 || valueBuffer.Count == 0)
                            {

                            }
                            else
                            {
                                for (int bufferIndex = 0; bufferIndex < timeBuffer.Count; bufferIndex++)
                                {
                                    gjVolumeParam.Add(EncodeVolumeParamPoint(timeBuffer[bufferIndex], valueBuffer[bufferIndex]));
                                }
                                gjVolumeParam.Add(EncodeVolumeParamPoint(timeBuffer[0] - 5, 1.0));//左间断点
                                gjVolumeParam.Add(EncodeVolumeParamPoint(timeBuffer[timeBuffer.Count - 1] + 5, 1.0));//右间断点
                                timeBuffer.Clear();
                                valueBuffer.Clear();
                            }
                        }
                    }
                    lastTime = time;
                }
            }
            catch (Exception ex)
            {
                Warnings.AddWarning($"演唱轨的音量参数曲线转换失败，原因：{ex.Message}");
            }
            return gjVolumeParam;
        }

        /// <summary>
        /// 返回转换后的音量参数点的时间。
        /// </summary>
        /// <param name="index">参数点的索引。</param>
        /// <param name="paramCurve">原始参数曲线。</param>
        /// <returns></returns>
        private int GetVolumeParamPointTime(int index, ParamCurve paramCurve)
        {
            return paramCurve.PointList[index].Item1;
        }

        /// <summary>
        /// 返回原始音量参数点的值。
        /// </summary>
        /// <param name="index">参数点的索引。</param>
        /// <param name="paramCurve">原始参数曲线。</param>
        /// <returns></returns>
        private double GetOriginalVolumeParamPointValue(int index, ParamCurve paramCurve)
        {
            return paramCurve.PointList[index].Item2;
        }

        /// <summary>
        /// 根据时间和值返回音量参数点。
        /// </summary>
        /// <param name="time">时间。</param>
        /// <param name="value">值。</param>
        /// <returns></returns>
        private GjVolumeParamPoint EncodeVolumeParamPoint(double time, double value)
        {
            GjVolumeParamPoint gjVolumeParamPoint = new GjVolumeParamPoint
            {
                Time = time,
                Value = value
            };
            return gjVolumeParamPoint;
        }

        /// <summary>
        /// 返回转换后的音量参数点的值。
        /// </summary>
        /// <param name="volume">原始参数点的值</param>
        /// <returns></returns>
        private double GetVolumeParamPointValue(double volume)
        {
            return (volume + 1000.0) / 1000.0;
        }

        
        /// <summary>
        /// 转换音量参数。
        /// </summary>
        /// <param name="singingTrackIndex">演唱轨索引。</param>
        /// <returns></returns>
        public ParamCurve DecodeVolumeParam(int singingTrackIndex, List<GjVolumeParamPoint> VolumeParam)
        {
            ParamCurve paramCurve = new ParamCurve();
            try
            {
                List<double> timeBuffer = new List<double>();
                List<double> valueBuffer = new List<double>();
                int time;
                int value;
                for (int volumeParamPointIndex = 0; volumeParamPointIndex < VolumeParam.Count; volumeParamPointIndex++)
                {
                    time = GetVolumeParamTime(VolumeParam[volumeParamPointIndex].Time);
                    value = GetVolumeParamValue(VolumeParam[volumeParamPointIndex].Value);
                    Tuple<int, int> volumeParamPoint = Tuple.Create(time, value);
                    paramCurve.PointList.Add(volumeParamPoint);
                }

                paramCurve.PointList.OrderBy(x => x.Item1).ToList();
            }
            catch (Exception)
            {

            }
            return paramCurve;
        }
        
        /// <summary>
        /// 转换音量参数点的时间。
        /// </summary>
        /// <param name="origin">原始时间。</param>
        /// <returns></returns>
        private int GetVolumeParamTime(double origin)
        {
            return (int)origin;
        }

        /// <summary>
        /// 转换音量参数点的值。
        /// </summary>
        /// <param name="origin">原始值。</param>
        /// <returns></returns>
        private int GetVolumeParamValue(double origin)
        {
            return (int)origin * 1000 - 1000;
        }
    }
}