using OpenSvip.Model;
using System.Collections.Generic;
using System.Linq;
using Melanchall.DryWetMidi.Core;
using Note = OpenSvip.Model.Note;
using System.Text.RegularExpressions;

namespace FlutyDeer.MidiPlugin
{
    public class MidiEventsUtil
    {
        private short PPQ;

        public void SetPPQ(short PPQ)
        {
            this.PPQ = PPQ;
        }

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

        public SingingTrack MidiEventsToSingingTrack(IEnumerable<MidiEvent> midiEvents)
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
                        tempKeyNumber = ((NoteOnEvent)midiEvent).NoteNumber;
                        tempStratPosition = (int)(previousEventTime + midiEvent.DeltaTime);
                        break;
                    case MidiEventType.NoteOff:
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
        }
    }
}