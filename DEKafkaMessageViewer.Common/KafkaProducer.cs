using Confluent.Kafka;
using Confluent.Kafka.Serialization;
using System;
using System.Collections.Generic;
using System.Text;

namespace DEKafkaMessageViewer.Common
{
	public sealed class KafkaProducer : KafkaBase, IKafkaProducer
	{
		public bool Produce(string broker, string topic, string message, string messageKey = "")
		{
			return ProduceMessage(broker, topic, message, messageKey);
		}
		public bool Produce(string broker, string topic, string message)
		{
			return ProduceMessage(broker, topic, message, string.Empty);
		}

		private bool ProduceMessage(string broker, string topic, string message, string messageKey)
		{
			if (string.IsNullOrEmpty(broker) || string.IsNullOrWhiteSpace(broker) || broker.Length <= 0)
			{
				throw new ArgumentNullException("broker argument for producer can't be null.");
			}
			if (string.IsNullOrWhiteSpace(topic) || string.IsNullOrEmpty(topic) || topic.Length <= 0)
			{
				throw new ArgumentNullException("topic argument for producer can't be null.");
			}
			if (string.IsNullOrWhiteSpace(message) || string.IsNullOrEmpty(message) || message.Length <= 0)
			{
				throw new ArgumentNullException("message argument for producer can't be null.");
			}

			var result = false;
			var config = new Dictionary<string, object> { { "bootstrap.servers", broker } };
			var serializer = new StringSerializer(Encoding.UTF8);
			if (string.IsNullOrEmpty(messageKey) || string.IsNullOrWhiteSpace(messageKey))
			{
				using(var producer = new Producer<Null, string>(config, null, serializer))
				{
					var deliveryReport = producer.ProduceAsync(topic, null, message);
					//if (deliveryReport.Error.Code == ErrorCode.NoError)
					//{
					//	Console.WriteLine("Producer：" + producer.Name + "\r\nTopic：" + topic + "\r\nPartition：" + deliveryReport.Partition + "\r\nOffset：" + deliveryReport.Offset + "\r\nMessage：" + deliveryReport.Value);
					//}
					deliveryReport.ContinueWith(task =>
					{
						if (task.Result.Error.Code == ErrorCode.NoError)
						{
							result = true;
							Console.WriteLine("Producer：" + producer.Name + "\r\nTopic：" + topic + "\r\nPartition：" + task.Result.Partition + "\r\nOffset：" + task.Result.Offset + "\r\nMessage：" + task.Result.Value);
						}
					});
					producer.Flush(TimeSpan.FromSeconds(500));
				}
			}
			else
			{
				using (var producer = new Producer<string, string>(config, serializer, serializer))
				{
					var deliveryReport = producer.ProduceAsync(topic, messageKey, message);
					deliveryReport.ContinueWith(task =>
					{
						if (task.Result.Error.Code == ErrorCode.NoError)
						{
							result = true;
						}
					});

					producer.Flush(TimeSpan.FromSeconds(500));
				}
			}

			return result;
		}
	}
}
