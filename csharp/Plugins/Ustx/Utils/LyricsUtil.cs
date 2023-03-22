namespace OxygenDioxide.UstxPlugin.Utils
{
    public static class LyricsUtil
    {
        
        static char[] unsupportedSymbolArray = { ',', '.', '?', '!', '，', '。', '？', '！'};
            

        public static string GetSymbolRemovedLyric(string lyric)
        {
            return lyric.TrimEnd(unsupportedSymbolArray);
        }
    }
}
