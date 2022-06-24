using System;
using System.IO;
using System.Text;

using OpenSvip.Framework;
using OpenSvip.Model;

using OpenUtau.Core.Ustx;
using OpenUtau.Core;

namespace OxygenDioxide.UstxPlugin.Stream
{
    public class UstxConverter : IProjectConverter
    {
        public Project Load(string path, ConverterOptions options)
        {
            //首先把ustx文件解析成openutau UProject对象
            string text = File.ReadAllText(path, Encoding.UTF8);
            UProject ustxProject = Yaml.DefaultDeserializer.Deserialize<UProject>(text);
            //然后从UProject对象中提取出信息，生成Project对象
            return new UstxDecoder().DecodeProject(ustxProject);
        }
        public void Save(string path, Project project, ConverterOptions options)
        { 
            UProject ustxProject = new UstxEncoder().EncodeProject(project);
            string text = Yaml.DefaultSerializer.Serialize(ustxProject); 
            File.WriteAllText(path, text);
        }
    }
}
