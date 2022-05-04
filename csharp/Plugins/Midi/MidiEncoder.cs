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
using MidiNote = Melanchall.DryWetMidi.Interaction.Note;
using OsNote = OpenSvip.Model.Note;
using NPinyin;

namespace Plugin.Midi
{
    public class MidiEncoder
    {
        public int Transpose { get; set; }

        public bool IsUseCompatibleLyric { get; set; }
        
        public bool IsRemoveSymbols { get; set; }

        public LyricEncodings LyricEncoding { get; set; }

        public int PPQ { get; set; }

        private Project osProject;

        private MidiFile midiFile = new MidiFile();

        public void EncodeMidiFile(Project project, string path)
        {
            osProject = project;
            TicksPerQuarterNoteTimeDivision timeDivision = new TicksPerQuarterNoteTimeDivision((short)PPQ);
            midiFile.TimeDivision = timeDivision;
            EncodeMidiChunks();
            WritingSettings settings = new WritingSettings
            {
                TextEncoding = GetEncoding()
            };
            midiFile.Write(path, true, settings: settings);
        }

        private void EncodeMidiChunks()
        {
            foreach (var trackChunk in EncodeTrackChunkList())
            {
                midiFile.Chunks.Add(trackChunk);
            }
        }

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
                    return new UTF8Encoding(false);
                default:
                    return Encoding.UTF8;
            }
        }

        private List<TrackChunk> EncodeTrackChunkList()
        {
            List<TrackChunk> trackChunkList = new List<TrackChunk>();
            trackChunkList.Add(EncodeTempoTrackChunk());
            foreach (var track in osProject.TrackList)
            {
                switch (track)
                {
                    case SingingTrack singingTrack:
                        trackChunkList.Add(EncodeTrackChunk(singingTrack));
                        break;
                    default:
                        break;
                }
            }
            return trackChunkList;
        }

        private TrackChunk EncodeTempoTrackChunk()
        {
            return new TrackChunk(EncodeSetTemopEventArray());
        }

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

        private TrackChunk EncodeTrackChunk(SingingTrack singingTrack)
        {
            MidiEvent[] midiEvents = EncodeNoteEventArray(singingTrack);

            TrackChunk trackChunk = new TrackChunk(midiEvents);

            return trackChunk;
        }

        private MidiEvent[] EncodeNoteEventArray(SingingTrack singingTrack)
        {
            List<MidiEvent> midiEventList = new List<MidiEvent>();
            int lastEventAbsoluteTime = 0;
            foreach (var note in singingTrack.NoteList)
            {
                midiEventList.Add(EncodeLyricEvent(note, lastEventAbsoluteTime));
                midiEventList.Add(EncodeNoteOnEvent(note));
                midiEventList.Add(EncodeNoteOffEvent(note, ref lastEventAbsoluteTime));
            }
            return midiEventList.ToArray();
        }
        
        private LyricEvent EncodeLyricEvent(OsNote note, int lastEventAbsoluteTime)
        {
            LyricEvent lyricEvent = new LyricEvent
            {
                Text = GetLyric(note),
                DeltaTime = note.StartPos - lastEventAbsoluteTime
            };
            return lyricEvent;
        }

        private string GetLyric(OsNote note)
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

        private NoteOnEvent EncodeNoteOnEvent(OsNote note)
        {
            NoteOnEvent noteOnEvent = new NoteOnEvent
            {
                NoteNumber = (SevenBitNumber)(note.KeyNumber + Transpose),
                Velocity = (SevenBitNumber)45,
                Channel = (FourBitNumber)0
            };
            return noteOnEvent;
        }

        private NoteOffEvent EncodeNoteOffEvent(OsNote note, ref int lastEventAbsoluteTime)
        {
            NoteOffEvent noteOffEvent = new NoteOffEvent
            {
                NoteNumber = (SevenBitNumber)(note.KeyNumber + Transpose),
                Velocity = (SevenBitNumber)0,
                DeltaTime = note.Length,
                Channel = (FourBitNumber)0
            };
            lastEventAbsoluteTime = note.StartPos + note.Length;
            return noteOffEvent;
        }

        private long GetMicrosecondsPerQuarterNote(long BPM)
        {
            return (long)(60.0 / BPM * 1000000.0);
        }

    }
}
