using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DEKafkaMessageViewer.Common
{
	public sealed class ConsumerResult
	{
		public string Broker { get; set; }
		public string Topic { get; set; }
		public string GroupId { get; set; }
		public string Message { get; set; }
		public long Offset { get; set; }
		public int Partition { get; set; }
	}
}
