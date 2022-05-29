using OpenSvip.Model;
using System.Collections.Generic;
using System.Linq;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Common;
using Note = OpenSvip.Model.Note;
using System.Text.RegularExpressions;
using NPinyin;
using OpenSvip.Framework;

namespace FlutyDeer.MidiPlugin
{
    public class MidiEventsUtil
    {
        private short PPQ;

        public void SetPPQ(short PPQ)
        {
            this.PPQ = PPQ;
        }

        public int SemivowelPreShift { get; set; }
        public bool IsUseCompatibleLyric { get; set; }
        public bool IsRemoveSymbols { get; set; }
        public int Transpose { get; set; }

        /// <summary>
        /// 将曲速标记列表转换为 MIDI 事件数组。
        /// </summary>
        /// <returns>含有曲速事件的 MIDI Event 数组。</returns>
        public MidiEvent[] SongTempoListToMidiEvents(List<SongTempo> songTempoList)
        {
            List<MidiEvent> midiEventList = new List<MidiEvent>();
            int PreviousEventTime = 0;
            foreach (var tempo in songTempoList)
            {
                midiEventList.Add(EncodeSetTempoEvent(tempo, ref PreviousEventTime));
                PreviousEventTime = tempo.Position;
            }
            return midiEventList.ToArray();
        }

        /// <summary>
        /// 将单个曲速标记转换为设置曲速 MIDI 事件。
        /// </summary>
        /// <param name="tempo">曲速。</param>
        /// <param name="PreviousEventTime">上一个曲速事件的绝对时间，单位为梯。</param>
        /// <returns>设置曲速 MIDI 事件。</returns>
        private SetTempoEvent EncodeSetTempoEvent(SongTempo tempo, ref int PreviousEventTime)
        {
            SetTempoEvent setTempoEvent = new SetTempoEvent
            {
                MicrosecondsPerQuarterNote = BPMToMicrosecondsPerQuarterNote((long)tempo.BPM),
                DeltaTime = tempo.Position - PreviousEventTime
            };
            PreviousEventTime = tempo.Position;
            return setTempoEvent;
        }

        /// <summary>
        /// 将曲速转换为每四分音符的微秒数。
        /// </summary>
        /// <param name="BPM">曲速。</param>
        /// <returns>每四分音符的微秒数。</returns>
        public long BPMToMicrosecondsPerQuarterNote(long BPM)
        {
            return (long)(60.0 / BPM * 1000000.0);
        }

        /// <summary>
        /// 转换演唱轨。
        /// </summary>
        /// <param name="singingTrack">原始演唱轨。</param>
        /// <returns>含有音符事件数组的 Track Chunk。</returns>
        public TrackChunk SingingTrackToMidiTrackChunk(SingingTrack singingTrack)
        {
            SemivowelPreShiftUtil.PreShiftSemivowelNotes(singingTrack.NoteList, SemivowelPreShift);
            List<MidiEvent> midiEventList = new List<MidiEvent>();
            int PreviousEventTime = 0;
            //bool IsSemivowelShiftForwardHandled = false;
            midiEventList.Add(new SequenceTrackNameEvent(singingTrack.Title));//写入轨道名称
            //这里不能用 DryWetMidi 的音符管理器来转换音符，因为不能设置音符的歌词。需要手动生成歌词、音符按下和松开三个事件。
            foreach (var note in singingTrack.NoteList)
            {
                midiEventList.Add(EncodeLyricEvent(note, PreviousEventTime));//歌词事件设置在音符按下事件的前面。
                midiEventList.Add(EncodeNoteOnEvent(note));
                midiEventList.Add(EncodeNoteOffEvent(note, ref PreviousEventTime));
            }
            TrackChunk trackChunk = new TrackChunk(midiEventList.ToArray());
            return trackChunk;
        }
        
