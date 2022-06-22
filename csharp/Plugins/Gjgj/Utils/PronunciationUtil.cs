using System.Collections.Generic;
using System.Text.RegularExpressions;
using FlutyDeer.GjgjPlugin.Model;
using FlutyDeer.GjgjPlugin.Optiions;
using NPinyin;
using OpenSvip.Model;

namespace FlutyDeer.GjgjPlugin.Utils
{
    public static class PronunciationUtil
    {
        /// <summary>
        /// 转换音符的拼音。
        /// </summary>
        /// <param name="note">原始音符。</param>
        /// <returns>如果原始发音为空值或者歌叽歌叽不支持，返回空字符串，否则返回原始的发音。</returns>
        public static string GetNotePinyin(Note note, LyricsAndPinyinOption lyricsAndPinyinSettings, ref List<string> unsupportedPinyinList)
        {
            if ((lyricsAndPinyinSettings == LyricsAndPinyinOption.lyricsOnly))//仅歌词
            {
                return null;
            }
            else
            {
                if (note.Pronunciation == null)//没有拼音的音符
                {
                    if (lyricsAndPinyinSettings == LyricsAndPinyinOption.SameAsSource)//和源相同时
                    {
                        return "";
                    }
                    else//仅拼音、歌词和拼音
                    {
                        string strRet = "";
                        var collection = Regex.Matches(Pinyin.GetPinyin(note.Lyric).ToLower(), "[a-z]");
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
                    if (pinyin != "" && !PinyinUtil.SupportedPinyinList().Contains(pinyin))
                    {
                        if (!unsupportedPinyinList.Contains(pinyin))
                        {
                            unsupportedPinyinList.Add(pinyin);
                        }
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
    }
}