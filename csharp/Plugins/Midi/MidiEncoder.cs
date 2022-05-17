using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Interaction;
using Melanchall.DryWetMidi.MusicTheory;
using OpenSvip.Framework;
using OpenSvip.Model;
using Note = OpenSvip.Model.Note;
using NPinyin;

namespace Plugin.Midi
{
    public class MidiEncoder
    {
        /// <summary>
        /// 移调。
        /// </summary>
        public int Transpose { get; set; }

        /// <summary>
        /// 是否使用歌词兼容性模式，默认为否。
        /// </summary>
        public bool IsUseCompatibleLyric { get; set; }

        /// <summary>
        /// 是否移除歌词中的常见标点符号，默认为是。
        /// </summary>
        public bool IsRemoveSymbols { get; set; }

        /// <summary>
        /// 歌词文本编码。
        /// </summary>
        public LyricEncodings LyricEncoding { get; set; }

        /// <summary>
        /// 半元音前移量，单位为梯。
        /// </summary>
        public int SemivowelPreShift { get; set; }

        /// <summary>
        /// 时基，默认为480。
        /// </summary>
        public int PPQ { get; set; }

        private Project osProject;

        private MidiFile midiFile = new MidiFile();

        /// <summary>
        /// 转换为 MIDI 文件。
        /// </summary>
        /// <param name="project">原始的 OpenSvip 工程。</param>
        /// <param name="path">输出路径。</param>
        public void EncodeMidiFile(Project project, string path)
        {
            osProject = project;
            TicksPerQuarterNoteTimeDivision timeDivision = new TicksPerQuarterNoteTimeDivision((short)PPQ);
            midiFile.TimeDivision = timeDivision;//设置时基。
            EncodeMidiChunks();//相当于 MIDI 里面的轨道。
            WritingSettings settings = new WritingSettings
            {
                TextEncoding = GetEncoding()
            };
            midiFile.Write(path, true, settings: settings);
        }

        /// <summary>
        /// 生成 MIDI Chunks。
        /// </summary>
        private void EncodeMidiChunks()
        {
            foreach (var trackChunk in EncodeTrackChunkList())
            {
                midiFile.Chunks.Add(trackChunk);
            }
        }

        /// <summary>
        /// 将选项转换为编码。
        /// </summary>
        /// <returns>文本编码。</returns>
        private Encoding GetEncoding()
        {
            switch (LyricEncoding)
            {
                case LyricEncodings.ASCII:
                    return Encoding.ASCII;
                case LyricEncodings.BigEndianUnicode:
                    return Encoding.BigEndianUnicode;
                case LyricEncodings.Default:
                    return Encoding.Default;
                case LyricEncodings.Unicode:
                    return Encoding.Unicode;
                case LyricEncodings.UTF32:
                    return Encoding.UTF32;
                case LyricEncodings.UTF7:
                    return Encoding.UTF7;
                case LyricEncodings.UTF8BOM:
                    return Encoding.UTF8;
                case LyricEncodings.UTF8:
                    return new UTF8Encoding(false);//不带BOM的UTF-8
                default:
                    return Encoding.UTF8;
            }
        }

        /// <summary>
        /// 生成 MIDI Track Chuck 列表
        /// </summary>
        /// <returns>含有曲速轨道和演唱轨的 Track Chuck 列表。</returns>
        private List<TrackChunk> EncodeTrackChunkList()
        {
            List<TrackChunk> trackChunkList = new List<TrackChunk>();
            trackChunkList.Add(EncodeTempoTrackChunk());//MIDI 里的曲速轨。
            foreach (var track in osProject.TrackList)
            {
                switch (track)
                {
                    case SingingTrack singingTrack:
                        trackChunkList.Add(EncodeNoteTrackChunk(singingTrack));
                        break;
                    default:
                        break;
                }
            }
            return trackChunkList;
        }

        /// <summary>
        /// 生成曲速轨。
        /// </summary>
        /// <returns>含有曲速事件数组的 Track Chunk。</returns>
        private TrackChunk EncodeTempoTrackChunk()
        {
            return new TrackChunk(EncodeSetTemopEventArray());
        }

