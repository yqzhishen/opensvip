using System;
using System.Xml.Serialization;

namespace OpenSvip.Framework
{
    [Serializable]
    [XmlType("Choice")]
    public class Choice
    {
        [XmlAttribute]
        public string Name { get; set; }
        [XmlAttribute]
        public string Tag { get; set; }
    }
}
