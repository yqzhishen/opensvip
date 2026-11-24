using FlutyDeer.MusicXml.Stream;
using OpenSvip.Framework;
using OpenSvip.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlutyDeer.MusicXml
{
    public class MusicXmlConverter : IProjectConverter
    {
        public Project Load(string path, ConverterOptions options)
        {
            throw new NotImplementedException("暂不支持将本插件作为输入端。");
        }

        public void Save(string path, Project project, ConverterOptions options)
        {
            var scorePartWise = new MusicXmlEncoder().EncodeMusicXml(project);
            using (var stream = new FileStream(path, FileMode.Create, FileAccess.Write))
            using (var writer = new StreamWriter(stream, Encoding.UTF8))
            {
                var xmlString = XmlConvertX.SerializeObject(scorePartWise);
                writer.Write(xmlString);
            }
        }
    }
}
