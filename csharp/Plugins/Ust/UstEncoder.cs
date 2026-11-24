
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using OpenSvip.Model;

namespace OxygenDioxide.UstPlugin.Stream
{
    class UstEncoder
    {
        public int TrackIndex = 0;
        static readonly Encoding ShiftJIS = Encoding.GetEncoding("shift_jis");

        (List<Note>, List<SongTempo>) PrepareProject(Project project)
        {
            var track = (SingingTrack)project.TrackList
                .Where(trackIt => trackIt is SingingTrack)
                .ElementAt(TrackIndex);
            //fix note overlap
            var notes = track.NoteList;
            if (notes.Count == 0)
            {
                return (new List<Note>(), project.SongTempoList);
            }
            var currentNote = notes[0];
            var result = notes.ToList();
            foreach (var note in notes.Skip(1))
            {
                if (note.StartPos == currentNote.StartPos)
                {
                    if (note.KeyNumber > currentNote.KeyNumber)
                    {
                        result.Remove(currentNote);
                        currentNote = note;
                    }
                    else
                    {
                        result.Remove(note);
                    }
                }
                else if (note.StartPos < currentNote.StartPos + currentNote.Length)
                {
                    currentNote.Length = note.StartPos - currentNote.StartPos;
                    currentNote = note;
                }
                else
                {
                    currentNote = note;
                }
            }
            //Fix first tempo position
            var tempos = project.SongTempoList.OrderBy(x => x.Position).ToList();
            if (tempos.Count > 0)
            {
                //TODO
            }
            //TODO
            return (result, project.SongTempoList);
        }

        static List<UstNote> NotesToUstNotes(IEnumerable<Note> notes)
        {
            var ustNotes = new List<UstNote>();
            var position = 0;
            foreach (var note in notes)
            {
                if (note.StartPos < position)
                {
                    continue;
                }
                if (note.StartPos > position)
                {
                    ustNotes.Add(new UstNote()
                    {
                        position = position,
                        duration = note.StartPos - position,
                        lyric = "R",
                        noteNum = 60,
                    });
                }
                ustNotes.Add(new UstNote()
                {
                    position = note.StartPos,
                    duration = note.Length,
                    lyric = note.Lyric,
                    noteNum = note.KeyNumber
                });
                position = note.StartPos + note.Length;
            }
            // TODO: Insert tempo changes.
            return ustNotes;
        }

        public void SaveFile(Project project, string filePath)
        {
            (var notes, var tempos) = PrepareProject(project);
            var ustNotes = NotesToUstNotes(notes);
            using (var writer = new StreamWriter(filePath, false, ShiftJIS))
            {
                WriteHeader(project, writer);
                for (var i = 0; i < ustNotes.Count; i++)
                {
                    writer.WriteLine($"[#{i:D4}]");
                    ustNotes[i].Write(writer);
                }
                WriteFooter(writer);
            }
        }

        static void WriteHeader(Project project, StreamWriter writer)
        {
            writer.WriteLine("[#SETTING]");
            writer.WriteLine($"Tempo={project.SongTempoList[0].BPM}");
            writer.WriteLine("Tracks=1");
            writer.WriteLine("Mode2=True");
        }
        
        static void WriteFooter(StreamWriter writer)
        {
            writer.WriteLine("[#TRACKEND]");
        }
    }
}