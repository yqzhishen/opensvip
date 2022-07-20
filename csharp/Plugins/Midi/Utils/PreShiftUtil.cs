using System.Collections.Generic;
using NPinyin;
using OpenSvip.Framework;
using OpenSvip.Model;

namespace FlutyDeer.MidiPlugin.Utils
{
    public static class PreShiftUtil
    {
        public static void PreShiftSemivowelNotes(List<Note> noteList, int PreShift)
        {
            if (PreShift <= 0) // 小于 0 的“前移”可能会导致更多的问题
            {
                return;
            }
            for (int index = 0; index < noteList.Count; index++)//这种方式不好，以后再改。
            {
                if (IsSemivowelNote(noteList[index]) && index > 0)//遇到半元音音符，先减短前一个音符的长度（如果有），再提前自身起始位置并加长自身长度。
                {
                    int currentStartPos = noteList[index].StartPos;
                    int preShiftedStartPos = currentStartPos - PreShift;
                    int previousEndPos = noteList[index - 1].StartPos + noteList[index - 1].Length;
                    if (preShiftedStartPos > noteList[index - 1].StartPos)//如果前移后没有超过上一个音符的起始位置，才前移，否则忽略。
                    {
                        if (preShiftedStartPos < previousEndPos)//如果前移后侵吞了上一个音符的尾部导致重叠，则减短上一个音符的长度，否则不处理。
                        {
                            noteList[index - 1].Length -= previousEndPos - preShiftedStartPos;
                        }
                        noteList[index].StartPos -= PreShift;
                        noteList[index].Length += PreShift;
                    }
                    else
                    {
                        Warnings.AddWarning("前移量过大，将导致音符长度小于或等于零，已忽略。", $"歌词：{noteList[index - 1].Lyric}，长度：{noteList[index - 1].Length}", WarningTypes.Notes);
                    }
                }
                else if (IsVowelNote(noteList[index]) && index > 0)//遇到元音音符，先减短前一个音符的长度（如果有），再提前自身起始位置并加长自身长度。
                {
                    int currentStartPos = noteList[index].StartPos;
                    int preShiftedStartPos = currentStartPos - PreShift / 2;
                    int previousEndPos = noteList[index - 1].StartPos + noteList[index - 1].Length;
                    if (preShiftedStartPos > noteList[index - 1].StartPos)//如果前移后没有超过上一个音符的起始位置，才前移，否则忽略。
                    {
                        if (preShiftedStartPos < previousEndPos)//如果前移后侵吞了上一个音符的尾部导致重叠，则减短上一个音符的长度，否则不处理。
                        {
                            noteList[index - 1].Length -= previousEndPos - preShiftedStartPos;
                        }
                        noteList[index].StartPos -= PreShift / 2;
                        noteList[index].Length += PreShift / 2;
                    }
                    else
                    {
                        Warnings.AddWarning("前移量过大，将导致音符长度小于或等于零，已忽略。", $"歌词：{noteList[index - 1].Lyric}，长度：{noteList[index - 1].Length}", WarningTypes.Notes);
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
            return notePinyin.StartsWith("y") || notePinyin.StartsWith("w");
        }

        private static bool IsVowelNote(Note note)
        {
            string notePinyin = GetPinyin(note);
            return notePinyin.StartsWith("a") || notePinyin.StartsWith("o") || notePinyin.StartsWith("e");
        }

        /// <summary>
        /// 获取音符的拼音。
        /// </summary>
        private static string GetPinyin(Note note)
        {
            return note.Pronunciation ?? Pinyin.GetPinyin(note.Lyric);
        }
    }
}