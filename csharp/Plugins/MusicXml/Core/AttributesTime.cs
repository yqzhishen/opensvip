using System;
using System.ComponentModel;
using System.Xml.Serialization;

namespace FlutyDeer.MusicXml.Core
{
    [DesignerCategory("code")]
    [XmlType(AnonymousType = true)]
    [Serializable]
    public class AttributesTime
    {
        [XmlElement("beats")]
        public byte Beats { get; set; }

        [XmlElement("beat-type")]
        public byte BeatType { get; set; }
    }
}
