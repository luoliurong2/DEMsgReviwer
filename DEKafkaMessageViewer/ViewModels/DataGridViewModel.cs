using Prism.Mvvm;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Data;

namespace DEKafkaMessageViewer.ViewModels
{
	public class DataGridViewModel:BindableBase
	{
        private ObservableCollection<TableColumnViewModel> _columns = new ObservableCollection<TableColumnViewModel>();
        private ObservableCollection<TableRowViewModel> _rows = new ObservableCollection<TableRowViewModel>();

        public DataGridViewModel()
		{
		}

		public ObservableCollection<TableColumnViewModel> Columns {
            get { return _columns; }
            set { SetProperty(ref _columns, value); RaisePropertyChanged("Columns"); }
        }

		public ObservableCollection<TableRowViewModel> Rows {
            get { return _rows; }
            set { SetProperty(ref _rows, value); RaisePropertyChanged("Rows"); }
        }

		internal void ShowAllRows()
		{
			CollectionViewSource.GetDefaultView(Rows).Filter = null;
			foreach (var column in Columns)
			{
				column.Show();
			}
		}

		internal void FilterRows(IEnumerable<CompareCondition> conditions, IEnumerable<string> selectionColumns)
		{
			foreach (var column in Columns)
			{
				if (selectionColumns == null || selectionColumns.Any(c => c.ToLower() == column.Header.ToLower()))
				{
					column.Show();
				}
				else
				{
					column.Hide();
				}
			}

			CollectionViewSource.GetDefaultView(Rows).Filter = obj =>
			{
				TableRowViewModel row = obj as TableRowViewModel;
				return row.Match(conditions);
			};
		}
	}
}
