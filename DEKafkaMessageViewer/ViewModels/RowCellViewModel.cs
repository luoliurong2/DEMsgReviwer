namespace DEKafkaMessageViewer.ViewModels
{
	public class RowCellViewModel : NotificationObject
	{
		private string value;

		public string Value
		{
			get { return value; }
			set { ChangeAndNotify(ref this.value, value, () => Value); }
		}
	}
}