        /// <summary>
        /// 将曲速标记列表转换为 MIDI 事件数组。
        /// </summary>
        /// <returns>含有曲速事件的 MIDI Event 数组。</returns>
        private MidiEvent[] EncodeSetTemopEventArray()
        {
            List<MidiEvent> midiEventList = new List<MidiEvent>();
            int lastEventAbsoluteTime = 0;
            foreach (var tempo in osProject.SongTempoList)
            {
                midiEventList.Add(EncodeSetTempoEvent(tempo, ref lastEventAbsoluteTime));
                lastEventAbsoluteTime = tempo.Position;
            }
            return midiEventList.ToArray();
        }

        /// <summary>
        /// 将单个曲速标记转换为设置曲速 MIDI 事件。
        /// </summary>
        /// <param name="tempo">曲速。</param>
        /// <param name="lastEventAbsoluteTime">上一个曲速事件的绝对时间，单位为梯。</param>
        /// <returns>设置曲速 MIDI 事件。</returns>
        private SetTempoEvent EncodeSetTempoEvent(SongTempo tempo, ref int lastEventAbsoluteTime)
        {
            SetTempoEvent setTempoEvent = new SetTempoEvent
            {
                MicrosecondsPerQuarterNote = GetMicrosecondsPerQuarterNote((long)tempo.BPM),
                DeltaTime = tempo.Position - lastEventAbsoluteTime
            };
            lastEventAbsoluteTime = tempo.Position;
            return setTempoEvent;
        }

        /// <summary>
        /// 转换演唱轨。
        /// </summary>
        /// <param name="singingTrack">原始演唱轨。</param>
        /// <returns>含有音符事件数组的 Track Chunk。</returns>
        private TrackChunk EncodeNoteTrackChunk(SingingTrack singingTrack)
        {
            MidiEvent[] midiEvents = EncodeNoteEventArray(singingTrack);
            TrackChunk trackChunk = new TrackChunk(midiEvents);

            return trackChunk;
        }

        /// <summary>
        /// 转换音符列表。
        /// </summary>
        /// <param name="singingTrack">原始演唱轨。</param>
        /// <returns>带词音符的 MIDI Event 数组。</returns>
        private MidiEvent[] EncodeNoteEventArray(SingingTrack singingTrack)
        {
            if (SemivowelPreShift > 0) // 小于 0 的“前移”可能会导致更多的问题
            {
                for (int index = 0; index < singingTrack.NoteList.Count; index++)//这种方式不好，以后再改。
                {
                    if (IsSemivowelNote(singingTrack.NoteList[index]) && index > 0)//遇到半元音音符，先减短前一个音符的长度（如果有），再提前自身起始位置并加长自身长度。
                    {
                        if (singingTrack.NoteList[index - 1].Length >= SemivowelPreShift)
                        {
                            singingTrack.NoteList[index - 1].Length -= SemivowelPreShift;
                        }
                        else
                        {
                            Warnings.AddWarning("半元音前移量过大，将导致音符长度小于或等于零，已忽略。", $"歌词：{singingTrack.NoteList[index - 1].Lyric}，长度：{singingTrack.NoteList[index - 1].Length}", WarningTypes.Notes);
                        }
                        singingTrack.NoteList[index].StartPos -= SemivowelPreShift;
                        singingTrack.NoteList[index].Length += SemivowelPreShift;
                    }
                }
            }
            List<MidiEvent> midiEventList = new List<MidiEvent>();
            int lastEventAbsoluteTime = 0;
            //bool IsSemivowelShiftForwardHandled = false;
            //这里不能用 DryWetMidi 的音符管理器来转换音符，因为不能设置音符的歌词。需要手动生成歌词、音符按下和松开三个事件。
            foreach (var note in singingTrack.NoteList)
            {
                midiEventList.Add(EncodeLyricEvent(note, lastEventAbsoluteTime));//歌词事件设置在音符按下事件的前面。
                midiEventList.Add(EncodeNoteOnEvent(note));
                midiEventList.Add(EncodeNoteOffEvent(note, ref lastEventAbsoluteTime));
            }
            return midiEventList.ToArray();
        }

