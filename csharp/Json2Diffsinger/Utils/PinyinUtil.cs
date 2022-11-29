using OpenSvip.Library;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Json2DiffSinger.Utils
{
    /// <summary>
    /// 用于处理拼音的工具。含声母韵母拆分，汉字转拼音。
    /// </summary>
    public static class PinyinUtil
    {
        private static readonly List<string> pinyinList = new List<string>();

        private static Dictionary<string, string[]> phonemeTable;

        /// <summary>
        /// 将拼音拆分成声母和韵母。
        /// </summary>
        /// <param name="pinyin">拼音。</param>
        /// <returns></returns>
        public static (string, string) Split(string pinyin)
        {
            var contains = phonemeTable.TryGetValue(pinyin, out var phonemes);
            if (!contains || !phonemes.Any())
            {
                return ("", pinyin);
            }

            return phonemes.Length < 2 ? ("", phonemes[0]) : (phonemes[0], phonemes[1]);
        }

        /// <summary>
        /// 从歌词添加拼音。
        /// </summary>
        /// <param name="lyricList">歌词。</param>
        public static void AddPinyinFromLyrics(List<string> lyricList)
        {
            var pinyinArray = PinyinUtils.GetPinyinSeries(lyricList);
            foreach (var pinyin in pinyinArray)
            {
                pinyinList.Add(pinyin);
            }
        }

        public static void LoadPhonemeTable(string path)
        {
            phonemeTable = File.ReadAllLines(path)
                .Select(rule => rule.Split('\t'))
                .ToDictionary(splitRule => splitRule[0], splitRule => splitRule[1].Split());
        }

        /// <summary>
        /// 清空所有拼音。
        /// </summary>
        public static void ClearAllPinyin()
        {
            pinyinList.Clear();
        }

        /// <summary>
        /// 获取音符的拼音。
        /// </summary>
        /// <param name="noteLyric"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        public static string GetNotePinyin(string noteLyric, int index)
        {
            string pinyin;
            if (noteLyric.Contains("-"))
            {
                return "-";
            }

            pinyin = pinyinList[index];
            return pinyin;
        }
    }
}
