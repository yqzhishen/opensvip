using FlutyDeer.GjgjPlugin.Model;
using FlutyDeer.GjgjPlugin.Options;
using OpenSvip.Library;
using OpenSvip.Model;
using System.Collections.Generic;

namespace FlutyDeer.GjgjPlugin.Utils
{
    public class NoteListUtil
    {
        public int FirstBarLength { get; set; }
        public bool IsUseLegacyPinyin { get; set; }
        public TimeSynchronizer TimeSynchronizer { get; set; }
        public LyricsAndPinyinOption LyricsAndPinyinOption { get; set; }


        /// <summary>
        /// 返回转换后的音符列表。
        /// </summary>
        /// <param name="noteID">音符ID。</param>
        /// <param name="singingTrack">原始演唱轨。</param>
        public List<GjNote> EncodeNoteList(int noteID, List<Note> noteList)
        {
            List<GjNote> gjNoteList = new List<GjNote>();
            if (IsUseLegacyPinyin)
            {
                foreach (var note in noteList)
                {
                    gjNoteList.Add(EncodeNote(noteID, note));
                    noteID++;
                }
            }
            else
            {
                List<string> lyricList = new List<string>();
                lyricList.Clear();
                foreach (var note in noteList)
                {
                    lyricList.Add(note.Lyric);
                }
                PinyinAndLyricUtil.ClearAllPinyin();
                PinyinAndLyricUtil.AddPinyin(lyricList);
                int index = 0;
                foreach (var note in noteList)
                {
                    gjNoteList.Add(EncodeNote(noteID, note, index));
                    noteID++;
                    index++;
                }
            }
            return gjNoteList;
        }

        /// <summary>
        /// 返回转换后的音符。
        /// </summary>
        /// <param name="noteID">音符ID。</param>
        /// <param name="note">原始音符。</param>
        private GjNote EncodeNote(int noteID, Note note, int index = 0)
        {
            GjNote gjNote = new GjNote
            {
                NoteID = noteID,
                Pinyin = PinyinAndLyricUtil.GetNotePinyin(note, IsUseLegacyPinyin, index),
                Lyric = PinyinAndLyricUtil.GetNoteLyric(note),
                StartTick = note.StartPos + FirstBarLength,
                Duration = note.Length,
                KeyNumber = note.KeyNumber,
                PhonePreTime = PhonemeUtil.GetNotePhonePreTime(note),
                PhonePostTime = PhonemeUtil.GetNotePhonePostTime(note),
                Style = NoteHeadTagUtil.ToIntTag(note.HeadTag),
                Velocity = 127
            };
            return gjNote;
        }
    }
}
