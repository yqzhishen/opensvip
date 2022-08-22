using Json2DiffSinger.Core.Models;
using Json2DiffSinger.Utils;
using Newtonsoft.Json;
using OpenSvip.Library;
using OpenSvip.Model;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Json2DiffSinger.Core.Converters
{
    public static class ChineseCharactersParamsEncoder
    {
        private static readonly bool IsCorrectRhythmIssues = false;
        public static ChineseCharactersParamsModel Encode(Project project, bool isIntended)
        {
            TimeSynchronizer synchronizer = new TimeSynchronizer(project.SongTempoList);
            int firstBarLength = 1920 * project.TimeSignatureList[0].Numerator / project.TimeSignatureList[0].Denominator;
            SingingTrack singingTrack = project.TrackList.Where(t => t is SingingTrack)
                                                         .Select(t => (SingingTrack)t)
                                                         .First();
            List<Note> osNotes = singingTrack.NoteList;
            List<DsNote> dsNotes = new List<DsNote>();
            int prevEndInTicks = 0;
            double preActualEndInSecs = 0;
            int index = 0;
            foreach (var note in osNotes)
            {
                double prevEndInSecs = synchronizer.GetActualSecsFromTicks(prevEndInTicks);
                int curStartInTicks = note.StartPos;
                int curEndInTicks = curStartInTicks + note.Length;
                double curStartInSecs = synchronizer.GetActualSecsFromTicks(curStartInTicks);
                double curEndInSecs = synchronizer.GetActualSecsFromTicks(curEndInTicks);
                double curActualStartInSecs = curStartInSecs;
                double curActualEndInSecs = curEndInSecs;
                if (note.EditedPhones != null
                    && note.EditedPhones.HeadLengthInSecs >= 0
                    && IsCorrectRhythmIssues)
                {
                    curActualStartInSecs -= note.EditedPhones.HeadLengthInSecs;
                }
                if (index < osNotes.Count - 1
                    && osNotes[index + 1].EditedPhones != null
                    && osNotes[index + 1].EditedPhones.HeadLengthInSecs >= 0
                    && IsCorrectRhythmIssues)
                {
                    var nextNote = osNotes[index + 1];
                    int nextStartInTicks = nextNote.StartPos;
                    double nextStartInSecs = synchronizer.GetActualSecsFromTicks(nextStartInTicks);
                    float nextHead = osNotes[index + 1].EditedPhones.HeadLengthInSecs;
                    double nextActualStartInSecs = nextStartInSecs - nextHead;
                    if (curEndInSecs > nextActualStartInSecs)
                    {
                        curActualEndInSecs -= curEndInSecs - nextActualStartInSecs;
                    }
                }
                if (curActualStartInSecs > preActualEndInSecs)
                {
                    var restNote = new DsNote
                    {
                        NoteName = "rest",
                        Duration = (float)Math.Round(curActualStartInSecs - preActualEndInSecs, 6),
                        Lyric = "SP"
                    };
                    dsNotes.Add(restNote);
                }
                var dsNote = new DsNote
                {
                    NoteName = NoteNameConvert.ToNoteName(note.KeyNumber),
                    Duration = (float)Math.Round(curActualEndInSecs - curActualStartInSecs, 6),
                    Lyric = LyricUtil.GetSymbolRemovedLyric(note.Lyric)
                };
                dsNotes.Add(dsNote);
                prevEndInTicks = curEndInTicks;
                preActualEndInSecs = curActualEndInSecs;
                index++;
            }

            string inputText = "";
            string inputNote = "";
            string inputDuration = "";
            for (int i = 0; i < dsNotes.Count; i++)
            {
                var curNote = dsNotes[i];
                inputText += curNote.Lyric.Replace("-", "");
                inputNote += curNote.NoteName;
                inputDuration += curNote.Duration;
                if (i < dsNotes.Count - 1 && !dsNotes[i + 1].Lyric.Contains("-"))
                {
                    inputText += " ";
                    inputNote += " | ";
                    inputDuration += " | ";
                }
                else
                {
                    inputNote += " ";
                    inputDuration += " ";
                }
            }
            //result = $"input text\n{inputText}\n\ninput note\n{inputNote}\n\ninput duration\n{inputDuration}";
            //result = $"{{\n    \"data\": [\n        \"{inputText}\",\n        \"{inputNote}\",\n        \"{inputDuration}\"\n    ]\n}}";
            var model = new ChineseCharactersParamsModel
            {
                LyricText = inputText,
                NoteSequence = inputNote,
                NoteDurationSequence = inputDuration
            };
            return model;
        }
    }
}
