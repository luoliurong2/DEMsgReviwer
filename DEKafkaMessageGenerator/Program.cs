using DEKafkaMessageGenerator.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using DEKafkaMessageViewer.Common;
using System.Threading;

namespace DEKafkaMessageGenerator
{
	class Program
	{
		static KafkaTargetConfiguration kafkaConfig = new KafkaTargetConfiguration();
		static void Main(string[] args)
		{
			//var filePath = @"C:\Users\luoliurr\Source\Repos\DEMsgReviwer\DEKafkaMessageGenerator\Samples\Sample2\KfkMsgSample2.xml";
			//var message = File.ReadAllText(filePath);

			//KafkaProducer producer = new KafkaProducer();
			KafkaConsumer consumer = new KafkaConsumer();
			try
			{
				//producer.Produce("10.62.153.123:9092", "de3557", message);

				CancellationTokenSource cancelSource = new CancellationTokenSource();
				Console.CancelKeyPress += (_, e) =>
				{
					e.Cancel = true; // prevent the process from terminating.
					cancelSource.Cancel();
				};
				Guid groupId = Guid.NewGuid();
				consumer.ConsumeAsync("10.62.153.123:9092", "de3557", groupId.ToString(), cancelSource, (result) =>
				{
					Console.WriteLine(result.Topic);
					Console.WriteLine(result.Broker);
					Console.WriteLine(result.Partition);
					Console.WriteLine(result.Message);
				});

				Console.ReadLine();

                Console.WriteLine("Done!");

				Console.ReadLine();
			}
			catch(Exception ex)
			{
				Console.WriteLine(ex.Message);
			}
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
