using Json2DiffSinger.Core.Models;
using OpenSvip.Library;
using OpenSvip.Model;
using System;
using System.Collections.Generic;

namespace Json2DiffSinger.Utils
{
    /// <summary>
    /// 用于转换音符列表的工具。
    /// </summary>
    public static class NoteListUtils
    {
        /// <summary>
        /// 将 OpenSvip Model 的音符列表转换为 ds 音符列表
        /// </summary>
        /// <param name="osNotes"></param>
        /// <param name="synchronizer"></param>
        /// <param name="trailingSpace"></param>
        /// <returns></returns>
        public static List<DsNote> Encode(List<Note> osNotes, TimeSynchronizer synchronizer, float trailingSpace = 0.05f)
        {
            List<DsNote> dsNotes = new List<DsNote>();
            int prevEndInTicks = 0;
            double prevActualEndInSecs = 0;
            int index = 0;
            var prevPhoneme = new DsPhoneme();
            InitPinyinUtils(osNotes);
            foreach (var note in osNotes)
            {
                #region Calculate Positions

                double prevEndInSecs = synchronizer.GetActualSecsFromTicks(prevEndInTicks);
                int curStartInTicks = note.StartPos;
                int curEndInTicks = curStartInTicks + note.Length;
                double curStartInSecs = synchronizer.GetActualSecsFromTicks(curStartInTicks);
                double curEndInSecs = synchronizer.GetActualSecsFromTicks(curEndInTicks);
                double curActualStartInSecs = curStartInSecs;
                double curActualEndInSecs = curEndInSecs;
                if (note.EditedPhones != null && note.EditedPhones.HeadLengthInSecs >= 0)
                {
                    curActualStartInSecs -= note.EditedPhones.HeadLengthInSecs;
                }
                if (index < osNotes.Count - 1 && osNotes[index + 1].EditedPhones != null && osNotes[index + 1].EditedPhones.HeadLengthInSecs >= 0)
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

                #endregion

                #region Fill Note Gap

                double gap;//音符间隙
                gap = curActualStartInSecs - prevActualEndInSecs;
                if (gap > 0)//有间隙
                {
                    if (gap < 0.5)//间隙很小，休止
                    {
                        var restPhoneme = new RestDsPhoneme((float)Math.Round(curActualStartInSecs - prevActualEndInSecs, 6));
                        var restNote = new RestDsNote((float)(curStartInSecs - prevEndInSecs), restPhoneme);
                        dsNotes.Add(restNote);
                        prevPhoneme = restPhoneme;
                    }
                    else if (gap < 1.0)//间隙适中，换气
                    {
                        var aspPhoneme = new AspirationDsPhoneme((float)Math.Round(curActualStartInSecs - prevActualEndInSecs, 6));
                        var apsNote = new AspirationDsNote((float)(curStartInSecs - prevEndInSecs), aspPhoneme);
                        dsNotes.Add(apsNote);
                        prevPhoneme = aspPhoneme;
                    }
                    else//间隙很大，休止
                    {
                        var restPhoneme = new RestDsPhoneme((float)Math.Round(curActualStartInSecs - prevActualEndInSecs, 6));
                        var restNote = new RestDsNote((float)(curStartInSecs - prevEndInSecs), restPhoneme);
                        dsNotes.Add(restNote);
                        prevPhoneme = restPhoneme;
                    }
                }

                #endregion

                #region Convert OpenSvip Notes To DsNotes

                var dsPhoneme = new DsPhoneme();
                if (note.Lyric.Contains("-"))//转音
                {
                    dsPhoneme.Vowel = new DsPhonemeItem
                    {
                        Phoneme = prevPhoneme.Vowel.Phoneme,
                        Duration = (float)Math.Round(curActualEndInSecs - curActualStartInSecs, 6),
                        NoteName = NoteNameConvert.ToNoteName(note.KeyNumber)
                    };
                }
                else
                {
                    string pinyin = !string.IsNullOrEmpty(note.Pronunciation)
                        ? note.Pronunciation
                        : PinyinUtil.GetNotePinyin(note.Lyric, index);
                    var (consonant, vowel) = PinyinUtil.Split(pinyin);
                    if (consonant != "")//不是纯元音
                    {
                        string consonantNoteName = prevPhoneme.Vowel.Phoneme == "SP" || prevPhoneme.Vowel.Phoneme == "AP"
                            ? NoteNameConvert.ToNoteName(note.KeyNumber)
                            : prevPhoneme.Vowel.NoteName;
                        dsPhoneme.Consonant = new DsPhonemeItem
                        {
                            Phoneme = consonant,
                            Duration = (float)Math.Round(curStartInSecs - curActualStartInSecs, 6),
                            NoteName = consonantNoteName
                        };
                        dsPhoneme.Vowel = new DsPhonemeItem
                        {
                            Phoneme = vowel,
                            Duration = (float)Math.Round(curActualEndInSecs - curStartInSecs, 6),
                            NoteName = NoteNameConvert.ToNoteName(note.KeyNumber)
                        };
                    }
                    else//纯元音
                    {
                        dsPhoneme.Vowel = new DsPhonemeItem
                        {
                            Phoneme = vowel,
                            Duration = (float)Math.Round(curActualEndInSecs - curActualStartInSecs, 6),
                            NoteName = NoteNameConvert.ToNoteName(note.KeyNumber)
                        };
                    }
                }
                var dsNote = new DsNote
                {
                    Lyric = LyricUtil.GetSymbolRemovedLyric(note.Lyric),
                    DsPhoneme = dsPhoneme,
                    NoteName = NoteNameConvert.ToNoteName(note.KeyNumber),
                    Duration = (float)(curEndInSecs - curStartInSecs)
                };
                dsNotes.Add(dsNote);

                #endregion

                prevEndInTicks = curEndInTicks;
                prevActualEndInSecs = curActualEndInSecs;
                prevPhoneme = dsPhoneme;
                index++;
            }
            InsertEndRestNote(dsNotes, trailingSpace);
            return dsNotes;
        }

        /// <summary>
        /// 在句尾插入一个休止符
        /// </summary>
        /// <param name="dsNotes"></param>
        /// <param name="trailingSpace"></param>
        private static void InsertEndRestNote(List<DsNote> dsNotes, float trailingSpace = 0.05f)
        {
            var endRestPhoneme = new RestDsPhoneme(trailingSpace);
            var endRestNote = new RestDsNote(trailingSpace, endRestPhoneme);
            dsNotes.Add(endRestNote);
        }

        /// <summary>
        /// 初始化中文转工具
        /// </summary>
        /// <param name="osNotes"></param>
        private static void InitPinyinUtils(List<Note> osNotes)
        {
            List<string> lyricList = new List<string>();
            lyricList.Clear();
            foreach (var note in osNotes)
            {
                lyricList.Add(note.Lyric);
            }
            PinyinUtil.ClearAllPinyin();
            PinyinUtil.AddPinyinFromLyrics(lyricList);
        }
    }
}
