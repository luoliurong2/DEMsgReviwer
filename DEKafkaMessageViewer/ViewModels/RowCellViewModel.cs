using Prism.Mvvm;

namespace DEKafkaMessageViewer.ViewModels
{
	public class RowCellViewModel : BindableBase
    {
		private string value;

		public string Value
		{
			get { return value; }
			set {
                SetProperty(ref this.value, value);
                RaisePropertyChanged("Value");
            }
		}
	}
}
