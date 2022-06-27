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
        public IEnumerable<LyricLine> LyricLines { get; set; }
        public int Count => LyricLines.Count();
        public LyricsFile()
        {
            
        }

        public LyricsFile(IEnumerable<LyricLine> lyricLines)
        {
            LyricLines = lyricLines;
        }

        public LyricLine GetLyricLine(int index)
        {
            return LyricLines.ElementAt(index);
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            foreach (var line in LyricLines)
            {
                sb.AppendLine(line.ToString() + "\n");
            }
            return sb.ToString();
        }

        public void Add(LyricLine line)
        {
            LyricLines.Append(line);
            LyricLines.OrderBy(l => l.Time);
        }
        
        public void AddRange(IEnumerable<LyricLine> lines)
        {
            foreach (var line in lines)
            {
                Add(line);
            }
        }

        public void Write(string path)
        {
            var stream = new FileStream(path, FileMode.Create, FileAccess.Write);
            var writer = new StreamWriter(stream, new UTF8Encoding(true));
            foreach (var line in LyricLines)
            {
                writer.WriteLine(line.ToString() + "\n");
            }
            writer.Flush();
            stream.Flush();
            writer.Close();
            stream.Close();
        }
    }
}
