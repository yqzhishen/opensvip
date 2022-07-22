using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace FlutyDeer.MusicXml.Core
{
    [DesignerCategory("code")]
    [XmlType(AnonymousType = true)]
    [Serializable]
    public class MeasureAttributes
    {
        [XmlAttribute("divisions")]
        public ushort Divisions { get; set; }

        public AttributesTime Time { get; set; }
    }
}
