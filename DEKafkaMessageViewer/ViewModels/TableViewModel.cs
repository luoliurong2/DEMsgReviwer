﻿using DEKafkaMessageViewer.Common;
using DEKafkaMessageViewer.Kafka;
using Prism.Mvvm;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows.Threading;

namespace DEKafkaMessageViewer.ViewModels
{
	public class TableViewModel : BindableBase
    {
		private Dictionary<string, string> columnNameToTypesDict;
        private string _header;
        private string _schemaName;
        private string _classifierName;
        private SynchronizedObservableCollection<TableRowViewModel> _rows = new SynchronizedObservableCollection<TableRowViewModel>();
        private ObservableCollection<TableColumnViewModel> _columns = new ObservableCollection<TableColumnViewModel>();

        public TableViewModel(string classifierName, string schemaName)
		{
            _classifierName = classifierName;
            _schemaName = schemaName;
			var propNames = DEKafkaMessageParser.GetPropertiesOfEntityClass(classifierName);
			columnNameToTypesDict = DEKafkaMessageParser.GetEntityPropertiesTypes(classifierName);
			foreach (var propName in propNames)
			{
				var typeString = columnNameToTypesDict[propName];
				if (typeString == "list`1")
				{
					continue;
				}
				TableColumnViewModel column = new TableColumnViewModel(propName, columnNameToTypesDict[propName]);
				Columns.Add(column);
			}
			_rows.CollectionChanged += Rows_CollectionChanged;
		}

		private void Rows_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
		{
			Header = String.Format(@"{0}.{1} ({2})", SchemaName, ClassifierName, Rows.Count.ToString());
        }

		public string ClassifierName {
            get { return _classifierName; }
            set { SetProperty(ref _classifierName, value); RaisePropertyChanged("ClassifierName"); }
        }

		public string SchemaName
        {
            get { return _schemaName; }
            set { SetProperty(ref _schemaName, value); RaisePropertyChanged("ShemaName"); }
        }

		public string Header
		{
			get
			{
                _header = String.Format(@"{0}.{1} ({2})", SchemaName, ClassifierName, Rows.Count.ToString());
                return _header;
			}
            set { SetProperty(ref _header, value); }
		}

		public SynchronizedObservableCollection<TableRowViewModel> Rows {
            get { return _rows; }
            set { SetProperty(ref _rows, value); RaisePropertyChanged("Rows"); }
        }

		public ObservableCollection<TableColumnViewModel> Columns {
            get { return _columns; }
            set { SetProperty(ref _columns, value); RaisePropertyChanged("Columns"); }
        }

        private string _operation;
		public string Operation {
            get { return _operation; }
            set { SetProperty(ref _operation, value); }
        }

		private void AppendRow(object msgUnit)
		{
			var newRow = CreateRow(msgUnit);
            Rows.Add(newRow);
        }

		private void AppendRow(object msgUnit, Dictionary<string, CellValue> beforeImage, Dictionary<string, CellValue> afterImage)
		{
			var keys = ReadKeys(msgUnit);
			var newRow = new TableRowViewModel(ClassifierName, Columns, afterImage ?? beforeImage, keys, columnNameToTypesDict);
            Rows.Add(newRow);
        }

		private void DeleteRow(Dictionary<string, CellValue> beforeImage, Dictionary<string, CellValue> afterImage)
		{
			var rowToBeDeleted = Rows.FirstOrDefault(r => r.Match(beforeImage ?? afterImage));
			if (rowToBeDeleted != null)
			{
                Rows.Remove(rowToBeDeleted);
            }
		}

		private void ClearRows()
		{
            Rows.Clear();
        }

		private string ReadTargetOperation(object msgUnit)
		{
			var op = msgUnit.GetType().GetProperty(DEKafkaMessageContract.TargetOperation).GetValue(msgUnit).ToString().ToUpper();
			if (string.IsNullOrEmpty(op))
			{
				op = "DELETEALL";
			}
			return op;
		}

