using System.IO;
using System.Text;
using Newtonsoft.Json;
using OpenSvip.Model;

namespace OpenSvip.Stream
{
    public static class Binary
    {
        public static Project Read(string path)
        {
            var converter = new BinarySvipConverter();
            var model = converter.Load(path);
            return converter.Parse(model);
        }

        public static void Write(string path, Project project)
        {
            var converter = new BinarySvipConverter();
            var model = converter.Build(project);
            converter.Save(path, model);
        }
    }

    public static class Json
    {
        public static Project Load(string path)
        {
            var stream = new FileStream(path, FileMode.Open, FileAccess.Read);
            var reader = new StreamReader(stream, new UTF8Encoding(false));
            var project = JsonConvert.DeserializeObject<Project>(reader.ReadToEnd());
            stream.Close();
            reader.Close();
            stream.Dispose();
            reader.Dispose();
            return project;
        }

        public static void Dump(string path, Project project, bool indented = false)
        {
            var stream = new FileStream(path, FileMode.Create, FileAccess.Write);
            var writer = new StreamWriter(stream, new UTF8Encoding(false));
            var settings = new JsonSerializerSettings
            {
                Formatting = indented ? Formatting.Indented : Formatting.None,
                ReferenceLoopHandling = ReferenceLoopHandling.Error
            };
            writer.Write(JsonConvert.SerializeObject(project, settings));
            writer.Flush();
            stream.Flush();
            writer.Close();
            stream.Close();
            writer.Dispose();
            stream.Dispose();
        }
    }
}
