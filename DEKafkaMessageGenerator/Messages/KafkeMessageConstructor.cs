using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DEKafkaMessageGenerator.Messages
{
	internal interface IKafkaMessageConstructor
	{
		KafkaMessage Construct(int batchId);
	}

	internal class KafkaMessageConstructor : IKafkaMessageConstructor
	{
		#region Private members

		private KafkaMessageTransmissionType kafkaTransmissionType;

		#endregion

		#region Constructor

		public KafkaMessageConstructor()
		{
			kafkaTransmissionType = KafkaMessageTransmissionType.Batch;
		}

		#endregion

		public KafkaMessage Construct(int batchId)
		{
			KafkaMessage kafkaMessage = CreateKafkaMessage(batchId);
			return kafkaMessage;
		}

		#region private methods

		private KafkaMessage CreateKafkaMessage(int batchId)
		{
			KafkaMessage message = new KafkaMessage();
			ConstructMessageHeader(message,
									KafkaMessageType.WholeBatch,
									batchId,
									0,
									0L,
									Guid.NewGuid().ToString());
			ConstructMessageBatchUnits(message);
			return message;
		}

		private void ConstructMessageHeader(KafkaMessage message, KafkaMessageType type, long batchId, int threadId, long offset, string transformationId)
		{
			message.Header.Type = type;
			message.Header.BatchId = batchId;
			message.Header.ThreadId = threadId;
			message.Header.OffsetInBatch = offset;
			message.Header.TransformationId = transformationId;
			message.Header.ProviderId = "DEMsgReviewer";
			message.Header.TransmissionType = kafkaTransmissionType;
			message.Header.FormatVersion = "DE6.0";//ModelVersion.KafkaMessageFormatVersion;
		}

		private void ConstructMessageBatchUnits(KafkaMessage message)
		{
			var transactionId = "";
			var msgUnits = new List<KafkaMessageUnit>();
			//TODO:
			message.BatchUnits.Add(new KafkaBatchUnit(transactionId, msgUnits));
		}

		#endregion private methods
	}
}