        /// <summary>
        /// 判断是否为半元音音符。
        /// </summary>
        private bool IsSemivowelNote(Note note)
        {
            string notePinyin = GetPinyin(note);
            if (notePinyin.StartsWith("y") || notePinyin.StartsWith("w") || notePinyin == "er")
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// 获取音符的拼音。
        /// </summary>
        private string GetPinyin(Note note)
        {
            if (note.Pronunciation != null)
            {
                return note.Pronunciation;
            }
            else
            {
                return Pinyin.GetPinyin(note.Lyric);
            }
        }

        /// <summary>
        /// 将歌词转换成 MIDI 事件。
        /// </summary>
        /// <param name="note">音符。</param>
        /// <param name="lastEventAbsoluteTime">上一个 MIDI 事件的绝对时间。</param>
        /// <returns>歌词事件。</returns>
        private LyricEvent EncodeLyricEvent(Note note, int lastEventAbsoluteTime)
        {
            try
            {
                LyricEvent lyricEvent = new LyricEvent
                {
                    Text = GetLyric(note),
                    DeltaTime = note.StartPos - lastEventAbsoluteTime//歌词事件支撑起乐谱中无音符的空白部分，防止所有音符粘连。
                };
                return lyricEvent;
            }
            catch (System.Exception)
            {
                //用于调试。
                Warnings.AddWarning("[LyricEvent] Text = " + GetLyric(note)
                + " StartPos = " + note.StartPos
                + " Length = " + note.Length
                + " lastEventAbsoluteTime = " + lastEventAbsoluteTime
                + " DeltaTime = " + (note.StartPos - lastEventAbsoluteTime).ToString());
            }
            return new LyricEvent();
        }

        /// <summary>
        /// 根据输出选项转换歌词。
        /// </summary>
        /// <param name="note">音符。</param>
        /// <returns></returns>
        private string GetLyric(Note note)
        {
            string lyric;
            if (note.Pronunciation != null)
            {
                lyric = note.Pronunciation;
            }
            else
            {
                lyric = note.Lyric;
                if (lyric.Length > 1 && IsRemoveSymbols)
                {
                    foreach (var symbol in SymbolList.SymbolToRemoveList())
                    {
                        lyric = lyric.Replace(symbol, "");
                    }
                }
                if (IsUseCompatibleLyric)
                {
                    lyric = Pinyin.GetPinyin(lyric);
                }
            }
            return lyric;
        }

        /// <summary>
        /// 生成 Note On 事件。
        /// </summary>
        /// <param name="note">音符。</param>
        /// <returns>Note On 事件。</returns>
        private NoteOnEvent EncodeNoteOnEvent(Note note)
        {
            NoteOnEvent noteOnEvent = new NoteOnEvent
            {
                NoteNumber = (SevenBitNumber)GetTransposedKeyNumber(note),
                Velocity = (SevenBitNumber)45,
                Channel = (FourBitNumber)0
            };
            return noteOnEvent;
        }

        /// <summary>
        /// 生成 Note Off 事件。
        /// </summary>
        /// <param name="note">音符。</param>
        /// <param name="lastEventAbsoluteTime">上一个 MIDI 事件的绝对时间。</param>
        /// <returns> Note Off 事件。</returns>
        private NoteOffEvent EncodeNoteOffEvent(Note note, ref int lastEventAbsoluteTime)
        {
            NoteOffEvent noteOffEvent = new NoteOffEvent
            {
                NoteNumber = (SevenBitNumber)GetTransposedKeyNumber(note),
                Velocity = (SevenBitNumber)0,
                DeltaTime = note.Length,
                Channel = (FourBitNumber)0
            };
            lastEventAbsoluteTime = note.StartPos + note.Length;
            return noteOffEvent;
        }

        /// <summary>
        /// 获取移调后的音高。
        /// </summary>
        /// <param name="note">音符。</param>
        /// <returns>移调后的音高。</returns>
        private int GetTransposedKeyNumber(Note note)
        {
            int transposedKeyNumber = note.KeyNumber + Transpose;
            if (transposedKeyNumber < 0)
            {
                transposedKeyNumber = 0;
            }
            else if (transposedKeyNumber > 127)
            {
                transposedKeyNumber = 127;
            }
            return transposedKeyNumber;
        }

        /// <summary>
        /// 将曲速转换为每四分音符的微秒数。
        /// </summary>
        /// <param name="BPM">曲速。</param>
        /// <returns>每四分音符的微秒数。</returns>
        private long GetMicrosecondsPerQuarterNote(long BPM)
        {
            return (long)(60.0 / BPM * 1000000.0);
        }

    }
}
