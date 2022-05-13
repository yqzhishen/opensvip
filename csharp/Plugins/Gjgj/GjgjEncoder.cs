using System;
using System.Collections.Generic;
using OpenSvip.Model;
using Gjgj.Model;
using OpenSvip.Library;
using OpenSvip.Framework;
using NPinyin;

namespace Plugin.Gjgj
{
    public class GjgjEncoder
    {
        public LyricsAndPinyinSettings lyricsAndPinyinSettings { get; set; }

        /// <summary>
        /// 指定的参数平均采样间隔。
        /// </summary>
        public int ParamSampleInterval { get; set; }

        /// <summary>
        /// 指定的歌手名称。
        /// </summary>
        public string SingerName { get; set; }

        private Project osProject;

        private GjProject gjProject;

        private TimeSynchronizer timeSync;

        private bool isUnsupportedPinyinExist = false;

        private List<string> unsupportedPinyinList = new List<string>();

        /// <summary>
        /// 转换为歌叽歌叽工程。
        /// </summary>
        /// <param name="project">原始的OpenSvip工程。</param>
        /// <returns>转换后的歌叽歌叽工程。</returns>
        public GjProject EncodeProject(Project project)
        {
            osProject = project;
            timeSync = new TimeSynchronizer(osProject.SongTempoList);
            gjProject = new GjProject
            {
                gjgjVersion = 2,
                ProjectSetting = EncodeProjectSetting(),
                TempoMap = EncodeTempoMap()
            };
            EncodeTracks();
            return gjProject;
        }

        /// <summary>
        /// 转换演唱轨和伴奏轨。
        /// </summary>
        private void EncodeTracks()
        {
            int noteID = 1;
            int trackID = 1;
            gjProject.SingingTrackList = new List<GjSingingTrack>(osProject.TrackList.Count);
            gjProject.InstrumentalTrackList = new List<GjInstrumentalTrack>();
            int trackIndex = 0;
            foreach (var track in osProject.TrackList)
            {
                switch (track)
                {
                    case SingingTrack singingTrack:
                        gjProject.SingingTrackList.Add(EncodeSingingTrack(ref noteID, trackID, singingTrack));
                        trackID++;
                        break;
                    case InstrumentalTrack instrumentalTrack:
                        gjProject.InstrumentalTrackList.Add(EncodeInstrumentalTrack(trackID, instrumentalTrack));
                        trackID++;
                        break;
                    default:
                        break;
                }
                trackIndex++;
            }
            if (isUnsupportedPinyinExist)
            {
                string unsupportedPinyin = string.Join("、", unsupportedPinyinList);
                Warnings.AddWarning($"当前工程文件有歌叽歌叽不支持的拼音，已忽略。不支持的拼音：{unsupportedPinyin}", type: WarningTypes.Lyrics);
            }
            gjProject.MIDITrackList = EncodeMIDITrackList();
        }

        /// <summary>
        /// 转换伴奏轨。
        /// </summary>
        /// <param name="trackID">轨道ID。</param>
        /// <param name="instrumentalTrack">原始伴奏轨。</param>
        /// <returns>转换后的伴奏轨。</returns>
        private GjInstrumentalTrack EncodeInstrumentalTrack(int trackID, InstrumentalTrack instrumentalTrack)
        {
            GjTrackVolume gjTrackVolume = new GjTrackVolume
            {
                Volume = 1.0f,
                LeftVolume = 1.0f,
                RightVolume = 1.0f,
                Mute = instrumentalTrack.Mute
            };
            GjInstrumentalTrack gjInstrumentalTrack = new GjInstrumentalTrack
            {
                TrackID = Convert.ToString(trackID),
                Path = instrumentalTrack.AudioFilePath,
                Offset = 0,//暂不转换伴奏位置偏移
                EQProgram = "",
                SortIndex = 0,
                TrackVolume = gjTrackVolume
            };
            return gjInstrumentalTrack;
        }

