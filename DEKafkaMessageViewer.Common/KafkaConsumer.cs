using Confluent.Kafka;
using Confluent.Kafka.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DEKafkaMessageViewer.Common
{
	/// <summary>
	/// reference: https://github.com/confluentinc/confluent-kafka-dotnet/tree/master/examples
	/// </summary>
	public sealed class KafkaConsumer : KafkaBase, IKafkaConsumer
	{
		public KafkaConsumer()
		{
			IsCancelled = false;
		}

		public bool IsCancelled { get; set; } = false;

		public void Consume(string broker, string topic, string groupId, Action<ConsumerResult> action = null)
		{
			if (string.IsNullOrEmpty(broker) || string.IsNullOrWhiteSpace(broker) || broker.Length <= 0)
			{
				throw new ArgumentNullException("broker argument for consumer can't be null.");
			}
			if (string.IsNullOrWhiteSpace(topic) || string.IsNullOrEmpty(topic) || topic.Length <= 0)
			{
				throw new ArgumentNullException("topic argument for consumer can't be null.");
			}
			if (string.IsNullOrWhiteSpace(groupId) || string.IsNullOrEmpty(groupId) || groupId.Length <= 0)
			{
				throw new ArgumentNullException("message argument for consumer can't be null.");
			}

			var config = new Dictionary<string, object> {
				{"bootstrap.servers", broker },
				{"group.id", groupId },
				{"enable.auto.commit", true },
				{"auto.commit.interval.ms", 5000 },
				{"statistics.interval.ms", 5000 },
				{"session.timeout.ms", 3000 },
				{"auto.offset.reset", "smallest" }
			};

			using (var consumer = new Consumer<string, string>(config, new StringDeserializer(Encoding.UTF8), new StringDeserializer(Encoding.UTF8)))
			{
				consumer.OnMessage += (_, message) =>
				{
					ConsumerResult msgResult = new ConsumerResult();
					msgResult.Broker = broker;
					msgResult.Topic = message.Topic;
					msgResult.Partition = message.Partition;
					msgResult.Offset = message.Offset;
					msgResult.Message = message.Value;

					action?.Invoke(msgResult);
				};

				consumer.Subscribe(topic);

				while (!IsCancelled)
				{
					consumer.Poll(TimeSpan.FromMilliseconds(1000));
				}
			}
		}
	}
}
