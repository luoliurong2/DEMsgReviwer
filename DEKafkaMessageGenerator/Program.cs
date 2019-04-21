using DEKafkaMessageGenerator.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DEKafkaMessageGenerator
{
	class Program
	{
		static KafkaTargetConfiguration kafkaConfig = new KafkaTargetConfiguration();
		static void Main(string[] args)
		{
			Console.WriteLine("How many messages do you want to generate? Input a number and then press <ENTER> to continue...");
			var messageCount = 0;
			if (int.TryParse(Console.ReadLine(), out messageCount))
			{
				return;
			}

			InitializeKafkaConfig();

			KafkaMessageConstructor construnctor = new KafkaMessageConstructor();
			var kfkMsg = construnctor.Construct(1);
		}

		private static bool SendMessage(string message, string messageKey)
		{
			var kafkaMessenger = new KafkaMessenger(kafkaConfig);
			var reply = kafkaMessenger.SendMessage(kafkaConfig.Topic, message, messageKey);
			if (reply.Error.HasError)
			{
				Console.WriteLine(reply.Error.Reason);
				return false;
			}
			return true;
		}


		private static void InitializeKafkaConfig()
		{
			kafkaConfig.Topic = "detest";
			kafkaConfig.PayloadFormat = "xml";
			kafkaConfig.TransmissionType = KafkaMessageTransmissionType.Batch;
			kafkaConfig.BootstrapServers = "localhost:9092";
			kafkaConfig.ClientId = "DEMsgReviewer";
			kafkaConfig.Acks = 1;
			kafkaConfig.TransformationID = Guid.NewGuid();
		}
	}

	public sealed class KafkaTargetConfiguration
	{
		//[DataMember]
		public string Topic { get; set; }

		//[DataMember]
		public KafkaMessageTransmissionType TransmissionType { get; set; }

		//[DataMember]
		public string PayloadFormat { get; set; }

		//[DataMember]
		public Dictionary<string, string> ProducerConfig { get; set; }

		//[DataMember]
		public string BootstrapServers { get; set; }

		//[DataMember]
		public string ClientId { get; set; }

		//[DataMember]
		public short Acks { get; set; }

		//[DataMember]
		public Dictionary<string, string> Files { get; set; }

		//[DataMember]
		public Guid TransformationID { get; set; }
	}
}
