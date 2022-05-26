using System;
using System.Collections.Generic;
using FlutyDeer.GjgjPlugin.Model;
using OpenSvip.Model;

namespace FlutyDeer.GjgjPlugin
{
    public class PitchParamUtil
    {
        /// <summary>
        /// 返回演唱轨的音高参数曲线。
        /// </summary>
        /// <param name="singingTrack">原始演唱轨。</param>
        /// <returns></returns>
        public GjPitchParam EncodePitchParam(SingingTrack singingTrack)
        {
            List<double> timeBuffer = new List<double>();
            List<double> valueBuffer = new List<double>();
            int lastTimeOrigin = -100;
            int timeOrigin;
            int valueOrigin;
            List<GjPitchParamPoint> pitchParamPointList = new List<GjPitchParamPoint>();
            List<GjModifyRange> modifyRangeList = new List<GjModifyRange>();
            for (int index = 0; index < singingTrack.EditedParams.Pitch.PointList.Count; index++)
            {
                timeOrigin = GetOriginalPitchParamPointTime(index, singingTrack);
                valueOrigin = GetOriginalPitchParamPointValue(index, singingTrack);

                if (lastTimeOrigin != timeOrigin)
                {
                    if (valueOrigin != -100 && timeOrigin != -192000 && timeOrigin != 1073741823)
                    {
                        timeBuffer.Add(GetPitchParamPointTime(timeOrigin));
                        valueBuffer.Add(GetPitchParamPointValue(valueOrigin));
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
                                pitchParamPointList.Add(EncodePitchParamPoint(timeBuffer[bufferIndex], valueBuffer[bufferIndex]));
                            }
                            modifyRangeList.Add(EncodeModifyRange(timeBuffer[0], timeBuffer[valueBuffer.Count - 1]));
                            timeBuffer.Clear();
                            valueBuffer.Clear();
                        }
                    }
                }
                lastTimeOrigin = timeOrigin;
            }
            GjPitchParam gjPitchParam = new GjPitchParam
            {
                PitchParamPointList = pitchParamPointList,
                ModifyRangeList = modifyRangeList
            };
            return gjPitchParam;
        }

        /// <summary>
        /// 返回原始音高参数点的时间。
        /// </summary>
        /// <param name="index">参数点的索引。</param>
        /// <param name="singingTrack">原始演唱轨。</param>
        /// <returns></returns>
        private int GetOriginalPitchParamPointTime(int index, SingingTrack singingTrack)
        {
            return singingTrack.EditedParams.Pitch.PointList[index].Item1;
        }

        /// <summary>
        /// 返回原始音高参数点的值。
        /// </summary>
        /// <param name="index">参数点的索引。</param>
        /// <param name="singingTrack">原始演唱轨。</param>
        /// <returns></returns>
        private int GetOriginalPitchParamPointValue(int index, SingingTrack singingTrack)
        {
            return singingTrack.EditedParams.Pitch.PointList[index].Item2;
        }

        /// <summary>
        /// 根据时间和值返回音高参数点。
        /// </summary>
        /// <param name="time">时间。</param>
        /// <param name="value">值。</param>
        /// <returns></returns>
        private GjPitchParamPoint EncodePitchParamPoint(double time, double value)
        {
            GjPitchParamPoint gjPitchParamPoint = new GjPitchParamPoint
            {
                Time = time,
                Value = value
            };
            return gjPitchParamPoint;
        }

        /// <summary>
        /// 根据改动区间的端点返回一个改动区间（ModifyRange）。
        /// </summary>
        /// <param name="left">左端点。</param>
        /// <param name="right">右端点。</param>
        /// <returns></returns>
        private GjModifyRange EncodeModifyRange(double left, double right)
        {
            GjModifyRange gjModifyRange = new GjModifyRange
            {
                Left = left,
                Right = right
            };
            return gjModifyRange;
        }

        /// <summary>
        /// 返回转换后的音高参数点的时间。
        /// </summary>
        /// <param name="origin">原始时间。</param>
        /// <returns></returns>
        private double GetPitchParamPointTime(double origin)
        {
            return origin / 5.0;
        }

        /// <summary>
        /// 返回转换后的音高参数点的值。
        /// </summary>
        /// <param name="origin">原始值。</param>
        /// <returns></returns>
        private double GetPitchParamPointValue(double origin)
        {
            return ToneToY((double)((origin) / 100.0));
        }

        
        /// <summary>
        /// 将音高转换为Y值。
        /// </summary>
        /// <param name="tone"></param>
        /// <returns></returns>
        private double ToneToY(double tone)
        {
            return (127 - tone + 0.5) * 18.0;
        }

        /// <summary>
        /// 转换修改部分的音高参数。
        /// </summary>
        /// <param name="singingTrackIndex">演唱轨索引。</param>
        public ParamCurve DecodePitchParam(int singingTrackIndex, GjProject gjProject)
        {
            ParamCurve paramCurvePitch = new ParamCurve();
            Tuple<int, int> defaultLeftEndpoint = Tuple.Create(-192000, -100);
            paramCurvePitch.PointList.Add(defaultLeftEndpoint);

            try
            {
                var index = -1;
                foreach (var range in gjProject.SingingTrackList[singingTrackIndex].PitchParam.ModifyRangeList)
                {
                    Tuple<int, int> leftEndpoint = Tuple.Create(GetPitchParamTime(range.Left), -100);//左间断点
                    Tuple<int, int> rightEndpoint = Tuple.Create(GetPitchParamTime(range.Right), -100);//右间断点
                    paramCurvePitch.PointList.Add(leftEndpoint);//添加左间断点
                    index = gjProject.SingingTrackList[singingTrackIndex].PitchParam.PitchParamPointList.FindIndex(index + 1, p => p.Time >= range.Left && p.Value <= range.Right);
                    if (index == -1)
                        continue;
                    for (; (index < gjProject.SingingTrackList[singingTrackIndex].PitchParam.PitchParamPointList.Count) && (gjProject.SingingTrackList[singingTrackIndex].PitchParam.PitchParamPointList[index].Time <= range.Right); ++index)
                    {
                        int pitchParamTime = GetPitchParamTime(gjProject.SingingTrackList[singingTrackIndex].PitchParam.PitchParamPointList[index].Time);
                        int pitchParamValue = GetPitchParamValue(gjProject.SingingTrackList[singingTrackIndex].PitchParam.PitchParamPointList[index].Value);
                        Tuple<int, int> pitchParamPoint = Tuple.Create(pitchParamTime, pitchParamValue);
                        paramCurvePitch.PointList.Add(pitchParamPoint);
                    }
                    paramCurvePitch.PointList.Add(rightEndpoint);//添加右间断点
                }
            }
            catch (Exception)
            {

            }

            Tuple<int, int> defaultRightEndpoint = Tuple.Create(1073741823, -100);
            paramCurvePitch.PointList.Add(defaultRightEndpoint);
            return paramCurvePitch;
        }
        
        /// <summary>
        /// 将Y值转换为音高。
        /// </summary>
        /// <param name="y">Y值。</param>
        /// <returns></returns>
        private double YToTone(double y)
        {
            return 127 + 0.5 - y / 18.0;
        }

        /// <summary>
        /// 转换音高参数点的时间。
        /// </summary>
        /// <param name="origin">原始时间。</param>
        /// <returns></returns>
        private int GetPitchParamTime(double origin)
        {
            return (int)(origin * 5.0);
        }

        /// <summary>
        /// 转换音高参数点的值。
        /// </summary>
        /// <param name="origin">原始值。</param>
        /// <returns></returns>
        private int GetPitchParamValue(double origin)
        {
            return (int)(YToTone(origin) * 100.0);
        }

    }
}