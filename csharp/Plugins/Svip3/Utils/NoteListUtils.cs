using FlutyDeer.Svip3Plugin.Model;
using OpenSvip.Model;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace FlutyDeer.Svip3Plugin.Utils
{
    public class NoteListUtils
    {
        #region Decoding

        public List<Note> Decode(List<Xs3SingingPattern> patterns)
        {
            var noteList = new List<Note>();
            foreach (var pattern in patterns)
            {
                int offset = pattern.OriginalStartPosition;
                int left = pattern.ClipPosition + offset;
                int right = left + pattern.ClippedDuration;
                var visiableNotes = pattern.NoteList.Where(n => n.StartPosition + offset >= left
                                                                && n.StartPosition + offset + n.Duration <= right);
                foreach (var note in visiableNotes)
                {
                    noteList.Add(DecodeNote(note, offset));
                }
            }
            return noteList;
        }

        private Note DecodeNote(Xs3Note note, int offset)
        {
            return new Note
            {
                StartPos = note.StartPosition + offset,
                Length = note.Duration,
                KeyNumber = note.KeyIndex,
                Lyric = DecodeLyric(note),
                Pronunciation = DecodePronunciation(note),
                HeadTag = HeadTagUtils.Decode(note),
                EditedPhones = PhonemeUtils.Decode(note)
            };
        }

        private string DecodeLyric(Xs3Note note)
        {
            return Regex.IsMatch(note.Lyric, @"[a-zA-Z]")
                ? "啦"
                : note.Lyric;
        }

        private string DecodePronunciation(Xs3Note note)
        {
            return string.IsNullOrEmpty(note.Pronunciation)
                && !note.Lyric.Contains("-")
                ? note.Lyric
                : note.Pronunciation;
        }

        #endregion

        #region Encoding

        public List<Xs3Note> Encode(List<Note> notes)
        {
            var svip3Notes = new List<Xs3Note>();
            foreach (var note in notes)
            {
                svip3Notes.Add(EncodeNote(note));
            }
            return svip3Notes;
        }

        private Xs3Note EncodeNote(Note note)
        {
            return new Xs3Note
            {
                StartPosition = note.StartPos,
                Duration = note.Length,
                KeyIndex = note.KeyNumber,
                Lyric = note.Lyric,
                Pronunciation = note.Pronunciation,
                ConsonantLength = PhonemeUtils.EncodeLength(note),
                HasConsonant = PhonemeUtils.EncodeBool(note),
                SilLength = HeadTagUtils.EncodeSilLen(note.HeadTag),
                SpLength = HeadTagUtils.EncodeSpLen(note.HeadTag)
            };
        }

        #endregion

    }
}
