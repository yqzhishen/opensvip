using System;
using System.Xml.Serialization;

namespace OpenSvip.Framework
{
    [Serializable]
    [XmlType("Option")]
    public class Option
    {
        [XmlAttribute]
        public string Name { get; set; }
        [XmlAttribute]
        public string Type { get; set; }
        [XmlAttribute]
        public string Default { get; set; }
        [XmlElement]
        public string Notes { get; set; }
        [XmlArray]
        public Choice[] EnumChoices { get; set; } = Array.Empty<Choice>();
    }
}
