using FlutyDeer.GjgjPlugin.Model;
using FlutyDeer.GjgjPlugin.Optiions;
using NPinyin;
using OpenSvip.Library;
using OpenSvip.Model;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace FlutyDeer.GjgjPlugin.Utils
{
    public static class PinyinAndLyricUtil
    {
        public static LyricsAndPinyinOption LyricsAndPinyinOption { get; set; }

        private static List<string> pinyinList = new List<string>();

        public static void AddPinyin(List<string> lyricList)
        {
            var pinyinArray = PinyinUtils.GetPinyinSeries(lyricList);
            foreach (var pinyin in pinyinArray)
            {
                pinyinList.Add(pinyin);
            }
        }

        public static void ClearAllPinyin()
        {
            pinyinList.Clear();
        }

        /// <summary>
        /// 转换音符的拼音。
        /// </summary>
        /// <param name="note">原始音符。</param>
        /// <returns>如果原始发音为空值或者歌叽歌叽不支持，返回空字符串，否则返回原始的发音。</returns>
        public static string GetNotePinyin(Note note, bool isUseLegacyPinyin, int index)
        {
            if ((LyricsAndPinyinOption == LyricsAndPinyinOption.lyricsOnly))//仅歌词
            {
                return null;
            }
            else
            {
                if (note.Pronunciation == null)//没有拼音的音符
                {
                    if (LyricsAndPinyinOption == LyricsAndPinyinOption.SameAsSource)//和源相同时
                    {
                        return "";
                    }
                    else//仅拼音、歌词和拼音
                    {
                        string strRet = "";
                        string pinyin = "";
                        if (isUseLegacyPinyin)
                        {
                            pinyin = Pinyin.GetPinyin(note.Lyric).ToLower();
                        }
                        else
                        {
                            pinyin = pinyinList[index];
                        }
                        var collection = Regex.Matches(pinyin, "[a-z]");
                        foreach(var ch in collection)
                        {
                            strRet += ch.ToString();
                        }
                        return strRet;
                    }
                }
                else//有拼音的音符
                {
                    string pinyin = note.Pronunciation.ToLower();
                    if (pinyin != "" && !UnsupportedPinyin.IsSupportedPinyin(pinyin))
                    {
                        UnsupportedPinyin.AddPinyin(pinyin);
                        pinyin = "";//过滤不支持的拼音
                    }
                    return pinyin;
                }
            }
        }

        /// <summary>
        /// 转换音符读音。
        /// </summary>
        /// <returns></returns>
        public static string DecodePronunciation(GjNote gjNote)
        {
            string pinyin = gjNote.Pinyin;
            string pronunciation;
            if (pinyin == "")
            {
                pronunciation = null;
            }
            else
            {
                pronunciation = pinyin;
            }
            return pronunciation;
        }

        public static string GetNoteLyric(Note note)
        {
            if (LyricsAndPinyinOption == LyricsAndPinyinOption.PinyinOnly && !note.Lyric.Contains("-"))
            {
                return "啊";
            }
            else
            {
                return note.Lyric;
            }
        }
    }
}