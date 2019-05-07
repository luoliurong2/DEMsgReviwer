using DEKafkaMessageGenerator.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using DEKafkaMessageViewer.Common;
using System.Threading;
using System.Xml;

namespace DEKafkaMessageGenerator
{
	class Program
	{
		static string SampleMessagePath = @"C:\Users\luoliurr\Source\Repos\DEMsgReviwer\DEKafkaMessageGenerator\Samples\Sample2\KfkMsgSample2.xml";
		static string KafkaBootstrapper = @"dataexchange2:9092";
		static string TopicName = "detest";
		static bool IsConsume = true;

		static void Main(string[] args)
		{
			Console.WriteLine($"Please specify Kafka broker boostrap server [{KafkaBootstrapper}]:");
			var kfkServer = Console.ReadLine();
			if (!string.IsNullOrEmpty(kfkServer))
			{
				KafkaBootstrapper = kfkServer;
			}

			Console.WriteLine("Do you want to produce message or consume message from kafka broker (P/[C])?");
			var oper = Console.ReadLine();
			if (!string.IsNullOrEmpty(oper) && oper != "C")
			{
				IsConsume = false;
			}

			if (IsConsume)
			{
				StartConsume();
			}
			else
			{
				StartProduce();
			}
		}

		static void StartProduce()
		{
			var messageContent = GetMessageContentFromFile();
			var messageKey = GetMessageKey();

			IKafkaProducer producer = new KafkaProducer();
			producer.Produce(KafkaBootstrapper, TopicName, messageContent, messageKey);
		}

		static string GetMessageKey()
		{
			XmlDocument doc = new XmlDocument();
			doc.Load(SampleMessagePath);

			var rootElement = doc.DocumentElement;
			if (rootElement.Name != "DEKafkaMessage")
			{
				throw new Exception($"Message in file {SampleMessagePath} is not valid.");
			}

			var targetElement = rootElement.ChildNodes[0].ChildNodes[0];
			if (targetElement == null)
			{
				throw new Exception($"Message in file {SampleMessagePath} is not valid.");
			}

			return targetElement.InnerText;
		}

		static string GetMessageContentFromFile()
		{
			return File.ReadAllText(SampleMessagePath);
		}

		static void StartConsume()
		{
			KafkaConsumer consumer = new KafkaConsumer();
			try
			{
				Console.WriteLine("Started consuming, press CTRL + C to break.");
				CancellationTokenSource cancelSource = new CancellationTokenSource();
				Console.CancelKeyPress += (_, e) =>
				{
					e.Cancel = true; // prevent the process from terminating.
					cancelSource.Cancel();
				};
				Guid groupId = Guid.NewGuid();
				consumer.ConsumeAsync(KafkaBootstrapper, TopicName, groupId.ToString(), cancelSource, (result) =>
				{
					Console.WriteLine(result.Topic);
					Console.WriteLine(result.Broker);
					Console.WriteLine(result.Partition);
					Console.WriteLine(result.Message);
				});
				Console.WriteLine("Done!");

				Console.ReadLine();
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.Message);
			}
		}
	}
}
