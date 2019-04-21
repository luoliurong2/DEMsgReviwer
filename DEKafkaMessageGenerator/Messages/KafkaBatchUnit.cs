using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace DEKafkaMessageGenerator.Messages
{
	[Serializable]
	[XmlRoot("BatchUnit")]
	public sealed class KafkaBatchUnit
	{
		public KafkaBatchUnit()
		{
		}

		public KafkaBatchUnit(string sourceTransactionId, IEnumerable<KafkaMessageUnit> messageUnits)
		{
			SourceTransactionId = sourceTransactionId;
			MessagesUnits = messageUnits.ToList();
		}

		[XmlElement("SourceTransactionId", Order = 1)]
		public string SourceTransactionId { get; set; }

		[XmlArray("MessageUnits", Order = 2), XmlArrayItem("MessageUnit", typeof(KafkaMessageUnit))]
		public List<KafkaMessageUnit> MessagesUnits { get; set; }
	}
}
