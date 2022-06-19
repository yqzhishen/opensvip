using OpenSvip.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
                TrackList = EncodeTrackList()
            };
            return vogenProject;
        }

        private string GetTimeSignature(TimeSignature timeSignature)
        {
            return timeSignature.Numerator + "/" + timeSignature.Denominator;
        }

        private List<VogTrack> EncodeTrackList()
        {
            List<VogTrack> vogTrackList = new List<VogTrack>();
            int trackID = 0;
            foreach (var track in osProject.TrackList)
            {
                switch (track)
                {
                    case SingingTrack singingTrack:
                        vogTrackList.Add(EncodeSingingTrack(trackID, singingTrack));
                        trackID++;
                        break;
                    default:
                        break;
                }
            }
            return vogTrackList;
        }

        private VogTrack EncodeSingingTrack(int trackID, SingingTrack singingTrack)
        {
            VogTrack vogTrack = new VogTrack
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
                vogNoteList.Add(EncodeNote(note));
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
