using System.Collections.Generic;
using Gjgj.Model;
using NPinyin;
using OpenSvip.Model;

namespace Plugin.Gjgj
{
    public static class PronunciationUtil
    {
        /// <summary>
        /// 转换音符的拼音。
        /// </summary>
        /// <param name="note">原始音符。</param>
        /// <returns>如果原始发音为空值或者歌叽歌叽不支持，返回空字符串，否则返回原始的发音。</returns>
        public static string GetNotePinyin(Note note, LyricsAndPinyinSettings lyricsAndPinyinSettings, ref List<string> unsupportedPinyinList)
        {
            if ((lyricsAndPinyinSettings == LyricsAndPinyinSettings.lyricsOnly))//仅歌词
            {
                return null;
            }
            else
            {
                if (note.Pronunciation == null)//没有拼音的音符
                {
                    if (lyricsAndPinyinSettings == LyricsAndPinyinSettings.SameAsSource)//和源相同时
                    {
                        return "";
                    }
                    else//仅拼音、歌词和拼音
                    {
                        return Pinyin.GetPinyin(note.Lyric);
                    }
                }
                else//有拼音的音符
                {
                    string pinyin = note.Pronunciation;
                    if (pinyin != "" && !GjgjSupportedPinyin.SupportedPinyinList().Contains(pinyin.ToLower()))
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