using DEKafkaMessageViewer.ViewModels;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace DEKafkaMessageViewer.Extensions
{
    public class DataGridColumnsBinder
    {
        public static ObservableCollection<TableColumnViewModel> GetColumns(DependencyObject obj)
        {
            return (ObservableCollection<TableColumnViewModel>)obj.GetValue(ColumnsProperty);
        }

        public static void SetColumns(DependencyObject obj, ObservableCollection<TableColumnViewModel> value)
        {
            obj.SetValue(ColumnsProperty, value);
        }

        // Using a DependencyProperty as the backing store for Columns.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ColumnsProperty =
            DependencyProperty.RegisterAttached("Columns",
                typeof(ObservableCollection<TableColumnViewModel>),
                typeof(DataGridColumnsBinder),
                new PropertyMetadata(null, new PropertyChangedCallback((obj, args) =>
                {
                    DataGrid grid = obj as DataGrid;
                    if (grid != null)
                    {
                        ObservableCollection<TableColumnViewModel> columns = args.NewValue as ObservableCollection<TableColumnViewModel>;
                        if (columns != null)
                        {
                            grid.Columns.Clear();
                            foreach (var column in columns)
                            {
                                AddColumn(grid, column);
                            }
                            columns.CollectionChanged += (s, e) =>
                            {
                                if (e.Action == NotifyCollectionChangedAction.Add)
                                {
                                    foreach (TableColumnViewModel column in e.NewItems)
                                    {
                                        AddColumn(grid, column);
                                    }
                                }
                                else if (e.Action == NotifyCollectionChangedAction.Remove)
                                {
                                    foreach (TableColumnViewModel column in e.OldItems)
                                    {
                                        RemoveColumn(grid, column);
                                    }
                                }
                                else if (e.Action == NotifyCollectionChangedAction.Reset)
                                {
                                    grid.Columns.Clear();
                                }
                            };
                        }
                    }
                })));

        private static void AddColumn(DataGrid grid, TableColumnViewModel column)
        {
            DataGridTextColumn tColumn = new DataGridTextColumn();
            tColumn.Header = column.Header;
            tColumn.MinWidth = 80;
            System.Windows.Data.Binding visibilityBinding = new System.Windows.Data.Binding("IsVisible");
            visibilityBinding.Converter = new BooleanToVisibilityConverter();
            visibilityBinding.Source = column;
            visibilityBinding.Mode = BindingMode.TwoWay;
            BindingOperations.SetBinding(tColumn, DataGridColumn.VisibilityProperty, visibilityBinding);
            tColumn.Binding = new System.Windows.Data.Binding($"ColumnsData[{column.Header}].Value");
            grid.Columns.Add(tColumn);
        }

        private static void RemoveColumn(DataGrid grid, TableColumnViewModel column)
        {
            var dColumn = grid.Columns.FirstOrDefault(c => c.Header.ToString() == column.Header);
            if (dColumn != null)
            {
                grid.Columns.Remove(dColumn);
            }
        }
    }
}
