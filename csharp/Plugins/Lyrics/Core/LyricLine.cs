using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlutyDeer.LyricsPlugin
{
    public class LyricLine
    {
        public TimeSpan Time { get; set; }
        public string Lyric { get; set; } = "";

        public override string ToString()
        {
            return $"[{Time.Minutes:D2}:{Time.Seconds:D2}.{Time.Milliseconds:D3}]{Lyric}";
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