        /// <summary>
        /// 将歌词转换成 MIDI 事件。
        /// </summary>
        /// <param name="note">音符。</param>
        /// <param name="PreviousEventTime">上一个 MIDI 事件的绝对时间。</param>
        /// <returns>歌词事件。</returns>
        private LyricEvent EncodeLyricEvent(Note note, int PreviousEventTime)
        {
            try
            {
                LyricEvent lyricEvent = new LyricEvent
                {
                    Text = GetLyric(note),
                    DeltaTime = note.StartPos - PreviousEventTime//歌词事件支撑起乐谱中无音符的空白部分，防止所有音符粘连。
                };
                return lyricEvent;
            }
            catch (System.Exception)
            {
                //用于调试。
                /* Warnings.AddWarning("[LyricEvent] Text = " + GetLyric(note)
                + " StartPos = " + note.StartPos
                + " Length = " + note.Length
                + " PreviousEventTime = " + PreviousEventTime
                + " DeltaTime = " + (note.StartPos - PreviousEventTime).ToString()); */
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
        /// <param name="PreviousEventTime">上一个 MIDI 事件的绝对时间。</param>
        /// <returns> Note Off 事件。</returns>
        private NoteOffEvent EncodeNoteOffEvent(Note note, ref int PreviousEventTime)
        {
            NoteOffEvent noteOffEvent = new NoteOffEvent
            {
                NoteNumber = (SevenBitNumber)GetTransposedKeyNumber(note),
                Velocity = (SevenBitNumber)0,
                DeltaTime = note.Length,
                Channel = (FourBitNumber)0
            };
            PreviousEventTime = note.StartPos + note.Length;
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

        /* public SingingTrack MidiEventsToSingingTrackLegacy(IEnumerable<MidiEvent> midiEvents, SevenBitNumber channel)
        {
            double previousEventTime = 0;
            List<Note> noteList = new List<Note>();
            string tempLyric = "啊";
            string tempPronunciation = null;
            int tempStratPosition = 0;
            int tempDuration = 0;
            int tempKeyNumber = 0;
            string trackName = "演唱轨";
            var sequenceTrackNameEvent = midiEvents.Where(x => x.EventType == MidiEventType.SequenceTrackName);
            if (sequenceTrackNameEvent != null)
            {
                trackName = ((SequenceTrackNameEvent)sequenceTrackNameEvent.First()).Text;
            }
            foreach (var midiEvent in midiEvents)
            {
                switch (midiEvent.EventType)
                {
                    case MidiEventType.Lyric:
                        string lyric = ((LyricEvent)midiEvent).Text;
                        if (Regex.IsMatch(lyric, @"[a-zA-Z]"))
                        {
                            tempLyric = "啊";
                            tempPronunciation = lyric;
                        }
                        else
                        {
                            tempLyric = lyric;
                        }
                        break;
                    case MidiEventType.NoteOn:
                        NoteOnEvent noteOnEvent = (NoteOnEvent)midiEvent;
                        if (noteOnEvent.Channel == channel)
                        {
                            tempKeyNumber = noteOnEvent.NoteNumber;
                            tempStratPosition = (int)(previousEventTime + midiEvent.DeltaTime);
                        }
                        break;
                    case MidiEventType.NoteOff:
                        NoteOffEvent noteOffEvent = (NoteOffEvent)midiEvent;
                        if (noteOffEvent.Channel == channel)
                        {
                            tempDuration = (int)midiEvent.DeltaTime;
                            Note note = new Note
                            {
                                Lyric = tempLyric,
                                Pronunciation = tempPronunciation,
                                StartPos = (int)(tempStratPosition * 480.0 / PPQ),
                                Length = (int)(tempDuration * 480.0 / PPQ),
                                KeyNumber = tempKeyNumber
                            };
                            tempPronunciation = null;
                            tempLyric = "啊";
                            noteList.Add(note);
                        }
                        break;
                }
                previousEventTime += (int)midiEvent.DeltaTime;
            }
            return new SingingTrack
            {
                Title = trackName,
                Mute = false,
                Solo = false,
                Volume = 0.7,
                Pan = 0.0,
                AISingerName = "陈水若",
                ReverbPreset = "干声",
                NoteList = noteList
            };
        } */
    }
}