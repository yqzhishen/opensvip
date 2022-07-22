using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlutyDeer.SrtPlugin
{
    public class SrtFile
    {
        public List<SrtItem> SrtItems { get; set; }

        public SrtFile(List<SrtItem> srtItems)
        {
            SrtItems = srtItems;
        }

        public SrtFile()
        {
            SrtItems = new List<SrtItem>();
        }

        public void Add(SrtItem srtItem)
        {
            SrtItems.Add(srtItem);
        }

        public void Write(string path, WritingSettings writingSettings)
        {
            using (var stream = new FileStream(path, FileMode.Create, FileAccess.Write))
            using (var writer = new StreamWriter(stream, writingSettings.Encoding))
            {
                int index = 1;
                foreach (var item in SrtItems)
                {
                    writer.WriteLine(index);
                    writer.WriteLine(item.ToString());
                    writer.Write("\n");
                    index++;
                }
            }
        }
    }
}
