using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Data;

namespace DEKafkaMessageViewer.ViewModels
{
	public class DataGridViewModel
	{
		public DataGridViewModel()
		{
			Columns = new ObservableCollection<TableColumnViewModel>();
			Rows = new ObservableCollection<TableRowViewModel>();
		}

		public ObservableCollection<TableColumnViewModel> Columns { get; set; }

		public ObservableCollection<TableRowViewModel> Rows { get; private set; }

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
