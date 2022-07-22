using System;
using System.ComponentModel;
using System.Xml.Serialization;

namespace FlutyDeer.MusicXml.Core
{
    [DesignerCategory("code")]
    [XmlType(AnonymousType = true)]
    [Serializable]
    public class Part
    {
        [XmlAttribute("id")]
        public string Id { get; set; }

        [XmlElement("measure")]
        public Measure[] Measures { get; set; }
    }
}
