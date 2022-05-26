using OpenSvip.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Interaction;
using Note = OpenSvip.Model.Note;
using TimeSignature = OpenSvip.Model.TimeSignature;

namespace FlutyDeer.MidiPlugin
{
    public class MidiDecoder
    {
        /// <summary>
        /// 歌词文本编码。
        /// </summary>
        public LyricEncodings LyricEncoding { get; set; }
        public bool ImportLyrics { get; set; }
        public ErrorMidiFilePolicyOption ErrorMidiFilePolicy { get; set; }
        private MidiFile midiFile;
        private MidiEventsUtil midiEventsUtil = new MidiEventsUtil();
        private short PPQ;
        public Project DecodeMidiFile(string path)
        {
            var osProject = new Project();
            midiFile = MidiFile.Read(path, GetReadingSettings());
            TicksPerQuarterNoteTimeDivision timeDivision = midiFile.TimeDivision as TicksPerQuarterNoteTimeDivision;
            midiEventsUtil.SetPPQ(timeDivision.TicksPerQuarterNote);
            PPQ = timeDivision.TicksPerQuarterNote;

            if (ImportLyrics)
            {
                bool isTempoMapTrack = true;
                List<Track> singingTrackList = new List<Track>();
                foreach (var chunkItem in midiFile.GetTrackChunks())
                {
                    if (isTempoMapTrack)//解析曲速轨
                    {
                        osProject.SongTempoList.AddRange(midiEventsUtil.MidiEventsToSongTempoList(chunkItem.Events));
                        isTempoMapTrack = false;
                    }
                    else//解析其他轨
                    {
                        singingTrackList.Add(new SingingTrack
                        {
                            Title = "演唱轨",
                            Mute = false,
                            Solo = false,
                            Volume = 0.7,
                            Pan = 0.0,
                            AISingerName = "陈水若",
                            ReverbPreset = "干声",
                            NoteList = midiEventsUtil.MidiEventsToNoteList(chunkItem.Events)
                        });
                    }
                }
                osProject.TrackList.AddRange(singingTrackList);
            }
            else
            {
                List<Track> singingTrackList = new List<Track>();
                foreach (TrackChunk trackChunk in midiFile.GetTrackChunks())
                {
                    List<Melanchall.DryWetMidi.Interaction.Note> list = trackChunk.GetNotes().ToList<Melanchall.DryWetMidi.Interaction.Note>();
                    if (list.Count > 0)
                    {
                        List<Note> noteList = new List<Note>();
                        foreach (Melanchall.DryWetMidi.Interaction.Note midiNote in list)
                        {
                            noteList.Add(new Note
                            {
                                StartPos = (int)(midiNote.Time * 480 / PPQ),
                                Length = (int)(midiNote.Length * 480 / PPQ),
                                KeyNumber = midiNote.NoteNumber,
                                Lyric = "啊",
                                Pronunciation = null
                            });
                        }
                        singingTrackList.Add(new SingingTrack
                        {
                            Title = "演唱轨",
                            Mute = false,
                            Solo = false,
                            Volume = 0.7,
                            Pan = 0.0,
                            AISingerName = "陈水若",
                            ReverbPreset = "干声",
                            NoteList = noteList
                        });
                    }
                }
                osProject.TrackList.AddRange(singingTrackList);
            }

            osProject.TimeSignatureList.Add(new TimeSignature
            {
                BarIndex = 0,
                Numerator = 4,
                Denominator = 4
            });
            return osProject;
        }

        private ReadingSettings GetReadingSettings()
        {

            ReadingSettings readingSettings = new ReadingSettings();
            readingSettings.TextEncoding = EncodingUtil.GetEncoding(LyricEncoding);
            if (ErrorMidiFilePolicy == ErrorMidiFilePolicyOption.Ignore)
            {
                readingSettings.InvalidChannelEventParameterValuePolicy = InvalidChannelEventParameterValuePolicy.ReadValid;
                readingSettings.InvalidChunkSizePolicy = InvalidChunkSizePolicy.Ignore;
                readingSettings.InvalidMetaEventParameterValuePolicy = InvalidMetaEventParameterValuePolicy.SnapToLimits;
                readingSettings.MissedEndOfTrackPolicy = MissedEndOfTrackPolicy.Ignore;
                readingSettings.NoHeaderChunkPolicy = NoHeaderChunkPolicy.Ignore;
                readingSettings.NotEnoughBytesPolicy = NotEnoughBytesPolicy.Ignore;
                readingSettings.UnexpectedTrackChunksCountPolicy = UnexpectedTrackChunksCountPolicy.Ignore;
                readingSettings.UnknownChannelEventPolicy = UnknownChannelEventPolicy.SkipStatusByteAndOneDataByte;
                readingSettings.UnknownChunkIdPolicy = UnknownChunkIdPolicy.ReadAsUnknownChunk;
                readingSettings.UnknownFileFormatPolicy = UnknownFileFormatPolicy.Ignore;
            }
            return readingSettings;
        }
    }
}
