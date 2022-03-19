using System.Collections.Generic;
using System.IO;
using System.Text;
using Newtonsoft.Json;
using OpenSvip.Framework;
using OpenSvip.Model;
using SynthV.Stream;

namespace Plugin.SynthV
{
    public class Test
    {
        public static void Main(string[] args)
        {
            var stream = new FileStream(
                @"C:\Users\YQ之神\Desktop\当你.json",
                FileMode.Open,
                FileAccess.Read);
            var reader = new StreamReader(stream, Encoding.UTF8);
            var project = JsonConvert.DeserializeObject<Project>(reader.ReadToEnd());
            stream.Close();
            reader.Close();
            new SynthVConverter().Save(@"C:\Users\YQ之神\Desktop\当你.svp", project, new ConverterOptions(new Dictionary<string, string>()));
        }
    }
}
