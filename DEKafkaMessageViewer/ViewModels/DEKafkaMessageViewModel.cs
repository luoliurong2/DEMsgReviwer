using DEKafkaMessageViewer.Kafka;
using System.Linq;

namespace DEKafkaMessageViewer.ViewModels
{
	public class DEKafkaMessageViewModel
	{
		private string uniqueId;
		private string batchId;

		public DEKafkaMessageViewModel(string rawContent, object msgObj)
		{
			RawXml = rawContent;
			FormattedXml = KafkaMessageXMLFormatter.Fromat(RawXml);
			var headerProp = msgObj.GetType().GetProperty(DEKafkaMessageContract.Header).GetValue(msgObj);
			var headerProperties = headerProp.GetType().GetProperties(System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public);
			var msgUniqueIdProp = headerProperties.FirstOrDefault(p => p.Name == DEKafkaMessageContract.MessageUniqueId);
			var BatchIdProp = headerProperties.FirstOrDefault(p => p.Name == DEKafkaMessageContract.BatchId);
			uniqueId = msgUniqueIdProp.GetValue(headerProp).ToString();
			batchId = BatchIdProp.GetValue(headerProp).ToString();
		}

		public string RawXml { get; private set; }

		public string FormattedXml { get; private set; }

		public override string ToString()
		{
			return $"Unique ID: {uniqueId}, Batch ID: {batchId}";
		}
	}
}