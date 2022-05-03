using System;
using System.Collections.Generic;
using System.Text;
using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Interaction;
using Melanchall.DryWetMidi.MusicTheory;
using OpenSvip.Framework;
using OpenSvip.Model;
using MidiNote = Melanchall.DryWetMidi.Interaction.Note;
using OsNote = OpenSvip.Model.Note;

namespace Plugin.Midi
{
    public class MidiEncoder
    {
        private Project osProject;

        private MidiFile midiFile = new MidiFile();

        public void EncodeMidiFile(Project project, string path)
        {
            osProject = project;
            TicksPerQuarterNoteTimeDivision timeDivision = new TicksPerQuarterNoteTimeDivision(480);
            midiFile.TimeDivision = timeDivision;
            EncodeMidiChunks();
            WritingSettings settings = new WritingSettings
            {
                TextEncoding = Encoding.UTF8
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
            return new TrackChunk(new SetTempoEvent(GetMicrosecondsPerQuarterNote(0)));
        }

        private TrackChunk EncodeTrackChunk(SingingTrack singingTrack)
        {
            MidiEvent[] midiEvents = EncodeMidiEventArray(singingTrack);

            TrackChunk trackChunk = new TrackChunk(midiEvents);

            return trackChunk;
        }

        private MidiEvent[] EncodeMidiEventArray(SingingTrack singingTrack)
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
            string lyric = note.Lyric;
            if (lyric.Length > 1)
            {
                foreach (var symbol in SymbolList.SymbolToRemoveList())
                {
                    lyric = lyric.Replace(symbol, "");
                }
            }
            return lyric;
        }

        private NoteOnEvent EncodeNoteOnEvent(OsNote note)
        {
            NoteOnEvent noteOnEvent = new NoteOnEvent
            {
                NoteNumber = (SevenBitNumber)note.KeyNumber,
                Velocity = (SevenBitNumber)45,
                Channel = (FourBitNumber)0
            };
            return noteOnEvent;
        }

        private NoteOffEvent EncodeNoteOffEvent(OsNote note, ref int lastEventAbsoluteTime)
        {
            NoteOffEvent noteOffEvent = new NoteOffEvent
            {
                NoteNumber = (SevenBitNumber)note.KeyNumber,
                Velocity = (SevenBitNumber)0,
                DeltaTime = note.Length,
                Channel = (FourBitNumber)0
            };
            lastEventAbsoluteTime = note.StartPos + note.Length;
            return noteOffEvent;
        }

        /// <summary>
        /// 返回每四分音符的微秒数。
        /// </summary>
        /// <param name="index">原始曲速的索引。</param>
        /// <returns></returns>
        private long GetMicrosecondsPerQuarterNote(int index)
        {
            return (long)(60.0 / osProject.SongTempoList[index].BPM * 1000000.0);
        }
    }
}
