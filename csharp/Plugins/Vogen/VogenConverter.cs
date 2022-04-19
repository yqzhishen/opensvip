using System;
using System.IO;
using System.Text;
using Newtonsoft.Json;
using OpenSvip.Framework;
using OpenSvip.Model;
using Plugin.Vogen;
using Vogen.Model;

namespace Vogen.Stream
{
    internal class VogenConverter : IProjectConverter
    {

        public Project Load(string path, ConverterOptions options)
        {
            ZipHelper.DecompressFile(path, Path.GetDirectoryName(path));
            string chartJsonPath = Path.GetDirectoryName(path) + "\\" + "chart.json";
            var stream = new FileStream(chartJsonPath, FileMode.Open, FileAccess.Read);
            var reader = new StreamReader(stream, Encoding.UTF8);
            var vogenProject = JsonConvert.DeserializeObject<VogenProject>(reader.ReadToEnd());
            stream.Close();
            reader.Close();
            return new VogenDecoder().DecodeProject(vogenProject);
            //return new Project();
        }

        public void Save(string path, Project project, ConverterOptions options)
        {
            var VogenProject = new VogenEncoder().EncodeProject(project);
            var stream = new FileStream(path, FileMode.Create, FileAccess.Write);
            var writer = new StreamWriter(stream, Encoding.UTF8);
            var jsonString = JsonConvert.SerializeObject(VogenProject);
            foreach (var ch in jsonString)
            {
                writer.Write(ch);
            }
            writer.Flush();
            stream.Flush();
            writer.Close();
            stream.Close();
            string chartJsonPath = Path.GetDirectoryName(path) + "\\" + "chart.json";
            FileInfo fi = new FileInfo(path);
            fi.MoveTo(chartJsonPath);
            ZipHelper.CompressFile(chartJsonPath, Path.GetDirectoryName(path) + "\\" +  Path.GetFileNameWithoutExtension(path) + ".vog");
            File.Delete(chartJsonPath);
        }
        
    }
}