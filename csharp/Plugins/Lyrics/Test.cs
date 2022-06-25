using System.Collections.Generic;
using System.IO;
using System.Text;
using FlutyDeer.LyricsPlugin.Stream;
using Newtonsoft.Json;
using OpenSvip.Framework;
using OpenSvip.Model;

namespace FlutyDeer.LyricsPlugin
{
    public static class Test
    {
        public static void Main(string[] args)
        {
            Json2Lrc();
        }

        private static void Json2Lrc()
        {
            var stream = new FileStream(
                @"D:\测试\玻璃做的少女.json",
                FileMode.Open,
                FileAccess.Read);
            var reader = new StreamReader(stream, Encoding.UTF8);
            var project = JsonConvert.DeserializeObject<Project>(reader.ReadToEnd());
            stream.Close();
            reader.Close();
            new LyricsConverter().Save(@"D:\测试\玻璃做的少女.txt", project, new ConverterOptions(new Dictionary<string, string>()));
        }
    }
}
