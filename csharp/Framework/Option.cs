using System;
using System.Xml.Serialization;

namespace OpenSvip.Framework
{
    [Serializable]
    [XmlType("Option")]
    public class Option
    {
        public string Name { get; set; }
        public string Type { get; set; }
        public string Default { get; set; }
    }
}
