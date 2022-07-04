using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlutyDeer.LyricsPlugin
{
    public class LyricsFile
    {
        public List<MetaInfoLine> MetaInfoLines { get; set; }
        public List<LyricLine> LyricLines { get; set; }
        public int Count => LyricLines.Count();
        public LyricsFile()
        {
            MetaInfoLines = new List<MetaInfoLine>();
            LyricLines = new List<LyricLine>();
        }

        public LyricsFile(List<LyricLine> lyricLines)
        {
            MetaInfoLines = new List<MetaInfoLine>();
            LyricLines = lyricLines;
        }

        //public override string ToString()
        //{
        //    var sb = new StringBuilder();
        //    foreach (var meta in MetaInfoLines)
        //    {
        //        if (meta.Value != null && meta.Value != "")
        //        {
        //            sb.AppendLine(meta.ToString());
        //        }
        //    }
        //    foreach (var line in LyricLines)
        //    {
        //        sb.AppendLine(line.ToString());
        //    }
        //    return sb.ToString();
        //}

        public void AddLyric(LyricLine line)
        {
            LyricLines.Add(line);
            LyricLines.OrderBy(l => l.Time);
        }
        
        public void AddMeta(MetaInfoLine line)
        {
            MetaInfoLines.Add(line);
        }

        public void Write(string path, WritingSettings writingSettings)
        {
            var stream = new FileStream(path, FileMode.Create, FileAccess.Write);
            var writer = new StreamWriter(stream, writingSettings.Encoding);
            foreach (var meta in MetaInfoLines)
            {
                if (meta.Type == MetaInfoType.Offset)
                {
                    if (meta.Value != "0")
                    {
                        writer.WriteLine(meta.ToString());
                    }
                }
                else
                {
                    if (meta.Value != null && meta.Value != "")
                    {
                        writer.WriteLine(meta.ToString());
                    }
                }
            }
            foreach (var line in LyricLines)
            {
                var content = writingSettings.WriteTimeLine
                    ? line.ToString()
                    : line.Lyric;
                writer.WriteLine(content);
            }
            writer.Flush();
            stream.Flush();
            writer.Close();
            stream.Close();
        }
    }
}
