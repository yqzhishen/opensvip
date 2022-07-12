using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Newtonsoft.Json;

namespace AceStdio.Resources
{
    public static class AceSingers
    {
        private static readonly Dictionary<int, string> SingerNames;
        static AceSingers()
        {
            var stream = new FileStream(
                Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? string.Empty, "Singers.json"),
                FileMode.Open,
                FileAccess.Read);
            var reader = new StreamReader(stream, Encoding.UTF8);
            SingerNames = JsonConvert.DeserializeObject<Dictionary<int, string>>(reader.ReadToEnd());
            stream.Close();
            reader.Close();
        }
        
        public static string GetName(int id)
        {
            return SingerNames.ContainsKey(id) ? SingerNames[id] : null;
        }

        public static int GetId(string name)
        {
            return SingerNames
                .Where(singer => singer.Value == name)
                .Select(singer => singer.Key)
                .FirstOrDefault();
        }
    }
}