using System.IO;
using System.Text;
using Newtonsoft.Json;
using OpenSvip.Model;
using OpenSvip.Framework;
using Plugin.SynthV;
using SynthV.Model;

namespace SynthV.Stream
{
    internal class SynthVConverter : IProjectConverter
    {
        public Project Load(string path, ConverterOptions options)
        {
            var stream = new FileStream(path, FileMode.Open, FileAccess.Read);
            var reader = new StreamReader(stream, Encoding.UTF8);
            var svProject = JsonConvert.DeserializeObject<SVProject>(reader.ReadToEnd());
            stream.Close();
            reader.Close();
            return new SynthVDecoder().DecodeProject(svProject);
        }

        public void Save(string path, Project project, ConverterOptions options)
        {
            var svProject = new SynthVEncoder().EncodeProject(project);
            var stream = new FileStream(path, FileMode.Create, FileAccess.Write);
            var writer = new StreamWriter(stream, Encoding.UTF8);
            var jsonString = JsonConvert.SerializeObject(svProject);
            foreach (var ch in jsonString)
            {
                if (ch > 32 && ch < 127)
                {
                    writer.Write(ch);
                }
                else
                {
                    writer.Write($@"\u{(int) ch:x4}");
                }
            }
            writer.Flush();
            stream.WriteByte(0);
            stream.Flush();
            writer.Close();
            stream.Close();
        }
    }
}
