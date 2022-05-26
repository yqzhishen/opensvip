using OpenSvip.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Interaction;
using Note = OpenSvip.Model.Note;
using TimeSignature = OpenSvip.Model.TimeSignature;
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

        public List<Note> MidiEventsToNoteList(IEnumerable<MidiEvent> midiEvents)
        {
            double previousEventTime = 0;
            List<Note> noteList = new List<Note>();
            string tempLyric = "啊";
            string tempPronunciation = null;
            int tempStratPosition = 0;
            int tempDuration = 0;
            int tempKeyNumber = 0;
            foreach (var midiEvent in midiEvents)
            {
                switch (midiEvent.EventType)
                {
                    case MidiEventType.Lyric:
                        string lyricEventStr = midiEvent.ToString();
                        string lyric = lyricEventStr.Split(new string[] { "(", ")" }, StringSplitOptions.RemoveEmptyEntries)[1];
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
                        string noteOnEventStr = midiEvent.ToString();
                        string noteOnStr = noteOnEventStr.Split(new string[] { "(", ")" }, StringSplitOptions.RemoveEmptyEntries)[1];
                        string[] noteOnStrArray = noteOnStr.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);
                        tempKeyNumber = int.Parse(noteOnStrArray[0]);
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
            return noteList;
        }

        public List<SongTempo> MidiEventsToSongTempoList(IEnumerable<MidiEvent> midiEvents)
        {
            List<SongTempo> songTempoList = new List<SongTempo>();
            if (midiEvents.Count() > 0)
            {
                try
                {
                    double previousEventTime = 0;
                    foreach (var midiEvent in midiEvents)
                    {
                        if (midiEvent.EventType == MidiEventType.SetTempo)
                        {
                            string setTempoEventStr = midiEvent.ToString();
                            float microsecondsPerQuarterNote = float.Parse(setTempoEventStr.Split(new string[] { "(", ")" }, StringSplitOptions.RemoveEmptyEntries)[1]);
                            var songTempo = new SongTempo
                            {
                                Position = (int)(previousEventTime * 480.0 / PPQ),
                                BPM = (float)(60.0 / microsecondsPerQuarterNote * 1000000.0)
                            };
                            songTempoList.Add(songTempo);
                        }
                        previousEventTime += midiEvent.DeltaTime;
                    }
                }
                catch (Exception)
                {

                }
            }
            return songTempoList;
        }
    }
}