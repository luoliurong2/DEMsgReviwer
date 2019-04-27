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
		}

        /// <summary>
        /// offsets are manually committed.
        /// no extra thread is created for the Poll loop.
        /// </summary>
		public void Consume(string broker, string topic, string groupId, CancellationTokenSource cancelSource, Action<ConsumerResult> callbackAction = null)
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

                consumer.Subscribe(topic);

                Console.WriteLine($"Started consumer, Ctrl-C to stop consuming");

                while (!cancelSource.IsCancellationRequested)
                {
                    if (!consumer.Consume(out Message<Ignore, string> msg, TimeSpan.FromMilliseconds(100)))
                    {
                        continue;
                    }

                    if (msg != null)
                    {
                        ConsumerResult msgResult = new ConsumerResult();
                        msgResult.Broker = broker;
                        msgResult.Topic = msg.Topic;
                        msgResult.Partition = msg.Partition;
                        msgResult.Offset = msg.Offset;
                        msgResult.Message = msg.Value;

                        callbackAction?.Invoke(msgResult);

                        if (msg.Offset % 5 == 0)
                        {
                            var committedOffsets = consumer.CommitAsync(msg).Result;
                            Console.WriteLine($"Committed offset: {committedOffsets}");
                        }
                    }
                }
            }
        }

        public void ConsumeAsync(string broker, string topic, string groupId, CancellationTokenSource cancelSource, Action<ConsumerResult> action = null)
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

            ThreadPool.QueueUserWorkItem(KafkaAutoCommittedOffsets, new ConsumerSetting() { Broker = broker, Topic = topic, GroupID = groupId, CancelSource = cancelSource, Action = action });
        }

        private void KafkaAutoCommittedOffsets(object state)
        {
            ConsumerSetting setting = state as ConsumerSetting;

            var config = new Dictionary<string, object>
                {
                    { "bootstrap.servers", setting.Broker },
                    { "group.id", setting.GroupID },
                    { "enable.auto.commit", true },  // this is the default
                    { "auto.commit.interval.ms", 5000 },
                    { "statistics.interval.ms", 60000 },
                    { "session.timeout.ms", 6000 },
                    { "auto.offset.reset", "smallest" }
                };

            using (var consumer = new Consumer<Ignore, string>(config, null, new StringDeserializer(Encoding.UTF8)))
            {
                if (setting.Action != null)
                {
                    consumer.OnMessage += (_, message) =>
                    {
                        ConsumerResult messageResult = new ConsumerResult();
                        messageResult.Broker = setting.Broker;
                        messageResult.Topic = message.Topic;
                        messageResult.Partition = message.Partition;
                        messageResult.Offset = message.Offset.Value;
                        messageResult.Message = message.Value;

                        //执行外界自定义的方法
                        setting.Action(messageResult);
                    };
                }

                consumer.Subscribe(setting.Topic);

                while (!setting.CancelSource.IsCancellationRequested)
                {
                    consumer.Poll(TimeSpan.FromMilliseconds(100));
                }
            }
        }
    }

    public sealed class ConsumerSetting
    {
        /// <summary>
        /// Kafka消息服务器的地址
        /// </summary>
        public string Broker { get; set; }

        /// <summary>
        /// Kafka消息所属的主题
        /// </summary>
        public string Topic { get; set; }

        /// <summary>
        /// Kafka消息消费者分组主键
        /// </summary>
        public string GroupID { get; set; }

        public CancellationTokenSource CancelSource { get; set; }

        /// <summary>
        /// 消费消息后可以执行的方法
        /// </summary>
        public Action<ConsumerResult> Action { get; set; }
    }
}
