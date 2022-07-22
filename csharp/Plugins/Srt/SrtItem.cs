using System;

namespace FlutyDeer.SrtPlugin
{
    public class SrtItem
    {
        public TimeSpan StartTime { get; set; }

        public TimeSpan EndTime { get; set; }

        public string Lyric { get; set; } = "";

        public override string ToString()
        {
            return $"{StartTime:hh\\:mm\\:ss\\,fff} --> {EndTime:hh\\:mm\\:ss\\,fff}\n{Lyric}";
        }
        public SrtItem(TimeSpan startTime, TimeSpan endTime, string lyric)
        {
            StartTime = startTime;
            EndTime = endTime;
            Lyric = lyric;
        }

        public SrtItem()
        {

        }
    }
}
