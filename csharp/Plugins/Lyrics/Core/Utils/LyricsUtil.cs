using System.Collections.Generic;
using System.Linq;

namespace FlutyDeer.MidiPlugin.Utils
{
    public static class LyricsUtil
    {
        public static List<string> SymbolToRemoveList()
        {
            string[] unsupportedSymbolArray = { "-", ",", ".", "?", "!", "，", "。", "？", "！" };
            return unsupportedSymbolArray.ToList();
        }

        public static string GetSymbolRemovedLyric(string lyric)
        {
            foreach (var symbol in SymbolToRemoveList())
            {
                lyric = lyric.Replace(symbol, "");
            }
            return lyric;
        }
    }
}