        /// <summary>
        /// 转换演唱轨。
        /// </summary>
        /// <param name="noteID">音符ID。</param>
        /// <param name="trackID">轨道ID。</param>
        /// <param name="singingTrack">原始演唱轨。</param>
        /// <returns>转换后的演唱轨。</returns>
        private GjSingingTrack EncodeSingingTrack(ref int noteID, int trackID, SingingTrack singingTrack)
        {
            GjSingingTrack gjSingingTrack = new GjSingingTrack
            {
                TrackID = Convert.ToString(trackID),
                Type = 0,
                Name = GetSingerCode(),
                SortIndex = 0,
                NoteList = EncodeNoteList(noteID, singingTrack),
                VolumeParam = EncodeVolumeParam(singingTrack),
                PitchParam = EncodePitchParam(singingTrack),
                SingerInfo = new GjSingerInfo(),
                Keyboard = EncodeKeyboard(),
                TrackVolume = EncodeTrackVolume(singingTrack),
                EQProgram = "无"
            };
            return gjSingingTrack;
        }

        /// <summary>
        /// 根据歌手名称返回歌手代号。
        /// </summary>
        private string GetSingerCode()
        {
            switch (SingerName)
            {
                case "扇宝":
                    return "513singer";
                case "SING-林嘉慧":
                    return "514singer";
                case "Rocky":
                    return "881singer";
                default:
                    return "513singer";
            }
        }

        /// <summary>
        /// 生成MIDI轨道列表。
        /// </summary>
        /// <returns>空的MIDI轨道列表。</returns>
        private List<GjMIDITrack> EncodeMIDITrackList()
        {
            return new List<GjMIDITrack>();
        }

        /// <summary>
        /// 返回转换后的音符列表。
        /// </summary>
        /// <param name="noteID">音符ID。</param>
        /// <param name="singingTrack">原始演唱轨。</param>
        private List<GjNote> EncodeNoteList(int noteID, SingingTrack singingTrack)
        {
            List<GjNote> gjNoteList = new List<GjNote>();
            foreach (var note in singingTrack.NoteList)
            {
                gjNoteList.Add(EncodeNote(noteID, note));
                noteID++;
            }
            return gjNoteList;
        }

        /// <summary>
        /// 返回转换后的音符。
        /// </summary>
        /// <param name="noteID">音符ID。</param>
        /// <param name="note">原始音符。</param>
        private GjNote EncodeNote(int noteID, Note note)
        {
            GjNote gjNote = new GjNote
            {
                NoteID = noteID,
                Lyric = GetLyric(note),
                Pinyin = GetNotePinyin(note),
                StartTick = GetNoteStartTick(note.StartPos),
                Duration = note.Length,
                KeyNumber = note.KeyNumber,
                PhonePreTime = GetNotePhonePreTime(note),
                PhonePostTime = GetNotePhonePostTime(note),
                Style = GetNoteStyle(note.HeadTag),
                Velocity = 127
            };
            return gjNote;
        }

        private string GetLyric(Note note)
        {
            if (lyricsAndPinyinSettings == LyricsAndPinyinSettings.PinyinOnly)
            {
                return "啊";
            }
            else
            {
                return note.Lyric;
            }
        }

        /// <summary>
        /// 返回轨道的Keyboard属性。
        /// </summary>
        private GjKeyboard EncodeKeyboard()
        {
            GjKeyboard gjKeyboard = new GjKeyboard
            {
                KeyMode = 1,
                KeyType = 0
            };
            return gjKeyboard;
        }

        /// <summary>
        /// 返回演唱轨的音量属性。
        /// </summary>
        /// <param name="singingTrack">原始演唱轨。</param>
        /// <returns></returns>
        private GjTrackVolume EncodeTrackVolume(SingingTrack singingTrack)
        {
            GjTrackVolume gjTrackVolume = new GjTrackVolume
            {
                Volume = 1.0f,
                LeftVolume = 1.0f,
                RightVolume = 1.0f,
                Mute = singingTrack.Mute
            };
            return gjTrackVolume;
        }

