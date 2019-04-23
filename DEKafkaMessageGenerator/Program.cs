﻿using DEKafkaMessageGenerator.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using DEKafkaMessageViewer.Common;

namespace DEKafkaMessageGenerator
{
	class Program
	{
		static KafkaTargetConfiguration kafkaConfig = new KafkaTargetConfiguration();
		static void Main(string[] args)
		{
			var filePath = @"C:\Projects\DataExchange\DEKafkaMessageViewer\DEKafkaMessageGenerator\Samples\KfkMsgSample.xml";
			var message = File.ReadAllText(filePath);

			KafkaProducer producer = new KafkaProducer();
			KafkaConsumer consumer = new KafkaConsumer();
			try
			{
				//producer.Produce("192.168.0.151:9092", "dataexchange", message);

				consumer.Consume("192.168.0.151:9092", "dataexchange", "kfkMsgReviewer", (result)=> {
					Console.WriteLine(result.Topic);
					Console.WriteLine(result.Broker);
					Console.WriteLine(result.Partition);
					Console.WriteLine(result.Message);
				});
				Console.ReadLine();
				consumer.IsCancelled = true;
				Console.ReadLine();
				Console.WriteLine("Done!");
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
