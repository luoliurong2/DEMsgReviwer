using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DEKafkaMessageGenerator.Messages
{
	/// <summary>
	/// The part of a Kafka Message we save in the tracking DB
	/// </summary>
	public class KafkaTrackedMsgDetail
	{
		public long BatchId { get; set; }

		public int ThreadId { get; set; }

		public long OffsetInBatch { get; set; }

		public KafkaTrackedMsgDetail()
		{
			BatchId = 0;
			ThreadId = 0;
			OffsetInBatch = 1;
		}

		public KafkaTrackedMsgDetail Clone()
		{
			return (KafkaTrackedMsgDetail)this.MemberwiseClone();
		}
	}
}
