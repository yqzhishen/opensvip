using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace FlutyDeer.Svip3Plugin.Utils
{
    public static class Singers
    {
        private static readonly Dictionary<string, string> SingerNames;

        static Singers()
        {
            string dictPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + @"\SingerDict.json";
            using (var stream = File.OpenRead(dictPath))
            {
                using (var reader = new StreamReader(stream, Encoding.UTF8))
                SingerNames = JsonConvert.DeserializeObject<Dictionary<string, string>>(reader.ReadToEnd());
            }
        }

        public static string GetName(string uuid)
        {
            return SingerNames.ContainsKey(uuid)
                ? SingerNames[uuid]
                : string.Empty;
        }

        public static string GetUuid(string name)
        {
            foreach (var pair in SingerNames.Where(p => p.Value.Equals(name)))
            {
                return pair.Key;
            }
            return SingerNames.Keys.First();
        }
    }
}
