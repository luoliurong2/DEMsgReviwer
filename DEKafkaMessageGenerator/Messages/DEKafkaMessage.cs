using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;

namespace DEKafkaMessageGenerator.Messages
{
	[Serializable]
	[XmlRoot("DEKafkaMessage")]
	public sealed class KafkaMessage
	{
		private static readonly XmlSerializer kafkaMsgSerializer;
		private static readonly XmlWriterSettings xmlWriterSetting;
		private static readonly XmlSerializerNamespaces emptyNamespace;// Ensure there's no namespace information in the message

		static KafkaMessage()
		{
			kafkaMsgSerializer = new XmlSerializer(typeof(KafkaMessage));
			emptyNamespace = new XmlSerializerNamespaces();
			emptyNamespace.Add(string.Empty, string.Empty);
			xmlWriterSetting = new XmlWriterSettings();
			xmlWriterSetting.OmitXmlDeclaration = false;
			xmlWriterSetting.ConformanceLevel = ConformanceLevel.Fragment;
			xmlWriterSetting.Encoding = Encoding.UTF8;
		}

		public KafkaMessage()
		{
			BatchUnits = new List<KafkaBatchUnit>();
			Header = new KafkaMessageHeader();
		}

		[XmlElement("Header", Order = 1)]
		public KafkaMessageHeader Header { get; set; }

		[XmlArray("BatchUnits", Order = 2), XmlArrayItem("BatchUnit", typeof(KafkaBatchUnit))]
		public List<KafkaBatchUnit> BatchUnits { get; set; }

		public string ToXml()
		{
			StringBuilder sb = new StringBuilder();
			using (var writer = XmlWriter.Create(sb, xmlWriterSetting))
			{
				writer.WriteWhitespace("");
				kafkaMsgSerializer.Serialize(writer, this, emptyNamespace);
				writer.Close();
			}
			return sb.ToString();
		}
	}
}
