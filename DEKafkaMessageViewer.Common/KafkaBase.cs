using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DEKafkaMessageViewer.Common
{
	public abstract class KafkaBase
	{
		public string GetKafkaBroker(string brokerNameKey = "Broker")
		{
			string kafkaBroker = string.Empty;
			if (!ConfigurationManager.AppSettings.AllKeys.Contains(brokerNameKey))
			{
				kafkaBroker = "http://localhost:9092";
			}
			else
			{
				kafkaBroker = ConfigurationManager.AppSettings[brokerNameKey];
			}

			return kafkaBroker;
		}

		public string GetTopicName(string topicNameKey = "Topic")
		{
			string topicName = string.Empty;
			if (!ConfigurationManager.AppSettings.AllKeys.Contains(topicNameKey))
			{
				throw new Exception("Key \"" + topicNameKey + "\" not found in Config file -> configuration/AppSettings");
			}
			else
			{
				topicName = ConfigurationManager.AppSettings[topicNameKey];
			}

			return topicName;
		}
	}
}
