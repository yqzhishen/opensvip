using System;
using System.ComponentModel;
using System.Xml.Serialization;

namespace FlutyDeer.MusicXml.Core
{
    [DesignerCategory("code")]
    [XmlType(AnonymousType = true)]
    [Serializable]
    public class NoteLyric
    {
        [XmlAttribute("number")]
        public byte Number { get; set; }

        [XmlElement("syllabic")]
        public string Syllabic { get; set; }

        [XmlElement("text")]
        public string Text { get; set; }
    }
}