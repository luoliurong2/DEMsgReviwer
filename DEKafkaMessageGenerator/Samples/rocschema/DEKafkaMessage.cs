using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace DE.Kafka
{
	public class DEKafkaMessage
	{
		public DEKafkaMessageHeader Header { get; set;}

		public List<BatchUnit> BatchUnits { get; set; }

		public class DEKafkaMessageHeader
		{
			public long BatchId { get; set; }

			public int ThreadId { get; set; }

			public string MessageType { get; set; }

			public string MessageUniqueId { get; set; }

			public string DETransmissionType { get; set; }

			public Guid TransformationId { get; set; }

			public string ProviderId { get; set; }

			public string MessageFormatVersion { get; set; }
		}


		public class BatchUnit
		{
			public string SourceTransactionId { get; set; }

			public List<MessageUnit> MessageUnits { get; set; }


			public class MessageUnit
			{
				public string SourceOperation { get; set; }

				public string TargetOperation { get; set; }

				public int OrderInBatchUnit { get; set; }

				public string SchemaName { get; set; }

				public string ClassifierName { get; set; }

				[XmlArray("KeysOfTarget", IsNullable = true), XmlArrayItem("KeyOfTarget", typeof(string), IsNullable = true)]
				public List<string> KeysOfTarget { get; set; }

				public DateTime SchemaLastModifiedDate { get; set; }

				public string Schema { get; set; }

				[XmlElement(ElementName = "MessagePayload")]
				public MessagePayload Payload { get; set; }

				public class MessagePayload
				{
					public string BeforeImage { get; set; }

					public string AfterImage { get; set; }
				}
			}
		}
	}
}