        /// <summary>
        /// 返回演唱轨的音量参数曲线。
        /// </summary>
        /// <param name="singingTrack">原始演唱轨。</param>
        /// <returns></returns>
        private List<GjVolumeParamPoint> EncodeVolumeParam(SingingTrack singingTrack)
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
                ParamCurve paramCurve = ParamCurveUtils.ReduceSampleRate(singingTrack.EditedParams.Volume, ParamSampleInterval);
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
            catch (Exception)
            {

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
        /// 返回演唱轨的音高参数曲线。
        /// </summary>
        /// <param name="singingTrack">原始演唱轨。</param>
        /// <returns></returns>
        private GjPitchParam EncodePitchParam(SingingTrack singingTrack)
        {
            try
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
                        if (valueOrigin != -100)
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
            catch (Exception)
            {
                return new GjPitchParam();
            }
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
        /// 返回谱面的曲速和拍号信息。
        /// </summary>
        /// <returns></returns>
        private GjTempoMap EncodeTempoMap()
        {
            GjTempoMap gjTempoMap = new GjTempoMap
            {
                TicksPerQuarterNote = 480,
                TempoList = EncodeTempoList(),
                TimeSignatureList = EncodeTimeSignatureList()
            };
            return gjTempoMap;
        }

        /// <summary>
        /// 返回转换后的曲速列表。
        /// </summary>
        /// <returns></returns>
        private List<GjTempo> EncodeTempoList()
        {
            List<GjTempo> gjTempoList = new List<GjTempo>();
            for (int index = 0; index < osProject.SongTempoList.Count; index++)
            {
                gjTempoList.Add(EncodeTempo(index));
            }
            return gjTempoList;
        }

        /// <summary>
        /// 返回转换后的曲速标记。
        /// </summary>
        /// <param name="index">曲速标记的索引。</param>
        /// <returns></returns>
        private GjTempo EncodeTempo(int index)
        {
            GjTempo gjTempo = new GjTempo
            {
                Time = osProject.SongTempoList[index].Position,
                MicrosecondsPerQuarterNote = GetMicrosecondsPerQuarterNote(index)
            };
            return gjTempo;
        }

        /// <summary>
        /// 返回转换后的拍号列表。
        /// </summary>
        /// <returns></returns>
        private List<GjTimeSignature> EncodeTimeSignatureList()
        {
            List<GjTimeSignature> gjTimeSignatureList = new List<GjTimeSignature>(osProject.TimeSignatureList.Count);
            int sumOfTime = 0;
            for (int index = 0; index < osProject.TimeSignatureList.Count; index++)
            {

                gjTimeSignatureList.Add(EncodeTimeSignature(sumOfTime, index));
            }
            return gjTimeSignatureList;
        }

        /// <summary>
        /// 返回转换后的拍号。
        /// </summary>
        /// <param name="sumOfTime">从谱面开始到当前拍号的累计时间。</param>
        /// <param name="index">原始拍号的索引。</param>
        /// <returns></returns>
        private GjTimeSignature EncodeTimeSignature(int sumOfTime, int index)
        {
            GjTimeSignature gjTimeSignature = new GjTimeSignature
            {
                Time = GetTimeSignatureTime(sumOfTime, index),
                Numerator = GetNumerator(index),
                Denominator = GetDenominator(index)
            };
            return gjTimeSignature;
        }

        /// <summary>
        /// 返回转换后的拍号的时间。
        /// </summary>
        /// <param name="sumOfTime">从谱面开始到当前拍号的累计时间。</param>
        /// <param name="index">原始拍号的索引。</param>
        /// <returns></returns>
        private int GetTimeSignatureTime(int sumOfTime, int index)
        {
            int time;
            if (index == 0)
            {
                time = GetBarIndex(0) * 1920 * GetNumerator(index) / GetDenominator(index);
            }
            else
            {
                sumOfTime += (GetBarIndex(index) - GetBarIndex(index - 1)) * 1920 * GetNumerator(index - 1) / GetDenominator(index - 1);
                time = sumOfTime;
            }
            return time;
        }

        /// <summary>
        /// 返回拍号所在的小节。
        /// </summary>
        /// <param name="index">原始拍号的索引。</param>
        /// <returns></returns>
        private int GetBarIndex(int index)
        {
            return osProject.TimeSignatureList[index].BarIndex;
        }

        /// <summary>
        /// 返回拍号的分子。
        /// </summary>
        /// <param name="index">原始拍号的索引。</param>
        /// <returns></returns>
        private int GetNumerator(int index)
        {
            return osProject.TimeSignatureList[index].Numerator;
        }

        /// <summary>
        /// 返回拍号的分母。
        /// </summary>
        /// <param name="index">原始拍号的索引。</param>
        /// <returns></returns>
        private int GetDenominator(int index)
        {
            return osProject.TimeSignatureList[index].Denominator;
        }

        /// <summary>
        /// 返回每四分音符的微秒数。
        /// </summary>
        /// <param name="index">原始曲速的索引。</param>
        /// <returns></returns>
        private int GetMicrosecondsPerQuarterNote(int index)
        {
            return (int)(60.0 / osProject.SongTempoList[index].BPM * 1000000.0);
        }

        /// <summary>
        /// 设置工程的基本属性。
        /// </summary>
        /// <returns>工程的基本属性。</returns>
        private GjProjectSetting EncodeProjectSetting()
        {
            GjProjectSetting gjProjectSetting = new GjProjectSetting
            {
                No1KeyName = "C",
                EQAfterMix = "",
                ProjectType = 0,
                Denominator = 4,
                SynMode = 0
            };
            return gjProjectSetting;
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
        /// 返回音符标记（无、换气和停顿）。
        /// </summary>
        /// <param name="origin">原始音符标记。</param>
        /// <returns></returns>
        private int GetNoteStyle(string origin)
        {
            switch (origin)
            {
                case null:
                    return 0;
                case "V":
                    return 1;
                case "0":
                    return 2;
                default:
                    return 0;
            }
        }

        /// <summary>
        /// 返回转换后的音符起始时间。
        /// </summary>
        /// <param name="origin">原始时间。</param>
        /// <returns></returns>
        private int GetNoteStartTick(int origin)
        {
            return origin + 1920 * GetNumerator(0) / GetDenominator(0);
        }

        /// <summary>
        /// 转换音符的拼音。
        /// </summary>
        /// <param name="origin">原始发音。</param>
        /// <returns>如果原始发音为空值或者歌叽歌叽不支持，返回空字符串，否则返回原始的发音。</returns>
        private string GetNotePinyin(Note note)
        {
            if ((lyricsAndPinyinSettings == LyricsAndPinyinSettings.lyricsOnly))//仅歌词
            {
                return null;
            }
            else
            {
                if (note.Pronunciation == null)//没有拼音的音符
                {
                    if (lyricsAndPinyinSettings == LyricsAndPinyinSettings.SameAsSource)//和源相同时
                    {
                        return "";
                    }
                    else//仅拼音、歌词和拼音
                    {
                        return Pinyin.GetPinyin(note.Lyric);
                    }
                }
                else//有拼音的音符
                {
                    string pinyin = note.Pronunciation;
                    if (pinyin != "" && !GjgjSupportedPinyin.SupportedPinyinList().Contains(pinyin.ToLower()))
                    {
                        isUnsupportedPinyinExist = true;
                        if (!unsupportedPinyinList.Contains(pinyin))
                        {
                            unsupportedPinyinList.Add(pinyin);
                        }
                        pinyin = "";//过滤不支持的拼音
                    }
                    return pinyin;
                }
            }

        }

        /// <summary>
        /// 转换音素的第一根杆子。
        /// </summary>
        /// <param name="note">原始音符。</param>
        /// <returns>转换后的音素的第一根杆子。</returns>
        private double GetNotePhonePreTime(Note note)
        {
            double phonePreTime = 0.0;
            try
            {
                if (note.EditedPhones != null)
                {
                    if (note.EditedPhones.HeadLengthInSecs != -1.0)
                    {
                        int noteStartPositionInTicks = note.StartPos + (1920 * GetNumerator(0) / GetDenominator(0));
                        double noteStartPositionInSeconds = timeSync.GetActualSecsFromTicks(noteStartPositionInTicks);
                        double phoneHeadPositionInSeconds = noteStartPositionInSeconds - note.EditedPhones.HeadLengthInSecs;
                        double phoneHeadPositionInTicks = timeSync.GetActualTicksFromSecs(phoneHeadPositionInSeconds);
                        double difference = noteStartPositionInTicks - phoneHeadPositionInTicks;
                        phonePreTime = -difference * (2000.0 / 3.0) / 480.0;
                    }
                }
            }
            catch (Exception)
            {

            }
            return phonePreTime;
        }

        /// <summary>
        /// 转换音素的第二根杆子。
        /// </summary>
        /// <param name="note">原始音符。</param>
        /// <returns>转换后的音素的第二根杆子。</returns>
        private double GetNotePhonePostTime(Note note)
        {
            double phonePostTime = 0.0;
            try
            {
                if (note.EditedPhones != null)
                {
                    if (note.EditedPhones.MidRatioOverTail != -1.0)
                    {
                        double noteLength = note.Length;
                        double ratio = note.EditedPhones.MidRatioOverTail;
                        phonePostTime = -(noteLength / (1.0 + ratio)) * (2000.0 / 3.0) / 480.0;
                    }
                }
            }
            catch (Exception)
            {

            }
            return phonePostTime;
        }
    }
}