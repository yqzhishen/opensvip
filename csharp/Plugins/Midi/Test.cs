using System.Collections.Generic;
using System.IO;
using System.Text;
using Newtonsoft.Json;
using OpenSvip.Framework;
using OpenSvip.Model;
using FlutyDeer.MidiStream;

namespace FlutyDeer.MidiPlugin
{
    public static class Test
    {
        public static void Main(string[] args)
        {
            Midi2Json();
        }

        private static void Midi2Json()
        {
            var project = new MidiConverter().Load(@"E:\编曲学习\破云来\破云来带词midi.mid", new ConverterOptions(new Dictionary<string, string>()));
            var stream = new FileStream(
                @"E:\编曲学习\破云来\破云来带词midi.json",
                FileMode.Create,
                FileAccess.Write);
            var writer = new StreamWriter(stream, new UTF8Encoding(false));
            writer.Write(JsonConvert.SerializeObject(project));
            writer.Flush();
            stream.Flush();
            writer.Close();
            stream.Close();
        }
        private static void Json2Midi()
        {
            var stream = new FileStream(
                @"E:\编曲学习\破云来\破云来带词midi.json",
                FileMode.Open,
                FileAccess.Read);
            var reader = new StreamReader(stream, Encoding.UTF8);
            var project = JsonConvert.DeserializeObject<Project>(reader.ReadToEnd());
            stream.Close();
            reader.Close();
            new MidiConverter().Save(@"E:\编曲学习\破云来\破云来带词midi.mid", project, new ConverterOptions(new Dictionary<string, string>()));
        }
    }
}
