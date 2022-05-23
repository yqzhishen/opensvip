using System.Collections.Generic;
using System.IO;
using System.Text;
using Newtonsoft.Json;
using OpenSvip.Framework;
using OpenSvip.Model;
using SynthV.Stream;

namespace SynthV.Test
{
    public static class Test
    {
        public static void Main(string[] args)
        {
            // Json2Svp();
            Svp2Json();
        }

        private static void Svp2Json()
        {
            var project = new SynthVConverter().Load(@"C:\Users\YQ之神\Desktop\暗香.svp",
                new ConverterOptions(new Dictionary<string, string>
                {
                    {"pitch", "full"}
                }));
            var stream = new FileStream(
                @"C:\Users\YQ之神\Desktop\暗香.json",
                FileMode.Create,
                FileAccess.Write);
            var writer = new StreamWriter(stream, new UTF8Encoding(false));
            writer.Write(JsonConvert.SerializeObject(project));
            writer.Flush();
            stream.Flush();
            writer.Close();
            stream.Close();
        }

        private static void Json2Svp()
        {
            var stream = new FileStream(
                @"C:\Users\YQ之神\Desktop\黏黏黏黏.json",
                FileMode.Open,
                FileAccess.Read);
            var reader = new StreamReader(stream, Encoding.UTF8);
            var project = JsonConvert.DeserializeObject<Project>(reader.ReadToEnd());
            stream.Close();
            reader.Close();
            new SynthVConverter().Save(@"C:\Users\YQ之神\Desktop\test.svp", project,
                new ConverterOptions(new Dictionary<string, string>()));
        }
    }
}