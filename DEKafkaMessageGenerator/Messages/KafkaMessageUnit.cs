using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace DEKafkaMessageGenerator.Messages
{
	public enum OperationEnum
	{
		Insert,
		Update,
		Delete,
		Merge
	}

	public class DbCommandWrapper : IDisposable
	{
		[XmlIgnore]
		public IDbCommand Command { get; set; }

		[XmlIgnore]
		public int RecordCount { get; set; } = 1;

		[XmlIgnore]
		public bool IsNonDurable { get; set; }

		[XmlIgnore]
		public bool CanBeCombined = true;

		[XmlIgnore]
		public string ClassifierName { get; set; }

		[XmlIgnore]
		public Dictionary<string, IDbCommand> RowCountParamAndCommand { get; set; }

		[XmlIgnore]
		public OperationEnum Operation { get; set; }

		private bool disposed;

		protected virtual void Dispose(bool disposing)
		{
			if (!disposed)
			{
				if (disposing)
				{
					if (Command != null)
					{
						Command.Dispose();
						Command = null;
					}
				}

				disposed = true;
			}
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}
	}

	[XmlRoot("MessageUnit")]
	public class KafkaMessageUnit : DbCommandWrapper
	{
		public KafkaMessageUnit()
		{
			KeysOfTarget = new List<string>();
			MessagePayload = new MessagePayload();
			Schema = string.Empty;
		}

		[XmlIgnore]
		public string SourceTransactionId { get; set; }

		[XmlIgnore]
		public bool IsLongTransaction { get; set; }

		public string SourceOperation { get; set; }
		public string TargetOperation { get; set; }
		public int OrderInBatchUnit { get; set; }
		public string SchemaName { get; set; }
		public new string ClassifierName { get; set; }

		[XmlArray("KeysOfTarget", IsNullable = true), XmlArrayItem("KeyOfTarget", typeof(string), IsNullable = true)]
		public List<string> KeysOfTarget { get; set; }
		public DateTime? SchemaLastModifiedDate { get; set; }
		public string Schema { get; set; }
		public MessagePayload MessagePayload { get; set; }
	}

	public class MessagePayload
	{
		public string BeforeImage { get; set; }
		public string AfterImage { get; set; }

		public string FormatToString()
		{
			return string.Format("Before Image: {0}  After Image: {1}", BeforeImage, AfterImage);
		}
	}
}
