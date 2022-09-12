using Google.Protobuf.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Xstudio.Proto;

namespace FlutyDeer.Svip3Plugin.Utils
{
    public class NoteListUtils
    {
        public List<OpenSvip.Model.Note> Decode(RepeatedField<SingingPattern> patterns)
        {
            var noteList = new List<OpenSvip.Model.Note>();
            foreach (var pattern in patterns)
            {
                int offset = pattern.RealPos;
                int left = pattern.PlayPos + offset;
                int right = left + pattern.PlayDur;
                var visiableNotes = pattern.NoteList.Where(n => n.StartPos >= left
                                                                && n.StartPos + n.WidthPos <= right);
                foreach (var note in visiableNotes)
                {
                    noteList.Add(DecodeNote(note, offset));
                }
            }
            return noteList;
        }

        private OpenSvip.Model.Note DecodeNote(Note note, int offset)
        {
            return new OpenSvip.Model.Note
            {
                StartPos = note.StartPos + offset,
                Length = note.WidthPos,
                KeyNumber = note.KeyIndex,
                Lyric = DecodeLyric(note),
                Pronunciation = DecodePronunciation(note),
                HeadTag = HeadTagUtils.Decode(note),
                EditedPhones = EditedPhonesUtils.Decode(note)
            };
        }

        private string DecodeLyric(Note note)
        {
            return Regex.IsMatch(note.Lyric, @"[a-zA-Z]")
                ? "啦"
                : note.Lyric;
        }

        private string DecodePronunciation(Note note)
        {
            return string.IsNullOrEmpty(note.Pronouncing)
                && !note.Lyric.Contains("-")
                ? note.Lyric
                : note.Pronouncing;
        }
    }
}
