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
    [XmlRoot("score-partwise", Namespace = "", IsNullable = false)]
    [Serializable]
    public class ScorePartWise
    {
        [XmlAttribute("version")]
        public decimal Version { get; set; }

		[XmlElement("part-list")]
		public PartList PartList { get; set; }

        [XmlElement("part")]
        public Part Part { get; set; }
	}
}
