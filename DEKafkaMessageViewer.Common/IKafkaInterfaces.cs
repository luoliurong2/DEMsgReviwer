using System;
using System.Threading;

namespace DEKafkaMessageViewer.Common
{
	public interface IKafkaConsumer
	{
        void Consume(string broker, string topic, string groupId, CancellationTokenSource cancelSource, Action<ConsumerResult> action = null);
        void ConsumeAsync(string broker, string topic, string groupId, CancellationTokenSource cancelSource, Action<ConsumerResult> action = null);
	}

	public interface IKafkaProducer
	{
		bool Produce(string broker, string topic, string message, string messageKey);
		bool Produce(string broker, string topic, string message);
	}
}
