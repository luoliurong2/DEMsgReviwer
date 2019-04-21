using Confluent.Kafka;
using Confluent.Kafka.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DEKafkaMessageViewer.Kafka
{
	public class KafkaConsumer
	{
		private bool stopTriggered = false;
		private string bootstrapServers;
		private string topic;
		private string otherConfigs;
		private string consumerGroupName = $"DataExchange-Consumer-Group-{Guid.NewGuid().ToString()}";

		public event EventHandler<Error> ConsumeError;

		public KafkaConsumer(string bootstrapServers, string topic, string otherConfigs = null)
		{
			this.bootstrapServers = bootstrapServers;
			this.topic = topic;
			this.otherConfigs = otherConfigs;
		}

		public void Subscribe(EventHandler<Message<string, string>> onMessage)
		{
			StartConsume(onMessage);
		}

		public void UnSubscribe()
		{
			stopTriggered = true;
		}

		private void StartConsume(EventHandler<Message<string, string>> onMessage)
		{
			StringDeserializer deserializer = new StringDeserializer(Encoding.UTF8);
			var configs = GetConsumerConfig();
			using (var consumer = new Consumer<string, string>(configs, deserializer, deserializer))
			{
				var topicPartitions = GetTopicPartitions();
				consumer.Assign(topicPartitions);
				consumer.OnError += OnConsumerError;
				stopTriggered = false;

				Message<string, string> kafkaMsg = null;
				while (true)
				{
					if (consumer.Consume(out kafkaMsg, TimeSpan.FromSeconds(5)))
					{
						onMessage?.Invoke(this, kafkaMsg);
					}

					if (stopTriggered)
					{
						break;
					}
				}
			}
		}

		private void OnConsumerError(object sender, Error e)
		{
			ConsumeError?.Invoke(this, e);
		}

		private Dictionary<string, object> GetConsumerConfig()
		{
			var config = new Dictionary<string, object>()
			{
				{ "bootstrap.servers", bootstrapServers },
				{ "group.id", consumerGroupName},
			};
			if(!string.IsNullOrEmpty(otherConfigs))
			{
				try
				{
					var configItems = otherConfigs.Split(',');
					foreach (var item in configItems)
					{
						var pair = item.Trim().Split(':');
						config.Add(pair[0], pair[1]);
					}
				}
				catch
				{
					throw new InvalidCastException("Other Configs are invalid, please check the format.");
				}
				
			}
			return config;
		}

		private List<TopicPartitionOffset> GetTopicPartitions()
		{
			//TODO: Retrive last consumed offset...
			Dictionary<string, object> producerConfig = new Dictionary<string, object>()
			{
				{ "bootstrap.servers", bootstrapServers}
			};
			var serializer = new StringSerializer(Encoding.UTF8);
			using (var producer = new Producer<string, string>(producerConfig, serializer, serializer))
			{
				var metaData = producer.GetMetadata(false, topic);
				var topicMetadata = metaData.Topics.FirstOrDefault(t => t.Topic == topic);
				if(topicMetadata != null)
				{
					List<TopicPartitionOffset> result = new List<TopicPartitionOffset>();
					foreach (var partition in topicMetadata.Partitions)
					{
						result.Add(new TopicPartitionOffset(new TopicPartition(topic, partition.PartitionId), new Offset(0)));
					}
					return result;
				}
			}
			throw new Exception("Error: Read Kafka meta data failed.");
		}
	}
}
