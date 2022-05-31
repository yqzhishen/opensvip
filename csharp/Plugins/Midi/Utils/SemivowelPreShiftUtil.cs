using System.Collections.Generic;
using NPinyin;
using OpenSvip.Framework;
using OpenSvip.Model;

namespace FlutyDeer.MidiPlugin
{
    public static class SemivowelPreShiftUtil
    {
        public static void PreShiftSemivowelNotes(List<Note> noteList, int SemivowelPreShift)
        {
            if (SemivowelPreShift > 0) // 小于 0 的“前移”可能会导致更多的问题
            {
                for (int index = 0; index < noteList.Count; index++)//这种方式不好，以后再改。
                {
                    if (IsSemivowelNote(noteList[index]) && index > 0)//遇到半元音音符，先减短前一个音符的长度（如果有），再提前自身起始位置并加长自身长度。
                    {
                        int currentNoteStartPos = noteList[index].StartPos;
                        int currentNotePreShiftedStartPos = currentNoteStartPos - SemivowelPreShift;
                        int previousNoteEndPos = noteList[index - 1].StartPos + noteList[index - 1].Length;
                        if (currentNotePreShiftedStartPos > noteList[index - 1].StartPos)//如果前移后没有超过上一个音符的起始位置，才前移，否则忽略。
                        {
                            if (currentNotePreShiftedStartPos < previousNoteEndPos)//如果前移后侵吞了上一个音符的尾部导致重叠，则减短上一个音符的长度，否则不处理。
                            {
                                noteList[index - 1].Length -= previousNoteEndPos - currentNotePreShiftedStartPos;
                            }
                            noteList[index].StartPos -= SemivowelPreShift;
                            noteList[index].Length += SemivowelPreShift;
                        }
                        else
                        {
                            Warnings.AddWarning("半元音前移量过大，将导致音符长度小于或等于零，已忽略。", $"歌词：{noteList[index - 1].Lyric}，长度：{noteList[index - 1].Length}", WarningTypes.Notes);
                        }
                    }
                }
            }
        }
        
        /// <summary>
        /// 判断是否为半元音音符。
        /// </summary>
        private static bool IsSemivowelNote(Note note)
        {
            string notePinyin = GetPinyin(note);
            if (notePinyin.StartsWith("y") || notePinyin.StartsWith("w") || notePinyin == "er")
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// 获取音符的拼音。
        /// </summary>
        private static string GetPinyin(Note note)
        {
            if (note.Pronunciation != null)
            {
                return note.Pronunciation;
            }
            else
            {
                return Pinyin.GetPinyin(note.Lyric);
            }
        }
    }
}