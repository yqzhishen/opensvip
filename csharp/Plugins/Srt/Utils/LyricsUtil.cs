using System.Collections.Generic;
using System.Linq;

namespace FlutyDeer.SrtPlugin.Utils
{
    public static class LyricsUtil
    {
        public static List<string> SymbolToRemoveList()
        {
            string[] unsupportedSymbolArray = { ",", ".", "?", "!", "，", "。", "？", "！" };
            return unsupportedSymbolArray.ToList();
        }

        public static string GetSymbolRemovedLyric(string lyric)
        {
            foreach (var symbol in SymbolToRemoveList())
            {
                lyric = lyric.Replace(symbol, "");
            }
            lyric = lyric.Replace("-", "");
            return lyric;
        }

        /// <summary>
        /// 歌词是否包含标点符号。
        /// </summary>
        /// <param name="lyric"></param>
        /// <returns></returns>
        public static bool ContainsSymbol(string lyric)
        {
            bool containsSymbol = false;
            foreach (var symbol in SymbolToRemoveList())
            {
                if (lyric.Contains(symbol))
                {
                    containsSymbol = true;
                    break;
                }
            }
            return containsSymbol;
        }
    }
}
