using System.Collections.Generic;
using System.Linq;

namespace Json2DiffSinger.Utils
{
    /// <summary>
    /// 用于处理歌词的工具。
    /// </summary>
    public static class LyricUtil
    {
        static List<string> SymbolToRemoveList()
        {
            string[] unsupportedSymbolArray = { ",", ".", "?", "!", "，", "。", "？", "！" };
            return unsupportedSymbolArray.ToList();
        }

        /// <summary>
        /// 移除歌词中的标点符号。
        /// </summary>
        /// <param name="lyric"></param>
        /// <returns></returns>
        public static string GetSymbolRemovedLyric(string lyric)
        {
            if (lyric.Length > 1)
            {
                foreach (var symbol in SymbolToRemoveList())
                {
                    lyric = lyric.Replace(symbol, "");
                }
            }
            return lyric;
        }
    }
}
