using Confluent.Kafka;
using Confluent.Kafka.Serialization;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DEKafkaMessageGenerator.Messages
{
	public interface IKafkaMessenger : IDisposable
	{
		Message<string, string> SendMessage(string topic, string message, string messageKey);
	}

	internal sealed class KafkaMessenger : IKafkaMessenger
	{
		private string brokers;
		private Producer<string, string> producer;
		private string clientId;
		private short acks = -1;
		private const string BootstrapServersConfigName = "bootstrap.servers";
		private const string ClientIdConfigName = "client.id";
		private const string AckConfigName = "acks";
		private const string DefaultClientId = "DataExchange";
		private const string DefaultCompression = "gzip";
		private const string DefaultSocketBlockTimeoutMs = "1";
		private const string CodecConfigName = "compression.codec";
		private const string SocketBlockTimeoutConfigName = "socket.blocking.max.ms";

		private string certificatePath = AppDomain.CurrentDomain.BaseDirectory + @"Data\Certificates\";

		public KafkaMessenger(KafkaTargetConfiguration targetInfo)
		{
			brokers = targetInfo.BootstrapServers;
			clientId = string.IsNullOrEmpty(targetInfo.ClientId) ? DefaultClientId : targetInfo.ClientId;
			acks = targetInfo.Acks;
			var keySerializer = new StringSerializer(Encoding.UTF8);
			var valueSerializer = new StringSerializer(Encoding.UTF8);
			var config = GetProducerConfig(targetInfo.ProducerConfig);
			//certificatePath = Path.Combine(certificatePath, targetInfo.TransformationID.ToString(), targetInfo.SchemaID.ToString());
			//SaveFiles(targetInfo.Files);
			producer = new Producer<string, string>(config, keySerializer, valueSerializer);
			ProducerConfig = config;
		}

		public IEnumerable<KeyValuePair<string, object>> ProducerConfig { get; private set; }

		private void SaveFiles(Dictionary<string, string> files)
		{
			if (!Directory.Exists(certificatePath))
			{
				Directory.CreateDirectory(certificatePath);
			}

			if (files != null)
			{
				foreach (var entry in files)
				{
					var filePath = Path.Combine(certificatePath, entry.Key); ;
					var fileContent = entry.Value;
					if (!string.IsNullOrEmpty(fileContent))
					{
						using (var fs = new FileStream(filePath, FileMode.Create))
						using (StreamWriter sw = new StreamWriter(fs))
						{
							sw.Write(fileContent);
						}
					}
				}
			}
		}

		private Dictionary<string, object> GetProducerConfig(Dictionary<string, string> userInputs)
		{
			Dictionary<string, object> config = new Dictionary<string, object>();
			foreach (var input in userInputs)
			{
				config.Add(input.Key, input.Value.ToString());
			}
			if (!config.ContainsKey(BootstrapServersConfigName))
			{
				config.Add(BootstrapServersConfigName, brokers);
			}
			if (!config.ContainsKey(ClientIdConfigName))
			{
				config.Add(ClientIdConfigName, clientId);
			}
			if (!config.ContainsKey(AckConfigName))
			{
				config.Add(AckConfigName, acks);
			}
			if (!config.ContainsKey(CodecConfigName))
			{
				config.Add(CodecConfigName, DefaultCompression);
			}
			if (!config.ContainsKey(SocketBlockTimeoutConfigName))
			{
				config.Add(SocketBlockTimeoutConfigName, DefaultSocketBlockTimeoutMs);
			}
			return config;
		}

		public Message<string, string> SendMessage(string topic, string message, string messageKey)
		{
			try
			{
				return producer.ProduceAsync(topic, messageKey, message).Result;
			}
			catch (KafkaException e)
			{
				return new Message<string, string>(topic, 0, 0, messageKey, message, new Timestamp(), e.Error);
			}
		}

		public void Dispose()
		{
			if (producer != null)
			{
				producer.Dispose();
				producer = null;
			}
		}
	}
}
