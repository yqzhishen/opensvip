using System;

namespace FlutyDeer.LyricsPlugin
{
    public class LyricLine
    {
        public TimeSpan StartTime { get; set; }
        public string Lyric { get; set; } = "";

        public override string ToString()
        {
            return $"[{StartTime:mm\\:ss\\.ff}]{Lyric}";
        }
        public LyricLine(TimeSpan time, string lyric)
        {
            StartTime = time;
            Lyric = lyric;
        }
        
        public LyricLine()
        {
            
        }
    }
}
