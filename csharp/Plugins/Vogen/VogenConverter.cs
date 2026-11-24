using System.IO;
using System.Text;
using Newtonsoft.Json;
using OpenSvip.Framework;
using OpenSvip.Model;
using FlutyDeer.VogenPlugin.Model;
using FlutyDeer.VogenPlugin.Options;

namespace FlutyDeer.VogenPlugin.Stream
{
    internal class VogenConverter : IProjectConverter
    {

        public Project Load(string path, ConverterOptions options)
        {
            string chartJsonPath = Path.GetDirectoryName(path) + "\\" + "chart.json";
            File.Delete(chartJsonPath);
            ZipHelper.UnZip(path, Path.GetDirectoryName(path));
            var stream = new FileStream(chartJsonPath, FileMode.Open, FileAccess.Read);
            var reader = new StreamReader(stream, Encoding.UTF8);
            var vogenProject = JsonConvert.DeserializeObject<VogenProject>(reader.ReadToEnd());
            stream.Close();
            reader.Close();
            File.Delete(chartJsonPath);
            return new VogenDecoder()
            {
                MergePhrase = options.GetValueAsEnum("mergePhrase", MergePhraseOption.Auto)
            }.DecodeProject(vogenProject);
        }

        public void Save(string path, Project project, ConverterOptions options)
        {
            var VogenProject = new VogenEncoder
            {
                Singer = options.GetValueAsString("singer", "Doaz")
            }.EncodeProject(project);
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
            File.Delete(chartJsonPath);
            FileInfo fi = new FileInfo(path);
            fi.CopyTo(chartJsonPath);
            fi.Delete();
            ZipHelper.CompressFile(chartJsonPath, Path.GetDirectoryName(path) + "\\" +  Path.GetFileNameWithoutExtension(path) + ".vog");
            File.Delete(chartJsonPath);
        }
        
    }
}