using System;
using System.ComponentModel;
using System.Xml.Serialization;

namespace FlutyDeer.MusicXml.Core
{
    [DesignerCategory("code")]
    [XmlType(AnonymousType = true)]
    [Serializable]
    public class DirectionSound
    {
        [XmlAttribute("tempo")]
        public decimal Tempo { get; set; }
    }
}