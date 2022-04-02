using System;
using System.IO;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Linq;
using OpenSvip.Model;
using OpenSvip.Library;
using Gjgj.Model;

namespace Plugin.Gjgj
{
    public class GjgjDecoder
    {
        public Project DecodeProject(GjProject gjProject)
        {
            var project = new Project
            {
                Version = GetSvipProjectVersion()
            };
            DecodeTempo(gjProject, project);
            TimeSynchronizer timeSynchronizer = new TimeSynchronizer(project.SongTempoList);
            DecodeTimeSignature(gjProject, project);
            DecodeTracks(timeSynchronizer, gjProject, project);
            return project;

        }

        private void DecodeTracks(TimeSynchronizer timeSynchronizer, GjProject gjProject, Project project)
        {
            DecodeSingingTracks(timeSynchronizer, gjProject, project);
            DecodeInstrumentalTracks(gjProject, project);
        }

        private static void DecodeInstrumentalTracks(GjProject gjProject, Project project)
        {
            for (int instrumentalTrackIndex = 0; instrumentalTrackIndex < gjProject.Instrumental.Count; instrumentalTrackIndex++)
            {
                Track svipTrack = new InstrumentalTrack
                {
                    Title = GetInstrumentalName(gjProject, instrumentalTrackIndex),
                    Mute = GetInstrumentalMute(gjProject, instrumentalTrackIndex),
                    Solo = false,
                    Volume = 0.3,
                    Pan = 0.0,
                    AudioFilePath = GetInstrumentalFilePath(gjProject, instrumentalTrackIndex),
                    Offset = 0
                };
                project.TrackList.Add(svipTrack);
            }
        }

        private void DecodeSingingTracks(TimeSynchronizer timeSynchronizer, GjProject gjProject, Project project)
        {
            for (int singingTrackIndex = 0; singingTrackIndex < gjProject.Tracks.Count; singingTrackIndex++)
            {
                List<Note> noteListFromGj = new List<Note>();
                for (int noteIndex = 0; noteIndex < gjProject.Tracks[singingTrackIndex].NoteList.Count; noteIndex++)
                {
                    Note noteFromGj = DecodeNote(gjProject, singingTrackIndex, noteIndex, timeSynchronizer, project);
                    noteListFromGj.Add(noteFromGj);
                }
                Params paramsFromGj = new Params();
                DecodeParams(gjProject, singingTrackIndex, paramsFromGj);

                Track svipTrack = new SingingTrack
                {
                    Title = GetSingerNameFromGj(gjProject.Tracks[singingTrackIndex].Name),
                    Mute = gjProject.Tracks[singingTrackIndex].MasterVolume.Mute,
                    Solo = false,
                    Volume = 0.7,
                    Pan = 0.0,
                    AISingerName = GetDefaultSingerName(),
                    ReverbPreset = GetDefaultReverbPreset(),
                    NoteList = noteListFromGj,
                    EditedParams = paramsFromGj
                };
                project.TrackList.Add(svipTrack);
            }
        }

        private void DecodeParams(GjProject gjProject, int singingTrackIndex, Params paramsFromGj)
        {
            DecodeVolumeParam(gjProject, singingTrackIndex, paramsFromGj);
            DecodePitchParam(gjProject, singingTrackIndex, paramsFromGj);
        }

        private void DecodePitchParam(GjProject gjProject, int singingTrackIndex, Params paramsFromGj)
        {
            ParamCurve paramCurvePitch = new ParamCurve();
            DecodeModifiedPitchParam(gjProject, singingTrackIndex, paramCurvePitch);
            DecodeOriginalPitchParam(gjProject, singingTrackIndex, paramsFromGj, paramCurvePitch);
            paramCurvePitch.PointList.OrderBy(x => x.Item1).ToList();
            paramsFromGj.Pitch = paramCurvePitch;
        }

