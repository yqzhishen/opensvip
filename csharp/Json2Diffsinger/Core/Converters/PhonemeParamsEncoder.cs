using Json2DiffSinger.Core.Models;
using Json2DiffSinger.Options;
using Json2DiffSinger.Utils;
using Newtonsoft.Json;
using OpenSvip.Library;
using OpenSvip.Model;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Json2DiffSinger.Core.Converters
{
    /// <summary>
    /// 用于生成音素模式的 ds 参数。
    /// </summary>
    public static class PhonemeParamsEncoder
    {
        /// <summary>
        /// 标准 MIDI 长度（不要动）。
        /// </summary>
        static readonly bool IsStandardMidiNoteLength = true;
        /// <summary>
        /// 是否为自动音素模式。
        /// </summary>
        static bool isAutoPhoneme = true;
        public static string Encode(Project project, bool isIntended, ModeOption mode)
        {
            if (mode == ModeOption.ManualPhoneme)
            {
                isAutoPhoneme = false;
            }
            TimeSynchronizer synchronizer = new TimeSynchronizer(project.SongTempoList);
            int firstBarLength = 1920 * project.TimeSignatureList[0].Numerator / project.TimeSignatureList[0].Denominator;
            SingingTrack singingTrack = project.TrackList.Where(t => t is SingingTrack)
                                                         .Select(t => (SingingTrack)t)
                                                         .First();
            List<Note> osNotes = singingTrack.NoteList;
            List<DsNote> dsNotes = new List<DsNote>();
            int prevEndInTicks = 0;
            double prevActualEndInSecs = 0;
            int index = 0;
            var prevPhoneme = new DsPhoneme();
            Tuple<string, string> splitedPinyin;
            //将歌词导入 PinyinUtil 静态类并转为拼音，以便后面取出
            List<string> lyricList = new List<string>();
            lyricList.Clear();
            foreach (var note in osNotes)
            {
                lyricList.Add(note.Lyric);
            }
            PinyinUtil.ClearAllPinyin();
            PinyinUtil.AddPinyinFromLyrics(lyricList);
            foreach (var note in osNotes)
            {
                double prevEndInSecs = synchronizer.GetActualSecsFromTicks(prevEndInTicks);
                int curStartInTicks = note.StartPos;
                int curEndInTicks = curStartInTicks + note.Length;
                double curStartInSecs = synchronizer.GetActualSecsFromTicks(curStartInTicks);
                double curEndInSecs = synchronizer.GetActualSecsFromTicks(curEndInTicks);
                double curActualStartInSecs = curStartInSecs;
                double curActualEndInSecs = curEndInSecs;
                if (note.EditedPhones != null && note.EditedPhones.HeadLengthInSecs != -1.0f)
                {
                    curActualStartInSecs -= note.EditedPhones.HeadLengthInSecs;
                }
                if (index < osNotes.Count - 1 && osNotes[index + 1].EditedPhones != null && osNotes[index + 1].EditedPhones.HeadLengthInSecs != -1.0f)
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
                double gap;//音符间隙
                if (IsStandardMidiNoteLength)
                {
                    gap = curStartInSecs - prevEndInSecs;
                }
                else
                {
                    gap = curActualStartInSecs - prevActualEndInSecs;
                }
                if (gap > 0)//有间隙
                {
                    if (gap < 0.5)//间隙很小，休止
                    {
                        var restPhoneme = new RestDsPhoneme((float)Math.Round(gap, 6));
                        var restNote = new RestDsNote(restPhoneme);
                        dsNotes.Add(restNote);
                        prevPhoneme = restPhoneme;
                    }
                    else if (gap < 1.0)//间隙适中，换气
                    {
                        var aspPhoneme = new AspirationDsPhoneme((float)Math.Round(gap, 6));
                        var apsNote = new AspirationDsNote(aspPhoneme);
                        dsNotes.Add(apsNote);
                        prevPhoneme = aspPhoneme;
                    }
                    else//间隙很大，休止
                    {
                        //double aspDur = 0.5;//换气时长
                        //var restPhoneme = new RestDsPhoneme((float)Math.Round(gap - aspDur, 6));
                        //var restNote = new RestDsNote(restPhoneme);
                        //dsNotes.Add(restNote);
                        //var aspPhoneme = new AspirationDsPhoneme((float)Math.Round(aspDur, 6));
                        //var apsNote = new AspirationDsNote(aspPhoneme);
                        //dsNotes.Add(apsNote);
                        //prevPhoneme = aspPhoneme;
                        var restPhoneme = new RestDsPhoneme((float)Math.Round(gap, 6));
                        var restNote = new RestDsNote(restPhoneme);
                        dsNotes.Add(restNote);
                        prevPhoneme = restPhoneme;
                    }
                }
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
                    splitedPinyin = PinyinUtil.Split(pinyin);
                    if (splitedPinyin.Item1 != "")//不是纯元音
                    {
                        string consonantNoteName = prevPhoneme.Vowel.Phoneme == "SP" || prevPhoneme.Vowel.Phoneme == "AP"
                            ? NoteNameConvert.ToNoteName(note.KeyNumber)
                            : prevPhoneme.Vowel.NoteName;
                        dsPhoneme.Consonant = new DsPhonemeItem
                        {
                            Phoneme = splitedPinyin.Item1,
                            Duration = (float)Math.Round(curStartInSecs - curActualStartInSecs, 6),
                            NoteName = consonantNoteName
                        };
                        dsPhoneme.Vowel = new DsPhonemeItem
                        {
                            Phoneme = splitedPinyin.Item2,
                            Duration = (float)Math.Round(curActualEndInSecs - curStartInSecs, 6),
                            NoteName = NoteNameConvert.ToNoteName(note.KeyNumber)
                        };
                    }
                    else//纯元音
                    {
                        dsPhoneme.Vowel = new DsPhonemeItem
                        {
                            Phoneme = splitedPinyin.Item2,
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
                    //Duration = dsPhoneme.Consonant.Duration + dsPhoneme.Vowel.Duration
                };
                if (IsStandardMidiNoteLength)
                {
                    dsNote.Duration = (float)(curEndInSecs - curStartInSecs);
                }
                else
                {
                    dsNote.Duration = dsPhoneme.Consonant.Duration + dsPhoneme.Vowel.Duration;
                }
                dsNotes.Add(dsNote);
                prevEndInTicks = curEndInTicks;
                prevActualEndInSecs = curActualEndInSecs;
                prevPhoneme = dsPhoneme;
                index++;
            }
            var endRestPhoneme = new RestDsPhoneme(1.0f);
            var endRestNote = new RestDsNote(endRestPhoneme);
            dsNotes.Add(endRestNote);

            string result;
            string inputText = "";
            string phonemeSeq = "";
            string inputNoteSeq = "";
            string inputDurationSeq = "";
            string isSlurSeq = "";
            string phonemeDurSeq = "";
            for (int i = 0; i < dsNotes.Count; i++)
            {
                var curNote = dsNotes[i];
                var dsPhoneme = curNote.DsPhoneme;
                var consonant = dsPhoneme.Consonant;
                var vowel = dsPhoneme.Vowel;
                inputText += curNote.Lyric.Replace("-", "");
                if (consonant.Phoneme != "")
                {
                    phonemeSeq += consonant.Phoneme + " ";
                    phonemeDurSeq += consonant.Duration + " ";
                    isSlurSeq += "0 ";
                    inputDurationSeq += curNote.Duration + " ";
                    //inputNoteSeq += consonant.NoteName + " ";
                    inputNoteSeq += curNote.NoteName + " ";
                }
                phonemeSeq += vowel.Phoneme;
                phonemeDurSeq += vowel.Duration;
                inputNoteSeq += vowel.NoteName;
                inputDurationSeq += curNote.Duration;
                isSlurSeq += curNote.IsSlur ? "1" : "0";
                if (i < dsNotes.Count - 1)
                {
                    if (!curNote.IsSlur)
                    {
                        inputText += " ";
                    }
                    inputNoteSeq += " ";
                    inputDurationSeq += " ";
                    isSlurSeq += " ";
                    phonemeSeq += " ";
                    phonemeDurSeq += " ";
                }
            }

            var model = new PhonemeParamsModel
            {
                LyricText = inputText,
                PhonemeSequence = phonemeSeq,
                NoteSequence = inputNoteSeq,
                NoteDurationSequence = inputDurationSeq,
                IsSlurSequence = isSlurSeq,
                PhonemeDurationSequence = phonemeDurSeq
            };
            if (isAutoPhoneme)
            {
                model.PhonemeDurationSequence = null;
            }
            //result = $"text\n{inputText}\n\n" +
            //    $"phoneme sequence\n{phonemeSeq}\n\n" +
            //    $"note sequence\n{inputNoteSeq}\n\n" +
            //    $"note duration sequence\n{inputDurationSeq}\n\n" +
            //    $"is slur sequence\n{isSlurSeq}\n\n" +
            //    $"phoneme duration sequence\n{phonemeDurSeq}";
            var formatting = new Formatting();
            if (isIntended)
            {
                formatting = Formatting.Indented;
            }
            result = JsonConvert.SerializeObject(model, formatting);
            return result;
        }
    }
}
