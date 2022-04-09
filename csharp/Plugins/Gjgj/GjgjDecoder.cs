using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using OpenSvip.Model;
using OpenSvip.Library;
using Gjgj.Model;

namespace Plugin.Gjgj
{
    public class GjgjDecoder
    {
        private GjProject gjProjectT;

        private TimeSynchronizer timeSynchronizer;

        public Project DecodeProject(GjProject originalProject)
        {
            gjProjectT = originalProject;
            var xsProject = new Project
            {
                Version = GetSvipProjectVersion(),
                SongTempoList = DecodeSongTempoList(),
                TimeSignatureList = DecodeTimeSignatureList()
            };
            timeSynchronizer = new TimeSynchronizer(xsProject.SongTempoList);
            xsProject.TrackList = DecodeTrackList(xsProject);
            return xsProject;

        }

        private List<Track> DecodeTrackList(Project project)
        {
            List<Track> trackList = new List<Track>();
            trackList.AddRange(DecodeSingingTracks(project));
            trackList.AddRange(DecodeInstrumentalTracks());
            return trackList;
        }

        private List<Track> DecodeSingingTracks(Project project)
        {
            List<Track> singingTrackList = new List<Track>();
            for (int index = 0; index < gjProjectT.SingingTrackList.Count; index++)
            {
                Track track = new SingingTrack
                {
                    Title = GetSingingTrackTitle(index),
                    Mute = gjProjectT.SingingTrackList[index].TrackVolume.Mute,
                    Solo = false,
                    Volume = 0.7,
                    Pan = 0.0,
                    AISingerName = GetDefaultAISingerName(),
                    ReverbPreset = GetDefaultReverbPreset(),
                    NoteList = DecodeNoteList(index, project),
                    EditedParams = DecodeParams(index)
                };
                singingTrackList.Add(track);
            }
            return singingTrackList;
        }
        
        private List<Track> DecodeInstrumentalTracks()
        {
            List<Track> instrumentalTrackList = new List<Track>();
            for (int index = 0; index < gjProjectT.InstrumentalTrackList.Count; index++)
            {
                Track track = new InstrumentalTrack
                {
                    Title = GetInstrumentalName(index),
                    Mute = GetInstrumentalMute(index),
                    Solo = false,
                    Volume = 0.3,
                    Pan = 0.0,
                    AudioFilePath = GetInstrumentalFilePath(index),
                    Offset = 0
                };
                instrumentalTrackList.Add(track);
            }
            return instrumentalTrackList;
        }

        private string GetSingingTrackTitle(int index)
        {
            switch (gjProjectT.SingingTrackList[index].Name)
            {
                case "513singer":
                    return "扇宝";
                case "514singer":
                    return "SING-林嘉慧";
                case "881singer":
                    return "Rocky";
                default:
                    return "演唱轨";
            }
        }

        private List<Note> DecodeNoteList(int singingTrackIndex, Project project)
        {
            List<Note> noteList = new List<Note>();
            for (int noteIndex = 0; noteIndex < gjProjectT.SingingTrackList[singingTrackIndex].NoteList.Count; noteIndex++)
            {
                noteList.Add(DecodeNote(singingTrackIndex, noteIndex, project));
            }
            return noteList;
        }

        private Note DecodeNote(int singingTrackIndex, int noteIndex, Project project)
        {
            Note note = new Note
            {
                StartPos = DecodeNoteStartPosition(singingTrackIndex, noteIndex, project),
                Length = gjProjectT.SingingTrackList[singingTrackIndex].NoteList[noteIndex].Duration,
                KeyNumber = gjProjectT.SingingTrackList[singingTrackIndex].NoteList[noteIndex].KeyNumber,
                Lyric = gjProjectT.SingingTrackList[singingTrackIndex].NoteList[noteIndex].Lyric,
                Pronunciation = DecodePronunciation(singingTrackIndex, noteIndex),
                EditedPhones = DecodePhones(singingTrackIndex, noteIndex, project),
                HeadTag = DecodeNoteHeadTag(singingTrackIndex, noteIndex)
            };
            return note;
        }

        private int DecodeNoteStartPosition(int singingTrackIndex, int noteIndex, Project project)//用了xs的拍号列表，是因为gj可能存在拍号列表里面没有拍号的情况
        {
            return gjProjectT.SingingTrackList[singingTrackIndex].NoteList[noteIndex].StartTick - 1920 * project.TimeSignatureList[0].Numerator / project.TimeSignatureList[0].Denominator;
        }

