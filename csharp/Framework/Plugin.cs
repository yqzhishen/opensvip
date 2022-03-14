using System;
using System.Xml.Serialization;

namespace OpenSvip.Framework
{
    [Serializable]
    [XmlRoot("Plugin")]
    public class Plugin
    {
        public string Name { get; set; }
        public string Version { get; set; }
        public string Author { get; set; }
        public string HomePage { get; set; }
        public string Description { get; set; }
        public string Requirements { get; set; }
        public string Format { get; set; }
        public string Suffix { get; set; }
        public string Identifier { get; set; }
        public string LibraryPath { get; set; }
        public string Converter { get; set; }
        public Option[] Options { get; set; }
    }
}
