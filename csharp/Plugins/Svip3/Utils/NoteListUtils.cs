using Google.Protobuf.Collections;
using OpenSvip.Model;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Xstudio.Proto;

namespace FlutyDeer.Svip3Plugin.Utils
{
    public class NoteListUtils
    {
        #region Decoding

        public List<OpenSvip.Model.Note> Decode(RepeatedField<SingingPattern> patterns)
        {
            var noteList = new List<OpenSvip.Model.Note>();
            foreach (var pattern in patterns)
            {
                int offset = pattern.RealPos;
                int left = pattern.PlayPos + offset;
                int right = left + pattern.PlayDur;
                var visiableNotes = pattern.NoteList.Where(n => n.StartPos + offset >= left
                                                                && n.StartPos + offset + n.WidthPos <= right);
                foreach (var note in visiableNotes)
                {
                    noteList.Add(DecodeNote(note, offset));
                }
            }
            return noteList;
        }

        private OpenSvip.Model.Note DecodeNote(Xstudio.Proto.Note note, int offset)
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

        private string DecodeLyric(Xstudio.Proto.Note note)
        {
            return Regex.IsMatch(note.Lyric, @"[a-zA-Z]")
                ? "啦"
                : note.Lyric;
        }

        private string DecodePronunciation(Xstudio.Proto.Note note)
        {
            return string.IsNullOrEmpty(note.Pronouncing)
                && !note.Lyric.Contains("-")
                ? note.Lyric
                : note.Pronouncing;
        }

        #endregion

        #region Encoding

        public RepeatedField<Xstudio.Proto.Note> Encode(List<OpenSvip.Model.Note> notes)
        {
            var svip3Notes = new RepeatedField<Xstudio.Proto.Note>();
            foreach (var note in notes)
            {
                svip3Notes.Add(EncodeNote(note));
            }
            return svip3Notes;
        }

        private Xstudio.Proto.Note EncodeNote(OpenSvip.Model.Note note)
        {
            return new Xstudio.Proto.Note
            {
                StartPos = note.StartPos,
                WidthPos = note.Length,
                KeyIndex = note.KeyNumber,
                Lyric = note.Lyric,
                Pronouncing = note.Pronunciation,
                ConsonantLen = EditedPhonesUtils.Encode(note),
                SilLen = HeadTagUtils.EncodeSilLen(note.HeadTag),
                SpLen = HeadTagUtils.EncodeSpLen(note.HeadTag)
            };
        }

        #endregion

    }
}