        private string DecodePronunciation(int singingTrackIndex, int noteIndex)
        {
            string pinyin = gjProjectT.SingingTrackList[singingTrackIndex].NoteList[noteIndex].Pinyin;
            string pronunciation;
            if (pinyin == "")
            {
                pronunciation = null;
            }
            else
            {
                pronunciation = pinyin;
            }
            return pronunciation;
        }

        private Phones DecodePhones(int singingTrackIndex, int noteIndex, Project project)
        {
            int duration = gjProjectT.SingingTrackList[singingTrackIndex].NoteList[noteIndex].Duration;
            int startPosition = DecodeNoteStartPosition(singingTrackIndex, noteIndex, project);
            double preTime = gjProjectT.SingingTrackList[singingTrackIndex].NoteList[noteIndex].PhonePreTime;
            double postTime = gjProjectT.SingingTrackList[singingTrackIndex].NoteList[noteIndex].PhonePostTime;
            Phones phones = new Phones();
            try
            {
                if (preTime != 0.0)
                {
                    phones.HeadLengthInSecs = DecodeHeadLengthInSecs(startPosition, preTime);
                }
                if (postTime != 0.0)
                {
                    phones.MidRatioOverTail = DecodeMidRatioOverTail(duration, postTime);
                }
            }
            catch (Exception)
            {

            }
            return phones;
        }
        
        private float DecodeHeadLengthInSecs(int startPosition, double preTime)
        {
            int difference;
            float headLengthInSecs;
            difference = startPosition + (int)(preTime / 1000.0 * 480.0);
            if (difference > 0)
            {
                headLengthInSecs = (float)(timeSynchronizer.GetActualSecsFromTicks(startPosition) - timeSynchronizer.GetActualSecsFromTicks(difference));
            }
            else
            {
                headLengthInSecs = -1.0f;
            }
            return headLengthInSecs;
        }
        
        private float DecodeMidRatioOverTail(int duration, double postTime)
        {
            float midRatioOverTail;
            double essentialVowelLength;
            double tailVowelLength;
            if (postTime != 0)
            {
                essentialVowelLength = duration * 1000 / 480 + postTime;
                tailVowelLength = -postTime;
                midRatioOverTail = (float)(essentialVowelLength / tailVowelLength);
            }
            else
            {
                midRatioOverTail = -1.0f;
            }
            return midRatioOverTail;
        }

        private string DecodeNoteHeadTag(int singingTrackIndex, int noteIndex)
        {
            switch (gjProjectT.SingingTrackList[singingTrackIndex].NoteList[noteIndex].Style)
            {
                case 0:
                    return null;
                case 1:
                    return "V";
                case 2:
                    return "0";
                default:
                    return null;
            }
        }

        private Params DecodeParams(int singingTrackIndex)
        {
            Params paramsFromGj = new Params
            {
                Volume = DecodeVolumeParam(singingTrackIndex),
                Pitch = DecodePitchParam(singingTrackIndex)
            };
            return paramsFromGj;
        }

