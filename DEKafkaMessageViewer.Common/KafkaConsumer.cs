using Confluent.Kafka;
using Confluent.Kafka.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DEKafkaMessageViewer.Common
{
    /// <summary>
    /// reference: 
    /// https://github.com/confluentinc/confluent-kafka-dotnet/blob/0.11.6.x/examples/Consumer/Program.cs
    /// </summary>
    public sealed class KafkaConsumer : KafkaBase, IKafkaConsumer
	{
		public KafkaConsumer()
		{
			IsCancelled = false;
		}

		public bool IsCancelled { get; set; } = false;

        /// <summary>
        /// offsets are manually committed.
        /// no extra thread is created for the Poll loop.
        /// </summary>
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

            var config = new Dictionary<string, object>
            {
                { "bootstrap.servers", broker },
                { "group.id", groupId },
                { "enable.auto.commit", false },
                { "statistics.interval.ms", 60000 },
                { "session.timeout.ms", 6000 },
                { "auto.offset.reset", "smallest" }
            };

            using (var consumer = new Consumer<Ignore, string>(config, new IgnoreDeserializer(), new StringDeserializer(Encoding.UTF8)))
            {
                // Note: All event handlers are called on the main thread.

                consumer.OnPartitionEOF += (_, end)
                    => Console.WriteLine($"Reached end of topic {end.Topic} partition {end.Partition}, next message will be at offset {end.Offset}");

                consumer.OnError += (_, error)
                    => Console.WriteLine($"Error: {error}");

                // Raised on deserialization errors or when a consumed message has an error != NoError.
                consumer.OnConsumeError += (_, error)
                    => Console.WriteLine($"Consume error: {error}");

                // Raised when the consumer is assigned a new set of partitions.
                consumer.OnPartitionsAssigned += (_, partitions) =>
                {
                    Console.WriteLine($"Assigned partitions: [{string.Join(", ", partitions)}], member id: {consumer.MemberId}");
                    // If you don't add a handler to the OnPartitionsAssigned event,
                    // the below .Assign call happens automatically. If you do, you
                    // must call .Assign explicitly in order for the consumer to 
                    // start consuming messages.
                    consumer.Assign(partitions);
                };

                // Raised when the consumer's current assignment set has been revoked.
                consumer.OnPartitionsRevoked += (_, partitions) =>
                {
                    Console.WriteLine($"Revoked partitions: [{string.Join(", ", partitions)}]");
                    // If you don't add a handler to the OnPartitionsRevoked event,
                    // the below .Unassign call happens automatically. If you do, 
                    // you must call .Unassign explicitly in order for the consumer
                    // to stop consuming messages from it's previously assigned 
                    // partitions.
                    consumer.Unassign();
                };

                consumer.OnStatistics += (_, json)
                    => Console.WriteLine($"Statistics: {json}");

                var startPartitions = GetTopicPartitions(broker, topic);
                consumer.Assign(startPartitions);

                consumer.Subscribe(topic);

                Console.WriteLine($"Started consumer, Ctrl-C to stop consuming");

                var cancelled = false;
                Console.CancelKeyPress += (_, e) => {
                    e.Cancel = true; // prevent the process from terminating.
                    cancelled = true;
                };

                while (!cancelled)
                {
                    if (!consumer.Consume(out Message<Ignore, string> msg, TimeSpan.FromMilliseconds(100)))
                    {
                        if (msg != null)
                        {
                            ConsumerResult msgResult = new ConsumerResult();
                            msgResult.Broker = broker;
                            msgResult.Topic = msg.Topic;
                            msgResult.Partition = msg.Partition;
                            msgResult.Offset = msg.Offset;
                            msgResult.Message = msg.Value;

                            action?.Invoke(msgResult);
                        }
                        continue;
                    }

                    Console.WriteLine($"Topic: {msg.Topic} Partition: {msg.Partition} Offset: {msg.Offset} {msg.Value}");

                    if (msg.Offset % 5 == 0)
                    {
                        var committedOffsets = consumer.CommitAsync(msg).Result;
                        Console.WriteLine($"Committed offset: {committedOffsets}");
                    }
                }
            }
        }

        private List<TopicPartitionOffset> GetTopicPartitions(string bootstrapServers, string topic)
        {
            Dictionary<string, object> producerConfig = new Dictionary<string, object>()
            {
                { "bootstrap.servers", bootstrapServers}
            };
            var serializer = new StringSerializer(Encoding.UTF8);
            using (var producer = new Producer<Null, string>(producerConfig, null, serializer))
            {
                var metaData = producer.GetMetadata(false, topic);
                var topicMetadata = metaData.Topics.FirstOrDefault(t => t.Topic == topic);
                if (topicMetadata != null)
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
