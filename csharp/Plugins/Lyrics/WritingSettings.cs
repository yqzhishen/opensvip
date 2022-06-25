using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlutyDeer.LyricsPlugin
{
    public class WritingSettings
    {
        public Encoding Encoding { get; set; } = Encoding.UTF8;
        public string Title { get; set; } = "";
        public string Artist { get; set; } = "";
        public string Album { get; set; } = "";
    }
}
