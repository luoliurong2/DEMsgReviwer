using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace DEKafkaMessageGenerator.Messages
{
	public enum KafkaMessageType
	{
		BeginBatch,
		BatchContent,
		EndBatch,
		WholeBatch,
	}

	public enum KafkaMessageTransmissionType
	{
		Batch,
		Transaction,
		AuditRecord,
	}

	[Serializable]
	public sealed class KafkaMessageHeader
	{
		public KafkaMessageHeader()
		{
			TrackedDetails = new KafkaTrackedMsgDetail();
			TransmissionType = KafkaMessageTransmissionType.Batch;
			TransformationId = string.Empty;
			ProviderId = string.Empty;
			FormatVersion = string.Empty;
		}

		[XmlIgnore]
		public KafkaTrackedMsgDetail TrackedDetails { get; set; }

		[XmlElement("BatchId", Order = 1)]
		public long BatchId
		{ get { return TrackedDetails.BatchId; } set { TrackedDetails.BatchId = value; } }

		[XmlElement("ThreadId", Order = 2)]
		public int ThreadId
		{
			get { return TrackedDetails.ThreadId; }
			set { TrackedDetails.ThreadId = value; }
		}

		[XmlElement("MessageUniqueId", Order = 3)]
		public string UniqueId
		{
			get
			{
				return string.Format($"{BatchId}-{ThreadId}-{OffsetInBatch}");
			}
			set { }
		}

		[XmlElement("MessageType", Order = 4)]
		public KafkaMessageType Type { get; set; }

		[XmlElement("DETransmissionType", Order = 5)]
		public KafkaMessageTransmissionType TransmissionType { get; set; }

		[XmlElement("TransformationId", Order = 6)]
		public string TransformationId { get; set; }


		[XmlElement("ProviderId", Order = 7)]
		public string ProviderId { get; set; }

		[XmlElement("MessageFormatVersion", Order = 8)]
		public string FormatVersion { get; set; }

		[XmlIgnore]
		public long OffsetInBatch
		{
			get { return TrackedDetails.OffsetInBatch; }
			set { TrackedDetails.OffsetInBatch = value; }
		}


		public KafkaMessageHeader Clone()
		{
			return (KafkaMessageHeader)this.MemberwiseClone();
		}
	}
}
