using System;
using System.ComponentModel;
using System.Xml.Serialization;

namespace FlutyDeer.MusicXml.Core
{
    [DesignerCategory("code")]
    [XmlType(AnonymousType = true)]
    [Serializable]
    public class NotePitch
    {
        [XmlElement("step")]
        public string Step { get; set; }

        [XmlElement("alter")]
        public sbyte Alter { get; set; }

        [XmlElement("octave")]
        public byte Octave { get; set; }
    }
}