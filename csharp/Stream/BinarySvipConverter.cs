using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using OpenSvip.Adapter;
using OpenSvip.Model;

namespace OpenSvip.Stream
{
    public class BinarySvipConverter : IProjectConverter<SingingTool.Model.AppModel>
    {
        public string Version { get; set; } = "SVIP" + SingingTool.Const.ToolConstValues.ProjectVersion;
        
        public BinarySvipConverter() { }

        public BinarySvipConverter(string version)
        {
            Version = version;
        }
        
        public SingingTool.Model.AppModel Load(string path)
        {
            var reader = new FileStream(path, FileMode.Open, FileAccess.Read);
            var buffer = new byte[9];
            reader.ReadByte();
            reader.Read(buffer, 0, 4);
            reader.ReadByte();
            reader.Read(buffer, 4, 5);
            Version = Encoding.Default.GetString(buffer);
            var model = (SingingTool.Model.AppModel) new BinaryFormatter().Deserialize(reader);
            reader.Close();
            reader.Dispose();
            return model;
        }

        public void Save(string path, SingingTool.Model.AppModel model)
        {
            var buffer = Encoding.Default.GetBytes(Version);
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

        public Project Parse(SingingTool.Model.AppModel model)
        {
            return new Project().Decode(Version, model);
        }

        public SingingTool.Model.AppModel Build(Project project)
        {
            var (version, model) = project.Encode();
            Version = version;
            return model;
        }

        public void Reset()
        {
            Version = "SVIP" + SingingTool.Const.ToolConstValues.ProjectVersion;
        }
    }
}
