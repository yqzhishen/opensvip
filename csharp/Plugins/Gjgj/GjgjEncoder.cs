using System;
using System.Collections.Generic;
using OpenSvip.Model;
using FlutyDeer.GjgjPlugin.Model;
using OpenSvip.Library;
using OpenSvip.Framework;
using FlutyDeer.GjgjPlugin.Utils;
using FlutyDeer.GjgjPlugin.Optiions;

namespace FlutyDeer.GjgjPlugin
{
    public class GjgjEncoder
    {
        public LyricsAndPinyinOption lyricsAndPinyinOption { get; set; }

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

        private List<string> unsupportedPinyinList = new List<string>();

        private PitchParamUtil pitchParamUtil = new PitchParamUtil();

        private VolumeParamUtil volumeParamUtil = new VolumeParamUtil();

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
            if (unsupportedPinyinList.Count > 0)
            {
                string unsupportedPinyin = string.Join("、", unsupportedPinyinList);
                Warnings.AddWarning($"当前工程文件有歌叽歌叽不支持的拼音，已忽略。不支持的拼音：{unsupportedPinyin}", type: WarningTypes.Lyrics);
            }
            gjProject.MIDITrackList = new List<GjMIDITrack>();
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
                Offset = EncodeInstOffset(instrumentalTrack.Offset),
                EQProgram = "",
                SortIndex = 0,
                TrackVolume = gjTrackVolume
            };
            return gjInstrumentalTrack;
        }

        private int EncodeInstOffset(int origin)
        {
            int position = origin + 1920 * GetNumerator(0) / GetDenominator(0);
            if (position > 0)
            {
                return (int)(timeSync.GetActualSecsFromTicks(position) * 10000000);
            }
            else
            {
                return (int)(position / 480 * 60 / osProject.SongTempoList[0].BPM * 10000000);
            }
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
                Name = SingerNameUtil.ToSingerCode(SingerName),
                SortIndex = 0,
                NoteList = EncodeNoteList(noteID, singingTrack),
                VolumeParam = volumeParamUtil.EncodeVolumeParam(singingTrack, ParamSampleInterval),
                PitchParam = pitchParamUtil.EncodePitchParam(singingTrack),
                SingerInfo = new GjSingerInfo(),
                Keyboard = EncodeKeyboard(),
                TrackVolume = EncodeTrackVolume(singingTrack),
                EQProgram = "无"
            };
            return gjSingingTrack;
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
                Pinyin = PronunciationUtil.GetNotePinyin(note, lyricsAndPinyinOption, ref unsupportedPinyinList),
                StartTick = GetNoteStartTick(note.StartPos),
                Duration = note.Length,
                KeyNumber = note.KeyNumber,
                PhonePreTime = GetNotePhonePreTime(note),
                PhonePostTime = GetNotePhonePostTime(note),
                Style = NoteHeadTagUtil.ToIntTag(note.HeadTag),
                Velocity = 127
            };
            return gjNote;
        }

        private string GetLyric(Note note)
        {
            if (lyricsAndPinyinOption == LyricsAndPinyinOption.PinyinOnly)
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
            foreach (var tempo in osProject.SongTempoList)
            {
                gjTempoList.Add(TempoUtil.EncodeTempo(tempo));
            }
            return gjTempoList;
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
                gjTimeSignatureList.Add(EncodeTimeSignature(ref sumOfTime, index));
            }
            return gjTimeSignatureList;
        }

        /// <summary>
        /// 返回转换后的拍号。
        /// </summary>
        /// <param name="sumOfTime">从谱面开始到当前拍号的累计时间。</param>
        /// <param name="index">原始拍号的索引。</param>
        /// <returns></returns>
        private GjTimeSignature EncodeTimeSignature(ref int sumOfTime, int index)
        {
            GjTimeSignature gjTimeSignature = new GjTimeSignature
            {
                Time = GetTimeSignatureTime(ref sumOfTime, index),
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
        private int GetTimeSignatureTime(ref int sumOfTime, int index)
        {
            int time;
            if (index == 0)
            {
                time = 0;
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
        /// 返回转换后的音符起始时间。
        /// </summary>
        /// <param name="origin">原始时间。</param>
        /// <returns></returns>
        private int GetNoteStartTick(int origin)
        {
            return origin + 1920 * GetNumerator(0) / GetDenominator(0);
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