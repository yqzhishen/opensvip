using System.Linq;

namespace OxygenDioxide.UstxPlugin.Utils
{
    public static class LyricsUtil
    {
        
        static char[] unsupportedSymbols = { ',', '.', '?', '!', '，', '。', '？', '！'};
            

        public static string GetSymbolRemovedLyric(string lyric)
        {
            return lyric.TrimEnd(unsupportedSymbols);
        }

        //判断字符是否为汉字
        public static bool isHanzi(char c)
        {
            return c >= 0x4e00 && c <= 0x9fbb;
        }

        //判断字符是否为xs支持的标点符号
        public static bool isPunctuation(char c)
        {
            return unsupportedSymbols.Contains(c);
        }
    }
}
