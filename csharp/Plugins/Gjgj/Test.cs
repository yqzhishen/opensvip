using System.Collections.Generic;
using System.IO;
using System.Text;
using Newtonsoft.Json;
using OpenSvip.Framework;
using OpenSvip.Model;
using FlutyDeer.GjgjPlugin.Stream;

namespace FlutyDeer.GjgjPlugin
{
    public static class Test
    {
        public static void Main(string[] args)
        {
            //Gj2Json();
            Json2Gj();
        }

        private static void Gj2Json()
        {
            var project = new GjgjConverter().Load(@"D:\测试\红昭愿（测试）.gj", new ConverterOptions(new Dictionary<string, string>()));
            var stream = new FileStream(
                @"D:\测试\红昭愿（测试）.json",
                FileMode.Create,
                FileAccess.Write);
            var writer = new StreamWriter(stream, new UTF8Encoding(false));
            writer.Write(JsonConvert.SerializeObject(project));
            writer.Flush();
            stream.Flush();
            writer.Close();
            stream.Close();
        }
        private static void Json2Gj()
        {
            var stream = new FileStream(
                @"D:\测试\红昭愿（测试）.json",
                FileMode.Open,
                FileAccess.Read);
            var reader = new StreamReader(stream, Encoding.UTF8);
            var project = JsonConvert.DeserializeObject<Project>(reader.ReadToEnd());
            stream.Close();
            reader.Close();
            new GjgjConverter().Save(@"D:\测试\红昭愿（测试）.gj", project, new ConverterOptions(new Dictionary<string, string>()));
        }
    }
}
