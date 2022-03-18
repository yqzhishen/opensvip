using System.IO;
using System.Text;
using Newtonsoft.Json;
using OpenSvip.Framework;
using OpenSvip.Model;
using Plugin.Gjgj;
using Gjgj.Model;
using System.Windows.Forms;

namespace Gjgj.Stream
{
    internal class GjgjConverter : IProjectConverter
    {

        public Project Load(string path, ConverterOptions options)
        {
            var stream = new FileStream(path, FileMode.Open, FileAccess.Read);
            var reader = new StreamReader(stream, Encoding.UTF8);
            var gjProject = JsonConvert.DeserializeObject<GjProject>(reader.ReadToEnd());
            stream.Close();
            reader.Close();
            return new GjgjDecoder().DecodeProject(gjProject);
        }

        public void Save(string path, Project project, ConverterOptions options)
        {
            try
            {
                var gjProject = new GjgjEncoder().EncodeProject(project);
                var stream = new FileStream(path, FileMode.Create, FileAccess.Write);
                var writer = new StreamWriter(stream, Encoding.UTF8);
                var jsonString = JsonConvert.SerializeObject(gjProject);
                foreach (var ch in jsonString)
                {
                    writer.Write(ch);
                    /*if (ch > 32 && ch < 127)
                    {
                        writer.Write(ch);
                    }
                    else
                    {
                        writer.Write($@"\u{(int)ch:x4}");
                    }*/
                }
                writer.Flush();
                stream.WriteByte(0);
                stream.Flush();
                writer.Close();
                stream.Close();
            }
            catch (System.Exception e)
            {
                MessageBox.Show("错误：" + e);
                throw;
            }
        }
    }
}
