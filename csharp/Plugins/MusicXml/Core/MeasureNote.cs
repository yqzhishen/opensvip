using System;
using System.ComponentModel;
using System.Xml.Serialization;

namespace FlutyDeer.MusicXml.Core
{
    [DesignerCategory("code")]
    [XmlType(AnonymousType = true)]
    [Serializable]
    public class MeasureNote
    {
        [XmlElement("rest")]
        public object Rest { get; set; }

        [XmlIgnore]
        public NoteGrace Grace { get; set; }

        public NotePitch Pitch { get; set; }

        [XmlElement("duration")]
        public ushort Duration { get; set; }

        [XmlElement("type")]
        public string Type { get; set; }

        [XmlIgnore]
        public NoteTie Tie { get; set; }

        [XmlAttribute("notations")]
        public NoteNotations NoteNotations  { get; set; }

        [XmlElement("lyric")]
        public NoteLyric Lyric { get; set; }
    }
}