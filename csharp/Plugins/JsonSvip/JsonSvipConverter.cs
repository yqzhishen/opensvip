using System.IO;
using System.Text;
using Newtonsoft.Json;
using OpenSvip.Framework;
using OpenSvip.Model;

namespace OpenSvip.Stream
{
    public class JsonSvipConverter : IProjectConverter
    {
        public Project Load(string path, ConverterOptions options)
        {
            var stream = new FileStream(path, FileMode.Open, FileAccess.Read);
            var reader = new StreamReader(stream, new UTF8Encoding(false));
            var project = JsonConvert.DeserializeObject<Project>(reader.ReadToEnd());
            stream.Close();
            reader.Close();
            return project;
        }

        public void Save(string path, Project project, ConverterOptions options)
        {
            var stream = new FileStream(path, FileMode.Create, FileAccess.Write);
            var writer = new StreamWriter(stream, new UTF8Encoding(false));
            var settings = new JsonSerializerSettings
            {
                Formatting = options.GetValueAsBoolean("indented") ? Formatting.Indented : Formatting.None,
                ReferenceLoopHandling = ReferenceLoopHandling.Error
            };
            writer.Write(JsonConvert.SerializeObject(project, settings));
            writer.Flush();
            stream.Flush();
            writer.Close();
            stream.Close();
        }
    }
}
