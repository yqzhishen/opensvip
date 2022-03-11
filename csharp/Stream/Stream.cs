using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using Newtonsoft.Json;
using OpenSvip.Model;

namespace OpenSvip.Stream;

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
        var (version, model) = project.Encode();
        var buffer = Encoding.Default.GetBytes(version);
        var writer = new FileStream(path, FileMode.Create, FileAccess.Write);
        writer.WriteByte(4);
        writer.Write(buffer, 0, 4);
        writer.WriteByte(5);
        writer.Write(buffer, 4, 5);
        new BinaryFormatter().Serialize(writer, model);
        writer.Flush();
        writer.Close();
        writer.Dispose();
    }
}

public static class Json
{
    public static Project Load(string path)
    {
        var stream = new FileStream(path, FileMode.Open, FileAccess.Read);
        var reader = new StreamReader(stream, Encoding.UTF8);
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
