using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlutyDeer.LyricsPlugin
{
    public class LyricLine
    {
        public string Time { get; set; } = "";
        public string Lyric { get; set; } = "";

        public override string ToString()
        {
            return $"[{Time}]{Lyric}";
        }

        public LyricLine(string time, string lyric)
        {
            Time = time;
            Lyric = lyric;
        }
        
        public LyricLine()
        {
            
        }
    }
}
