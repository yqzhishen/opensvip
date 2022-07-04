using System;

namespace FlutyDeer.LyricsPlugin
{
    public class LyricLine
    {
        public TimeSpan Time { get; set; }
        public string Lyric { get; set; } = "";

        public override string ToString()
        {
            return $"[{Time:mm\\:ss\\.ff}]{Lyric}";
        }
        public LyricLine(TimeSpan time, string lyric)
        {
            Time = time;
            Lyric = lyric;
        }
        
        public LyricLine()
        {
            
        }
    }
}
