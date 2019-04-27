using DEKafkaMessageViewer.Kafka;
using Prism.Mvvm;
using System.Linq;

namespace DEKafkaMessageViewer.ViewModels
{
	public class DEKafkaMessageViewModel:BindableBase
	{
		private string uniqueId;
		private string batchId;
        private string _rawXml;
        private string _formatedXml;

		public DEKafkaMessageViewModel(string rawContent, object msgObj)
		{
            _rawXml = rawContent;
            _formatedXml = KafkaMessageXMLFormatter.Fromat(RawXml);
			var headerProp = msgObj.GetType().GetProperty(DEKafkaMessageContract.Header).GetValue(msgObj);
			var headerProperties = headerProp.GetType().GetProperties(System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public);
			var msgUniqueIdProp = headerProperties.FirstOrDefault(p => p.Name == DEKafkaMessageContract.MessageUniqueId);
			var BatchIdProp = headerProperties.FirstOrDefault(p => p.Name == DEKafkaMessageContract.BatchId);
			uniqueId = msgUniqueIdProp.GetValue(headerProp).ToString();
			batchId = BatchIdProp.GetValue(headerProp).ToString();
		}

		public string RawXml {
            get { return _rawXml; }
            set { SetProperty(ref _rawXml, value); }
        }

		public string FormattedXml
        {
            get { return _formatedXml; }
            set { SetProperty(ref _formatedXml, value); }
        }

		public override string ToString()
		{
			return $"Unique ID: {uniqueId}, Batch ID: {batchId}";
		}
	}
}