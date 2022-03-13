using System;
using System.IO;
using System.Text;
using System.Xml.Serialization;

namespace OpenSvip.Framework
{
    [Serializable]
    [XmlRoot("Application")]
    public class ApplicationInfo
    {

        public static ApplicationInfo Application { get; }

        static ApplicationInfo()
        {
            var path = AppDomain.CurrentDomain.BaseDirectory + "OpenSvip.Application.xml";
            var stream = new FileStream(path, FileMode.Open, FileAccess.Read);
            var reader = new StreamReader(stream, new UTF8Encoding(false));
            Application = (ApplicationInfo) new XmlSerializer(typeof(ApplicationInfo)).Deserialize(reader);
            stream.Close();
            reader.Close();
            stream.Dispose();
            reader.Dispose();
        }

        public string ArtifactId { get; set; }
        public string Version { get; set; }
        public string Url { get; set; }
        public Plugin[] Plugins { get; set; }
    }
}
