using FlutyDeer.MusicXml.Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;

namespace FlutyDeer.MusicXml.Stream
{
    public static class XmlConvertX
    {
        public static string SerializeObject(object o)
        {
            if (o is null)
            {
                throw new ArgumentNullException(nameof(o));
            }
			XmlSerializer serializer = new XmlSerializer(typeof(ScorePartWise));
			string result;
			using (StringWriter stringWriter = new StringWriter())
			{
				XmlWriterSettings settings = new XmlWriterSettings
				{
					Indent = true,
					OmitXmlDeclaration = true
				};
				using (XmlWriter writer = XmlWriter.Create(stringWriter, settings))
				{
					XmlSerializerNamespaces emptyNamespaces = new XmlSerializerNamespaces(new XmlQualifiedName[]
					{
						XmlQualifiedName.Empty
					});
					serializer.Serialize(writer, o, emptyNamespaces);
					result = stringWriter.ToString().Replace(" />", "/>");
				}
			}
			return result;
		}
    }
}