		internal void UpdateTableData(object msgUnit)
		{
			var targetOperation = ReadTargetOperation(msgUnit);
			if (targetOperation == "INSERT" || targetOperation == "Create")
			{
				AppendRow(msgUnit);
			}
			else
			{
				var beforeImage = GetBeforeAfterImage(msgUnit, true);
				var afterImage = GetBeforeAfterImage(msgUnit, false);
				if (targetOperation == "DELETE")
				{
					DeleteRow(beforeImage, afterImage);
				}
				else if (targetOperation == "DELETEALL")
				{
					ClearRows();
				}
				else
				{
					var payload = beforeImage ?? afterImage;
					var rowToBeUpdated = Rows.FirstOrDefault(r => r.Match(payload));
					if (rowToBeUpdated != null)
					{
						rowToBeUpdated.Update(afterImage ?? beforeImage);
					}
					else if (targetOperation == "MERGE")
					{
						AppendRow(msgUnit, beforeImage, afterImage);
					}
				}
			}
		}

		private Dictionary<string, CellValue> GetBeforeAfterImage(object msgUnit, bool isBefore)
		{
			var imageType = isBefore ? DEKafkaMessageContract.BeforeImage : DEKafkaMessageContract.AfterImage;
			var payload = msgUnit.GetType().GetProperty(DEKafkaMessageContract.Payload).GetValue(msgUnit);
			var payloadData = payload.GetType().GetProperty(imageType).GetValue(payload);
			var imageString = payloadData == null ? string.Empty : payloadData.ToString();

			object imageData = null;
			if (!string.IsNullOrEmpty(imageString.Trim()))
			{
				imageData = DEKafkaMessageParser.DeserializePayloadData(ClassifierName, imageString);
			}

			if (imageData == null)
			{
				return null;
			}
			else
			{
				Dictionary<string, CellValue> image = new Dictionary<string, CellValue>();
				var columns = imageData.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public);
				foreach (var column in columns)
				{
					var dataType = columnNameToTypesDict[column.Name];
					string columnValue = string.Empty;
					var columnValueObj = column.GetValue(imageData);
					if (columnValueObj != null)
					{
						columnValue = column.PropertyType.IsNested ? GetGroupPropertyValue(columnValueObj) : columnValueObj.ToString();
					}
					image[column.Name] = new CellValue() { Value = columnValue.ToString(), ValueType = dataType };
				}
				return image;
			}
		}

		private string GetGroupPropertyValue(object group, int nestedLevel = 0)
		{
			//The format of a group property value will be like:
			//GroupProp1:
			//		subProp1: value1
			//		subProp2: value2
			//		subGroupProp1:
			//				subSubProp1: subValue1
			//				subSubProp2: subValue2

			var props = group.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public);
			StringBuilder sb = new StringBuilder();

			foreach (var prop in props)
			{
				var propName = prop.Name;

				if (sb.Length > 0)
				{
					sb.AppendLine();
				}

				if (nestedLevel > 0)
				{
					//Append Indent
					for (int i = 0; i < nestedLevel; i++)
					{
						sb.Append("	");
					}
				}

				if (prop.PropertyType.IsNested)
				{
					sb.Append($"{prop.Name}:");
					sb.AppendLine();
					string propValue = GetGroupPropertyValue(prop.GetValue(group), nestedLevel + 1);
					sb.Append(propValue);
				}
				else
				{
					var propValue = prop.GetValue(group);
					if (propValue == null)
					{
						propValue = string.Empty;
					}
					sb.Append($"{prop.Name}: {propValue.ToString()}");
				}
			}
			return sb.ToString();
		}

		private TableRowViewModel CreateRow(object msgUnit, bool isBefore = false)
		{
			var afterValues = GetBeforeAfterImage(msgUnit, isBefore);
			var keyColumns = ReadKeys(msgUnit);

			return new TableRowViewModel(ClassifierName, Columns, afterValues, keyColumns, columnNameToTypesDict);
		}

		private List<string> ReadKeys(object msgUnit)
		{
			var keys = msgUnit.GetType().GetProperty(DEKafkaMessageContract.KeysOfTarget).GetValue(msgUnit) as IEnumerable;
			List<string> keyColumns = new List<string>();
			foreach (var key in keys)
			{
				keyColumns.Add(key.ToString());
			}
			return keyColumns;
		}
	}
}