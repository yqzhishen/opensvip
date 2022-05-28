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

            //导入曲速和拍号
            TempoMap tempoMap = midiFile.GetTempoMap();
            var tempoChanges = tempoMap.GetTempoChanges();
            var timesignatureChanges = tempoMap.GetTimeSignatureChanges();
            foreach (var change in tempoChanges)
            {
                var tempo = change.Value;
                var time = change.Time;
                osProject.SongTempoList.Add(new SongTempo
                {
                    Position = (int)time,
                    BPM = (float)(60000000 / tempo.MicrosecondsPerQuarterNote)
                });
            }
            if (timesignatureChanges != null && timesignatureChanges.Count() > 0)
            {
                var firstTimeSignatureTime = timesignatureChanges.First().Time;
                if (firstTimeSignatureTime != 0)//由于位于0且为四四拍的拍号不存在，所以需要添加一个拍号
                {
                    osProject.TimeSignatureList.Add(new TimeSignature
                    {
                        BarIndex = 0,
                        Numerator = 4,
                        Denominator = 4
                    });
                }
                foreach (var change in timesignatureChanges)
                {
                    var time = change.Time;
                    var midiTimeSignature = change.Value;
                    MetricTimeSpan metricTime = TimeConverter.ConvertTo<MetricTimeSpan>(time, tempoMap);
                    BarBeatTicksTimeSpan barBeatTicksTimeFromMetric = TimeConverter.ConvertTo<BarBeatTicksTimeSpan>(metricTime, tempoMap);
                    osProject.TimeSignatureList.Add(new TimeSignature
                    {
                        BarIndex = (int)barBeatTicksTimeFromMetric.Bars,
                        Numerator = midiTimeSignature.Numerator,
                        Denominator = midiTimeSignature.Denominator
                    });
                }
            }
            else
            {
                osProject.TimeSignatureList.Add(new TimeSignature
                {
                    BarIndex = 0,
                    Numerator = 4,
                    Denominator = 4
                });
            }

            if (ImportLyrics)//导入歌词，使用实验性方法。
            {
                List<Track> singingTrackList = new List<Track>();
                int index = 0;
                foreach (var chunkItem in midiFile.GetTrackChunks())
                {
                    if (index > 0)
                    {
                        singingTrackList.Add(midiEventsUtil.MidiEventsToSingingTrack(chunkItem.Events));
                    }
                    index++;
                }
                osProject.TrackList.AddRange(singingTrackList);
            }
            else//不导入歌词，使用 DryWetMidi 的方法。
            {
                string trackName = "演唱轨";
                List<Track> singingTrackList = new List<Track>();
                foreach (TrackChunk trackChunk in midiFile.GetTrackChunks())
                {
                    var midiEvents = trackChunk.Events;
                    var sequenceTrackNameEvent = midiEvents.Where(x => x.EventType == MidiEventType.SequenceTrackName);
                    if (sequenceTrackNameEvent != null && sequenceTrackNameEvent.Count() > 0)
                    {
                        trackName = ((SequenceTrackNameEvent)sequenceTrackNameEvent.First()).Text;
                    }
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
                            Title = trackName,
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
