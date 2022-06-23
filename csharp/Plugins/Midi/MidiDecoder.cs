using OpenSvip.Model;
using System.Collections.Generic;
using System.Linq;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Interaction;
using Note = OpenSvip.Model.Note;
using FlutyDeer.MidiPlugin.Utils;
using FlutyDeer.MidiPlugin.Options;

namespace FlutyDeer.MidiPlugin
{
    public class MidiDecoder
    {
        public bool IsImportLyrics { get; set; }

        public MultiChannelOption MultiChannelOption { get; set; }

        public string Channels { get; set; }

        public bool IsImportTimeSignatures { get; set; }

        private short PPQ;

        public Project DecodeMidiFile(MidiFile midiFile)
        {
            TicksPerQuarterNoteTimeDivision timeDivision = midiFile.TimeDivision as TicksPerQuarterNoteTimeDivision;
            PPQ = timeDivision.TicksPerQuarterNote;
            TempoMap tempoMap = midiFile.GetTempoMap();
            var osProject = new Project
            {
                SongTempoList = TempoListUtil.DecodeSongTempoList(tempoMap),
                TimeSignatureList = TimeSignatureListUtil.DecodeTimeSignatureList(tempoMap, IsImportTimeSignatures),
                TrackList = DecodeTrackList(midiFile.GetTrackChunks())
            };
            return osProject;
        }

        private List<Track> DecodeTrackList(IEnumerable<TrackChunk> trackChunks)
        {
            //先用DryWetMidi的方法导入音符
            List<Track> singingTrackList = new List<Track>();
            foreach (TrackChunk trackChunk in trackChunks)
            {
                var channels = MultiChannelUtil.GetChannels(Channels);//先获取要导入哪些通道上的音符
                var midiNoteList = new List<Melanchall.DryWetMidi.Interaction.Note>();
                switch (MultiChannelOption)
                {
                    case MultiChannelOption.First:
                        midiNoteList = trackChunk.GetNotes()
                            .Where(n => n.Channel == 0).ToList();
                        ImportTracks(singingTrackList, trackChunk, midiNoteList);
                        break;
                    case MultiChannelOption.Split:
                        for (int i = 0; i < 16; i++)
                        {
                            midiNoteList = trackChunk.GetNotes()
                                    .Where(n => n.Channel == i).ToList();
                            ImportTracks(singingTrackList, trackChunk, midiNoteList);
                        }
                        break;
                    case MultiChannelOption.Custom:
                        foreach (var channel in channels)
                        {
                            midiNoteList = trackChunk.GetNotes()
                                .Where(n => n.Channel == channel).ToList();
                            ImportTracks(singingTrackList, trackChunk, midiNoteList);
                        }
                        break;
                }
            }
            return singingTrackList;
        }

        private void ImportTracks(List<Track> singingTrackList, TrackChunk trackChunk, List<Melanchall.DryWetMidi.Interaction.Note> midiNoteList)
        {
            string trackName = "演唱轨";
            var midiEvents = trackChunk.Events;
            var sequenceTrackNameEvent = midiEvents.Where(x => x.EventType == MidiEventType.SequenceTrackName);
            if (sequenceTrackNameEvent != null && sequenceTrackNameEvent.Count() > 0)
            {
                trackName = ((SequenceTrackNameEvent)sequenceTrackNameEvent.First()).Text;
            }
            if (midiNoteList.Count > 0)
            {
                List<Note> noteList = new List<Note>();
                foreach (Melanchall.DryWetMidi.Interaction.Note midiNote in midiNoteList)
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

                if (IsImportLyrics)//需要导入歌词再从当前Chunk的事件里读取
                {
                    LyricsUtil.ImportLyricsFromTrackChunk(trackChunk, noteList);
                }

                if (new NoteOverlapUtil().IsOverlapedItemsExists(noteList))
                {
                    //Warnings.AddWarning("音符重叠", type: WarningTypes.Notes);
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
    }
}
