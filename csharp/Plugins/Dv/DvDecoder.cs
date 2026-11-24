using System.Linq;
using OpenSvip.Model;
using OxygenDioxide.DvPlugin.Model;

namespace OxygenDioxide.DvPlugin.Stream
{
    public static class DvDecoder
    {
        public static Project DecodeProject(DvProject dvProject)
        {
            var project = new Project();
            project.TrackList = dvProject.tracks
                .OfType<DvSingingTrack>()
                .Select(DecodeTrack)
                .ToList();
            return project;
        }

        public static Track DecodeTrack(DvSingingTrack dvTrack)
        {
            return new SingingTrack
            {
                NoteList = dvTrack.segments
                .SelectMany(DecodeSegment)
                .ToList(),
            };
        }

        public static Note[] DecodeSegment(DvSegment dvSegment)
        {
            return dvSegment.notes
                .Select(n => DecodeNote(n, dvSegment.start))
                .ToArray();
        }
        
        public static Note DecodeNote(DvNote dvNote, int segmentOffset)
        {
            return new Note
            {
                StartPos = dvNote.start + segmentOffset,
                Length = dvNote.duration,
                KeyNumber = dvNote.key,
                Lyric = dvNote.lyrics,
            };
        }
    }
}