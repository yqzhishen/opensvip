using FlutyDeer.MidiPlugin.Options;
using OpenSvip.Library;
using OpenSvip.Model;
using System.Collections.Generic;

namespace FlutyDeer.MidiPlugin.Utils
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

        public static string GetNotePinyin(string noteLyric, int index)
        {
            string pinyin;
            if (noteLyric.Contains("-"))
            {
                return "-";
            }
            else
            {
                pinyin = pinyinList[index];
                return pinyin;
            }
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