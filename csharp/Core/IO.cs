using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using Newtonsoft.Json;
using OpenSvip.Model;

namespace OpenSvip
{
    namespace IO
    {
        public static class Binary
        {
            public static Project Read(string path)
            {
                var reader = new FileStream(path, FileMode.Open, FileAccess.Read);
                var buffer = new byte[9];
                reader.ReadByte();
                reader.Read(buffer, 0, 4);
                reader.ReadByte();
                reader.Read(buffer, 4, 5);
                var version = Encoding.Default.GetString(buffer);
                var model = (SingingTool.Model.AppModel) new BinaryFormatter().Deserialize(reader);
                reader.Close();
                reader.Dispose();
                return new Project().Decode(version, model);
            }

            public static void Write(string path, Project project)
            {
                var tuple = project.Encode();
                var buffer = Encoding.Default.GetBytes(tuple.Item1);
                var writer = new FileStream(path, FileMode.OpenOrCreate, FileAccess.Write);
                writer.WriteByte(4);
                writer.Write(buffer, 0, 4);
                writer.WriteByte(5);
                writer.Write(buffer, 4, 5);
                new BinaryFormatter().Serialize(writer, tuple.Item2);
                writer.Flush();
                writer.Close();
                writer.Dispose();
            }
        }

        public static class Json
        {
            public static Project Load(string path)
            {
                return null;
            }

            public static void Dump(string path, Project project, bool indented = false)
            {
                var stream = new FileStream(path, FileMode.OpenOrCreate, FileAccess.Write);
                var writer = new StreamWriter(stream, Encoding.UTF8);
                var formatting = indented ? Formatting.Indented : Formatting.None;
                writer.Write(JsonConvert.SerializeObject(project, formatting));
                writer.Flush();
                writer.Close();
                writer.Dispose();
                stream.Flush();
                stream.Close();
                stream.Dispose();
            }
        }
    }
}
