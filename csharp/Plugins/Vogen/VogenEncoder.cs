using OpenSvip.Model;
using System.Collections.Generic;
using FlutyDeer.VogenPlugin.Model;
using NPinyin;

namespace FlutyDeer.VogenPlugin
{
    public class VogenEncoder
    {
        public string Singer { get; set; }

        private Project osProject;

        private VogenProject vogenProject;

        public VogenProject EncodeProject(Project project)
        {
            osProject = project;
            vogenProject = new VogenProject
            {
                BPM = osProject.SongTempoList[0].BPM,
                TimeSignature = GetTimeSignature(osProject.TimeSignatureList[0]),
                InstrumentalOffset = 0,
                PhraseList = EncodeTrackList()
            };
            return vogenProject;
        }

        private string GetTimeSignature(TimeSignature timeSignature)
        {
            return timeSignature.Numerator + "/" + timeSignature.Denominator;
        }

        private List<VogPhrase> EncodeTrackList()
        {
            List<VogPhrase> vogTrackList = new List<VogPhrase>();
            int trackID = 0;
            foreach (var track in osProject.TrackList)
            {
                switch (track)
                {
                    case SingingTrack singingTrack:
                        if (singingTrack.NoteList.Count > 0)//Vogen不支持空轨道
                        {
                            vogTrackList.Add(EncodeSingingTrack(trackID, singingTrack));
                            trackID++;
                        }
                        break;
                    default:
                        break;
                }
            }
            return vogTrackList;
        }

        private VogPhrase EncodeSingingTrack(int trackID, SingingTrack singingTrack)
        {
            VogPhrase vogTrack = new VogPhrase
            {
                TrackName = "utt-" + trackID,
                SingerName = Singer,
                RomScheme = "man",
                NoteList = EncodeNoteList(singingTrack.NoteList)
            };
            return vogTrack;
        }

        private List<VogNote> EncodeNoteList(List<Note> noteList)
        {
            List<VogNote> vogNoteList = new List<VogNote>();
            foreach (var note in noteList)
            {
                if (note.Length > 0)
                {
                    vogNoteList.Add(EncodeNote(note));
                }
            }
            return vogNoteList;
        }

        private VogNote EncodeNote(Note note)
        {
            VogNote vogNote = new VogNote
            {
                KeyNumber = note.KeyNumber,
                Lyric = note.Lyric,
                Pronunciation = GetPronunciation(note),
                StartPosition = note.StartPos,
                Duration = note.Length
            };
            return vogNote;
        }

        private string GetPronunciation(Note note)
        {
            if (note.Lyric == "-")
            {
                return "-";
            }
            else if (note.Pronunciation == null)
            {
                return Pinyin.GetPinyin(note.Lyric);
            }
            else
            {
                return note.Pronunciation;
            }
        }
    }
}
