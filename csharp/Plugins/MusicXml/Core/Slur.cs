using System;
using System.ComponentModel;
using System.Xml.Serialization;

namespace FlutyDeer.MusicXml.Core
{
    [DesignerCategory("code")]
    [XmlType(AnonymousType = true)]
    [Serializable]
    public class Slur
    {
        [XmlAttribute("type")]
        public string Type { get; set; }
    }
}