        private void DecodeModifiedPitchParam(GjProject gjProject, int singingTrackIndex, ParamCurve paramCurvePitch)
        {
            Tuple<int, int> defaultLeftEndpoint = Tuple.Create(-192000, -100);
            paramCurvePitch.PointList.Add(defaultLeftEndpoint);

            try
            {
                var index = -1;
                foreach (var range in gjProject.Tracks[singingTrackIndex].PitchParam.ModifyRanges)
                {
                    Tuple<int, int> leftEndpoint = Tuple.Create(GetPitchParamTimeFromGj(range.Left), -100);//左间断点
                    Tuple<int, int> rightEndpoint = Tuple.Create(GetPitchParamTimeFromGj(range.Right), -100);//右间断点
                    paramCurvePitch.PointList.Add(leftEndpoint);//添加左间断点
                    index = gjProject.Tracks[singingTrackIndex].PitchParam.PitchPointList.FindIndex(index + 1, p => p.Time >= range.Left && p.Value <= range.Right);
                    if (index == -1)
                        continue;
                    for (; (index < gjProject.Tracks[singingTrackIndex].PitchParam.PitchPointList.Count) && (gjProject.Tracks[singingTrackIndex].PitchParam.PitchPointList[index].Time <= range.Right); ++index)
                    {
                        int pitchParamTime = GetPitchParamTimeFromGj(gjProject.Tracks[singingTrackIndex].PitchParam.PitchPointList[index].Time);
                        int pitchParamValue = GetPitchParamValueFromGj(gjProject.Tracks[singingTrackIndex].PitchParam.PitchPointList[index].Value);
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
        }

        private void DecodeOriginalPitchParam(GjProject gjProject, int singingTrackIndex, Params paramsFromGj, ParamCurve paramCurvePitch)
        {
            try
            {
                int value;
                int leftEndpoint;
                int rightEndpoint;
                int time;
                bool isInModifyRange;
                List<int> pitchPointBufferTime = new List<int>();
                List<int> pitchPointBufferPitch = new List<int>();

                for (int originPitchIndex = 0; originPitchIndex < gjProject.Tracks[singingTrackIndex].PitchParam.DefaultPitchPointList.Count - 1; originPitchIndex++)//遍历所有默认音高参数点
                {
                    time = GetPitchParamTimeFromGj(gjProject.Tracks[singingTrackIndex].PitchParam.DefaultPitchPointList[originPitchIndex].Time);
                    value = GetPitchParamValueFromGj(gjProject.Tracks[singingTrackIndex].PitchParam.DefaultPitchPointList[originPitchIndex].Value);
                    isInModifyRange = false;
                    for (int index = 0; index < gjProject.Tracks[singingTrackIndex].PitchParam.ModifyRanges.Count; index++)//判断当前默认音高参数点是否在ModifyRange内
                    {
                        leftEndpoint = GetPitchParamTimeFromGj(gjProject.Tracks[singingTrackIndex].PitchParam.ModifyRanges[index].Left);
                        rightEndpoint = GetPitchParamTimeFromGj(gjProject.Tracks[singingTrackIndex].PitchParam.ModifyRanges[index].Right);
                        if (time >= leftEndpoint && time <= rightEndpoint)
                        {
                            isInModifyRange = true;
                            break;
                        }
                    }
                    if (!isInModifyRange)//不在ModifyRange内才写入
                    {
                        pitchPointBufferTime.Add(time);
                        pitchPointBufferPitch.Add(value);
                    }
                }

                for (int i = 0; i < pitchPointBufferTime.Count; i++)
                {
                    Tuple<int, int> pitchParamPoint = Tuple.Create(pitchPointBufferTime[i], pitchPointBufferPitch[i]);
                    paramCurvePitch.PointList.Add(pitchParamPoint);
                }
            }
            catch (Exception)
            {

            }
        }

        private void DecodeVolumeParam(GjProject gjProject, int singingTrackIndex, Params paramsFromGj)
        {
            try
            {
                List<double> timeBuffer = new List<double>();
                List<double> valueBuffer = new List<double>();
                int time;
                int value;
                ParamCurve paramCurveVolume = new ParamCurve();
                for (int volumeParamPointIndex = 0; volumeParamPointIndex < gjProject.Tracks[singingTrackIndex].VolumeParam.Count; volumeParamPointIndex++)
                {
                    time = GetVolumeParamTimeFromGj(gjProject, singingTrackIndex, volumeParamPointIndex);
                    value = GetVolumeParamValueFromGj(gjProject.Tracks[singingTrackIndex].VolumeParam[volumeParamPointIndex].Value);
                    Tuple<int, int> volumeParamPoint = Tuple.Create(time, value);
                    paramCurveVolume.PointList.Add(volumeParamPoint);
                }

                paramCurveVolume.PointList.OrderBy(x => x.Item1).ToList();
                paramsFromGj.Volume = paramCurveVolume;
            }
            catch (Exception)
            {

            }
        }

        private Note DecodeNote(GjProject gjProject, int singingTrackIndex, int noteIndex, TimeSynchronizer timeSynchronizer, Project project)
        {
            int noteDurationFromGj = gjProject.Tracks[singingTrackIndex].NoteList[noteIndex].Duration;
            int convertedStartPosition = gjProject.Tracks[singingTrackIndex].NoteList[noteIndex].StartTick - 1920 * project.TimeSignatureList[0].Numerator / project.TimeSignatureList[0].Denominator;
            double preTimeFromGj = gjProject.Tracks[singingTrackIndex].NoteList[noteIndex].PreTime;
            double postTimeFromGj = gjProject.Tracks[singingTrackIndex].NoteList[noteIndex].PostTime;
            float headLengthInSecsFromGj;
            float midRatioOverTailFromGj;
            double essentialVowelLength;
            double tailVowelLength;
            int delta;
            Phones phones = new Phones();
            try
            {
                if (preTimeFromGj != 0.0)
                {
                    delta = convertedStartPosition + (int)(preTimeFromGj / 1000.0 * 480.0);
                    if (delta > 0)
                    {
                        headLengthInSecsFromGj = (float)(timeSynchronizer.GetActualSecsFromTicks(convertedStartPosition) - timeSynchronizer.GetActualSecsFromTicks(delta));
                        phones.HeadLengthInSecs = headLengthInSecsFromGj;
                    }
                }
                if (postTimeFromGj != 0.0)
                {
                    essentialVowelLength = noteDurationFromGj * 1000 / 480 + postTimeFromGj;
                    tailVowelLength = -postTimeFromGj;
                    midRatioOverTailFromGj = (float)(essentialVowelLength / tailVowelLength);
                    phones.MidRatioOverTail = midRatioOverTailFromGj;
                }
            }
            catch (Exception)
            {

            }

            //用于调试
            //MessageBox.Show("convertedStartPosition = " + convertedStartPosition +
            //    "\nconvertedPreTime" + (int)(preTimeFromGj / 1000.0 * 480.0) +
            //    "\nessentialVowelLength = " + essentialVowelLength +
            //    "\ntailVowelLength = " + tailVowelLength + 
            //    "\nMidRatioOverTail = " + midRatioOverTailFromGj);
            string pronunciation = gjProject.Tracks[singingTrackIndex].NoteList[noteIndex].Pinyin;


            Note note = new Note
            {
                StartPos = convertedStartPosition,
                Length = noteDurationFromGj,
                KeyNumber = GetKeyNumberFromGj(gjProject.Tracks[singingTrackIndex].NoteList[noteIndex].KeyNumber),
                Lyric = gjProject.Tracks[singingTrackIndex].NoteList[noteIndex].Lyric,
                EditedPhones = phones
            };
            if (pronunciation == "")
            {
                note.Pronunciation = null;
            }
            else
            {
                note.Pronunciation = pronunciation;
            }
            return note;
        }

        private void DecodeTimeSignature(GjProject gjProject, Project project)
        {
            if (gjProject.TempoMap.TimeSignature.Count == 0)//如果拍号只有4/4，gjgj不存
            {
                project.TimeSignatureList.Add(GetInitialTimeSignature());
            }
            else
            {
                if (gjProject.TempoMap.TimeSignature[0].Time != 0)//如果存的第一个拍号不在0处，说明0处的拍号是4/4
                {
                    project.TimeSignatureList.Add(GetInitialTimeSignature());

                    int sumOfTime = 0;
                    for (int index = 0; index < gjProject.TempoMap.TimeSignature.Count; index++)
                    {
                        TimeSignature timeSignature = new TimeSignature();
                        if (index == 0)
                        {
                            sumOfTime += gjProject.TempoMap.TimeSignature[0].Time / 1920;
                            timeSignature.BarIndex = sumOfTime;
                        }
                        else
                        {
                            sumOfTime += (gjProject.TempoMap.TimeSignature[index].Time - gjProject.TempoMap.TimeSignature[index - 1].Time) * GetDenominator(gjProject, index - 1) / 1920 / GetNumerator(gjProject, index - 1);
                            timeSignature.BarIndex = sumOfTime;
                        }
                        timeSignature.Numerator = GetNumerator(gjProject, index);
                        timeSignature.Denominator = GetDenominator(gjProject, index);
                        project.TimeSignatureList.Add(timeSignature);
                    }
                }
                else
                {
                    int sumOfTime = 0;
                    for (int index = 0; index < gjProject.TempoMap.TimeSignature.Count; index++)
                    {
                        TimeSignature timeSignature = new TimeSignature();
                        if (index == 0)
                        {
                            timeSignature.BarIndex = 0;
                        }
                        else
                        {
                            sumOfTime += (gjProject.TempoMap.TimeSignature[index].Time - gjProject.TempoMap.TimeSignature[index - 1].Time) * GetDenominator(gjProject, index - 1) / 1920 / GetNumerator(gjProject, index - 1);
                            timeSignature.BarIndex = sumOfTime;
                        }
                        timeSignature.Numerator = GetNumerator(gjProject, index);
                        timeSignature.Denominator = GetDenominator(gjProject, index);
                        project.TimeSignatureList.Add(timeSignature);
                    }
                }
            }

        }

        private void DecodeTempo(GjProject gjProject, Project project)
        {
            for (int i = 0; i < gjProject.TempoMap.Tempos.Count; i++)
            {
                SongTempo songTempo = new SongTempo
                {
                    Position = gjProject.TempoMap.Tempos[i].Time,
                    BPM = GetBPMFromGj(gjProject.TempoMap.Tempos[i].MicrosecondsPerQuarterNote)
                };
                project.SongTempoList.Add(songTempo);
            }
        }

        private string GetSingerNameFromGj(string origin)
        {
            switch (origin)
            {
                case "513singer":
                    return "扇宝";
                case "514singer":
                    return "SING-林嘉慧";
                default:
                    return "演唱轨";
            }
        }

        private double YToTone(double y)
        {
            return 71.5 - y / 18.0;
        }

        private int GetPitchParamTimeFromGj(double origin)
        {
            return (int)(origin * 5.0);
        }

        private int GetPitchParamValueFromGj(double origin)
        {
            return (int)(YToTone(origin) * 100.0 + 2400.0);
        }

        private int GetVolumeParamTimeFromGj(GjProject gjProject, int singingTrackIndex, int volumeParamPointIndex)
        {
            return (int)gjProject.Tracks[singingTrackIndex].VolumeParam[volumeParamPointIndex].Time;
        }

        private int GetVolumeParamValueFromGj(double origin)
        {
            return (int)origin * 1000 - 1000;
        }

        private float GetBPMFromGj(double origin)
        {
            return (float)(60.0 / origin * 1000000.0);
        }

        private int GetKeyNumberFromGj(int origin)
        {
            return origin + 24;
        }

        private static string GetDefaultReverbPreset()
        {
            return "干声";
        }

        private static string GetDefaultSingerName()
        {
            return "陈水若";
        }

        private static string GetInstrumentalFilePath(GjProject gjProject, int instrumentalTrackIndex)
        {
            return gjProject.Instrumental[instrumentalTrackIndex].Path;
        }

        private static string GetInstrumentalName(GjProject gjProject, int instrumentalTrackIndex)
        {
            return Path.GetFileNameWithoutExtension(gjProject.Instrumental[instrumentalTrackIndex].Path);//获取伴奏文件名作为轨道标题
        }

        private static bool GetInstrumentalMute(GjProject gjProject, int instrumentalTrackIndex)
        {
            return gjProject.Instrumental[instrumentalTrackIndex].MasterVolume.Mute;
        }

        private static string GetSvipProjectVersion()
        {
            return "SVIP7.0.0";
        }

        private TimeSignature GetInitialTimeSignature()
        {
            TimeSignature defaultTimeSignature = new TimeSignature
            {
                BarIndex = 0,
                Numerator = 4,
                Denominator = 4
            };
            return defaultTimeSignature;
        }

        private static int GetDenominator(GjProject gjProject, int index)
        {
            return gjProject.TempoMap.TimeSignature[index].Denominator;
        }

        private static int GetNumerator(GjProject gjProject, int index)
        {
            return gjProject.TempoMap.TimeSignature[index].Numerator;
        }
    }
}