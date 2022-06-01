using System.Collections.Generic;
using System.IO;
using System.Text;
using Newtonsoft.Json;
using OpenSvip.Framework;
using OpenSvip.Model;

namespace FlutyDeer.Y77Plugin
{
    public static class Test
    {
        public static void Main(string[] args)
        {
            Json2Y77();
        }

        private static void Y772Json()
        {
            var project = new Y77Converter().Load(@"E:\编曲学习\破云来\破云来带词midi.mid", new ConverterOptions(new Dictionary<string, string>()));
            var stream = new FileStream(
                @"E:\编曲学习\破云来\破云来.json",
                FileMode.Create,
                FileAccess.Write);
            var writer = new StreamWriter(stream, new UTF8Encoding(false));
            writer.Write(JsonConvert.SerializeObject(project));
            writer.Flush();
            stream.Flush();
            writer.Close();
            stream.Close();
        }
        private static void Json2Y77()
        {
            var stream = new FileStream(
                @"E:\编曲学习\岁月成碑\歌声合成工程\岁月成碑test.json",
                FileMode.Open,
                FileAccess.Read);
            var reader = new StreamReader(stream, Encoding.UTF8);
            var project = JsonConvert.DeserializeObject<Project>(reader.ReadToEnd());
            stream.Close();
            reader.Close();
            new Y77Converter().Save(@"E:\编曲学习\岁月成碑\歌声合成工程\岁月成碑test.y77", project, new ConverterOptions(new Dictionary<string, string>()));
        }
    }
}
