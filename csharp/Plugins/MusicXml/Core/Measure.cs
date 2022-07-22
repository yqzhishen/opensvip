using System;
using System.ComponentModel;
using System.Xml.Serialization;

namespace FlutyDeer.MusicXml.Core
{
    [DesignerCategory("code")]
    [XmlType(AnonymousType = true)]
    [Serializable]
    public class Measure
    {
        [XmlAttribute("number")]
        public ushort Number { get; set; }

        [XmlElement("attributes", typeof(MeasureAttributes))]
        [XmlElement("direction", typeof(MeasureDirection))]
        [XmlElement("note", typeof(MeasureNote))]
        public object[] Items { get; set; }
    }
}