        private ParamCurve DecodeVolumeParam(int singingTrackIndex)
        {
            ParamCurve paramCurve = new ParamCurve();
            try
            {
                List<double> timeBuffer = new List<double>();
                List<double> valueBuffer = new List<double>();
                int time;
                int value;
                for (int volumeParamPointIndex = 0; volumeParamPointIndex < gjProjectT.SingingTrackList[singingTrackIndex].VolumeParam.Count; volumeParamPointIndex++)
                {
                    time = GetVolumeParamTimeFromGj(singingTrackIndex, volumeParamPointIndex);
                    value = GetVolumeParamValueFromGj(gjProjectT.SingingTrackList[singingTrackIndex].VolumeParam[volumeParamPointIndex].Value);
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

        private ParamCurve DecodePitchParam(int singingTrackIndex)
        {
            ParamCurve paramCurvePitch = new ParamCurve();
            DecodeModifiedPitchParam(singingTrackIndex, paramCurvePitch);
            DecodeOriginalPitchParam(singingTrackIndex, paramCurvePitch);
            paramCurvePitch.PointList.OrderBy(x => x.Item1).ToList();
            return paramCurvePitch;
        }

        private void DecodeModifiedPitchParam(int singingTrackIndex, ParamCurve paramCurvePitch)
        {
            Tuple<int, int> defaultLeftEndpoint = Tuple.Create(-192000, -100);
            paramCurvePitch.PointList.Add(defaultLeftEndpoint);

            try
            {
                var index = -1;
                foreach (var range in gjProjectT.SingingTrackList[singingTrackIndex].PitchParam.ModifyRangeList)
                {
                    Tuple<int, int> leftEndpoint = Tuple.Create(GetPitchParamTimeFromGj(range.Left), -100);//左间断点
                    Tuple<int, int> rightEndpoint = Tuple.Create(GetPitchParamTimeFromGj(range.Right), -100);//右间断点
                    paramCurvePitch.PointList.Add(leftEndpoint);//添加左间断点
                    index = gjProjectT.SingingTrackList[singingTrackIndex].PitchParam.PitchParamPointList.FindIndex(index + 1, p => p.Time >= range.Left && p.Value <= range.Right);
                    if (index == -1)
                        continue;
                    for (; (index < gjProjectT.SingingTrackList[singingTrackIndex].PitchParam.PitchParamPointList.Count) && (gjProjectT.SingingTrackList[singingTrackIndex].PitchParam.PitchParamPointList[index].Time <= range.Right); ++index)
                    {
                        int pitchParamTime = GetPitchParamTimeFromGj(gjProjectT.SingingTrackList[singingTrackIndex].PitchParam.PitchParamPointList[index].Time);
                        int pitchParamValue = GetPitchParamValueFromGj(gjProjectT.SingingTrackList[singingTrackIndex].PitchParam.PitchParamPointList[index].Value);
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

        private void DecodeOriginalPitchParam(int singingTrackIndex, ParamCurve paramCurvePitch)
        {
            try
            {
                int value;
                int left;
                int right;
                int time;
                bool isInModifyRange;
                List<int> timeBuffer = new List<int>();
                List<int> valueBuffer = new List<int>();

                for (int originPitchIndex = 0; originPitchIndex < gjProjectT.SingingTrackList[singingTrackIndex].PitchParam.DefaultPitchParamPointList.Count - 1; originPitchIndex++)//遍历所有默认音高参数点
                {
                    time = GetPitchParamTimeFromGj(gjProjectT.SingingTrackList[singingTrackIndex].PitchParam.DefaultPitchParamPointList[originPitchIndex].Time);
                    value = GetPitchParamValueFromGj(gjProjectT.SingingTrackList[singingTrackIndex].PitchParam.DefaultPitchParamPointList[originPitchIndex].Value);
                    isInModifyRange = false;
                    for (int index = 0; index < gjProjectT.SingingTrackList[singingTrackIndex].PitchParam.ModifyRangeList.Count; index++)//判断当前默认音高参数点是否在ModifyRange内
                    {
                        left = GetPitchParamTimeFromGj(gjProjectT.SingingTrackList[singingTrackIndex].PitchParam.ModifyRangeList[index].Left);
                        right = GetPitchParamTimeFromGj(gjProjectT.SingingTrackList[singingTrackIndex].PitchParam.ModifyRangeList[index].Right);
                        if (time >= left && time <= right)
                        {
                            isInModifyRange = true;
                            break;
                        }
                    }
                    if (!isInModifyRange)//不在ModifyRange内才写入
                    {
                        timeBuffer.Add(time);
                        valueBuffer.Add(value);
                    }
                }

                for (int i = 0; i < timeBuffer.Count; i++)
                {
                    Tuple<int, int> pitchParamPoint = Tuple.Create(timeBuffer[i], valueBuffer[i]);
                    paramCurvePitch.PointList.Add(pitchParamPoint);
                }
            }
            catch (Exception)
            {

            }
        }

        private List<TimeSignature> DecodeTimeSignatureList()
        {
            List<TimeSignature> timeSignatureList = new List<TimeSignature>();
            if (gjProjectT.TempoMap.TimeSignatureList.Count == 0)//如果拍号只有4/4，gjgj不存
            {
                timeSignatureList.Add(GetInitialTimeSignature());
            }
            else
            {
                if (gjProjectT.TempoMap.TimeSignatureList[0].Time != 0)//如果存的第一个拍号不在0处，说明0处的拍号是4/4
                {
                    timeSignatureList.Add(GetInitialTimeSignature());

                    int sumOfTime = 0;
                    for (int index = 0; index < gjProjectT.TempoMap.TimeSignatureList.Count; index++)
                    {
                        TimeSignature timeSignature = new TimeSignature();
                        if (index == 0)
                        {
                            sumOfTime += gjProjectT.TempoMap.TimeSignatureList[0].Time / 1920;
                            timeSignature.BarIndex = sumOfTime;
                        }
                        else
                        {
                            sumOfTime += (GetGjTimeSignatureTime(index) - GetGjTimeSignatureTime(index - 1)) * GetDenominator(index - 1) / 1920 / GetNumerator(index - 1);
                            timeSignature.BarIndex = sumOfTime;
                        }
                        timeSignature.Numerator = GetNumerator(index);
                        timeSignature.Denominator = GetDenominator(index);
                        timeSignatureList.Add(timeSignature);
                    }
                }
                else
                {
                    int sumOfTime = 0;
                    for (int index = 0; index < gjProjectT.TempoMap.TimeSignatureList.Count; index++)
                    {
                        TimeSignature timeSignature = new TimeSignature();
                        if (index == 0)
                        {
                            timeSignature.BarIndex = 0;
                        }
                        else
                        {
                            sumOfTime += (GetGjTimeSignatureTime(index) - GetGjTimeSignatureTime(index - 1)) * GetDenominator(index - 1) / 1920 / GetNumerator(index - 1);
                            timeSignature.BarIndex = sumOfTime;
                        }
                        timeSignature.Numerator = GetNumerator(index);
                        timeSignature.Denominator = GetDenominator(index);
                        timeSignatureList.Add(timeSignature);
                    }
                }
            }
            return timeSignatureList;
        }

        private List<SongTempo> DecodeSongTempoList()
        {
            List<SongTempo> songTempoList = new List<SongTempo>();
            for (int i = 0; i < gjProjectT.TempoMap.TempoList.Count; i++)
            {
                songTempoList.Add(DecodeSongTempo(i));
            }
            return songTempoList;
        }

        private SongTempo DecodeSongTempo(int index)
        {
            SongTempo songTempo = new SongTempo
            {
                Position = GetSongTempoPosition(index),
                BPM = GetSongTempoBPM(index)
            };
            return songTempo;
        }

        private int GetSongTempoPosition(int index)
        {
            return gjProjectT.TempoMap.TempoList[index].Time;
        }

        private float GetSongTempoBPM(int index)
        {
            double origin = gjProjectT.TempoMap.TempoList[index].MicrosecondsPerQuarterNote;
            return (float)(60.0 / origin * 1000000.0);
        }

        private double YToTone(double y)
        {
            return 127 + 0.5 - y / 18.0;
        }

        private int GetPitchParamTimeFromGj(double origin)
        {
            return (int)(origin * 5.0);
        }

        private int GetPitchParamValueFromGj(double origin)
        {
            return (int)(YToTone(origin) * 100.0);
        }

        private int GetVolumeParamTimeFromGj(int singingTrackIndex, int volumeParamPointIndex)
        {
            return (int)gjProjectT.SingingTrackList[singingTrackIndex].VolumeParam[volumeParamPointIndex].Time;
        }

        private int GetVolumeParamValueFromGj(double origin)
        {
            return (int)origin * 1000 - 1000;
        }

        private static string GetDefaultReverbPreset()
        {
            return "干声";
        }

        private static string GetDefaultAISingerName()
        {
            return "陈水若";
        }

        private string GetInstrumentalFilePath(int instrumentalTrackIndex)
        {
            return gjProjectT.InstrumentalTrackList[instrumentalTrackIndex].Path;
        }

        private string GetInstrumentalName(int instrumentalTrackIndex)
        {
            return Path.GetFileNameWithoutExtension(gjProjectT.InstrumentalTrackList[instrumentalTrackIndex].Path);//获取伴奏文件名作为轨道标题
        }

        private bool GetInstrumentalMute(int instrumentalTrackIndex)
        {
            return gjProjectT.InstrumentalTrackList[instrumentalTrackIndex].TrackVolume.Mute;
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

        private int GetGjTimeSignatureTime(int index)
        {
            return gjProjectT.TempoMap.TimeSignatureList[index].Time;
        }

        private int GetDenominator(int index)
        {
            return gjProjectT.TempoMap.TimeSignatureList[index].Denominator;
        }

        private int GetNumerator(int index)
        {
            return gjProjectT.TempoMap.TimeSignatureList[index].Numerator;
        }

    }
}