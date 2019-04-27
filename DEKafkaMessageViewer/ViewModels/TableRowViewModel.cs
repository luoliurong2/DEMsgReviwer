using DEKafkaMessageViewer.Common;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DEKafkaMessageViewer.ViewModels
{
	public class TableRowViewModel:BindableBase
	{
		private string tableName;
		private Dictionary<string, string> columnNameToTypesDict;
        private Dictionary<string, RowCellViewModel> _columnsData = new Dictionary<string, RowCellViewModel>();
        private List<string> _keyColNames = new List<string>();

        public TableRowViewModel(string tableName, 
								 IEnumerable<TableColumnViewModel> columns,
								 Dictionary<string, CellValue> columnsData,
								 List<string> keyColumns,
								 Dictionary<string, string> columnNameToTypesDict)
		{
			this.tableName = tableName;
			foreach (var column in columns)
			{
				var columnValue = columnsData.ContainsKey(column.Header) ? columnsData[column.Header].Value : string.Empty;
                _columnsData[column.Header] = new RowCellViewModel() { Value = columnValue };
			}
            _keyColNames = keyColumns;
			this.columnNameToTypesDict = columnNameToTypesDict;
		}

		public Dictionary<string, RowCellViewModel> ColumnsData {
            get { return _columnsData; }
            set { SetProperty(ref _columnsData, value); RaisePropertyChanged("ColumnsData"); }
        }

		internal List<string> KeyColumnsNames {
            get { return _keyColNames; }
            set { SetProperty(ref _keyColNames, value); RaisePropertyChanged("KeyColumnsNames"); }
        }

		public string TableName
		{
            get { return tableName; }
            set { SetProperty(ref tableName, value); RaisePropertyChanged("TableName"); }
		}

		public override int GetHashCode()
		{
			return base.GetHashCode();
		}

		public override bool Equals(object obj)
		{
			TableRowViewModel other = obj as TableRowViewModel;
			if (other == null)
			{
				return false;
			}
			if (tableName != other.tableName)
			{
				return false;
			}

			if (KeyColumnsNames.Any())
			{
				foreach (var key in KeyColumnsNames)
				{
					if (!other.KeyColumnsNames.Contains(key))
					{
						return false;
					}

					if (ColumnsData[key] != other.ColumnsData[key])
					{
						return false;
					}
				}
			}
			else
			{
				foreach (var pair in ColumnsData)
				{
					if (!other.ColumnsData.ContainsKey(pair.Key) || ColumnsData[pair.Key] != other.ColumnsData[pair.Key])
					{
						return false;
					}
				}
			}

			return true;
		}

		internal bool Match(Dictionary<string, CellValue> payload)
		{
			if(KeyColumnsNames.Any())
			{
				foreach (var keyColumn in KeyColumnsNames)
				{
					if(!payload.ContainsKey(keyColumn) || payload[keyColumn].Value != ColumnsData[keyColumn].Value)
					{
						return false;
					}
				}
			}
			else
			{
				foreach (var pair in ColumnsData)
				{
					if(!payload.ContainsKey(pair.Key) || payload[pair.Key].Value != pair.Value.Value)
					{
						return false;
					}
				}
			}

			return true;
		}

		internal void Update(Dictionary<string, CellValue> newValues)
		{
			foreach (var pair in newValues)
			{
				ColumnsData[pair.Key].Value = newValues[pair.Key].Value;
			}
		}

		internal bool Match(IEnumerable<CompareCondition> conditions)
		{
			foreach (var condition in conditions)
			{
				bool matched = Match(condition);
				if (condition.NeedCombineNextCondition)
				{
					if (condition.NextConditionSwitch == ConditionSwitch.And)
					{
						if (matched)
						{
							continue;
						}
						else
						{
							return false;
						}
					}
					else
					{
						continue;
					}
				}
				else
				{
					return matched;
				}
			}
			return false;
		}

		private bool Match(CompareCondition condition)
		{
			var columnTypeString = columnNameToTypesDict[condition.FiledName];
			var columnType = TableColumnViewModel.TypeStringToColumnTypeDict[columnTypeString];
			if (!TableColumnViewModel.CanCompare(columnType))
			{
				return false;
			}

			var column = ColumnsData[condition.FiledName];
			var comparedValue = condition.ComparedValue;
			switch (condition.Operator)
			{
				case CompareOperator.BiggerThan:
					return ColumnBiggerThan(column.Value, comparedValue, columnType);

				case CompareOperator.SmallerThan:
					return ColumnSmallerThan(column.Value, comparedValue, columnType);

				case CompareOperator.BiggerThanOrEquals:
					return ColumnBiggerThanOrEquals(column.Value, comparedValue, columnType);

				case CompareOperator.SmallerThanOrEquals:
					return ColumnSmallerThanOrEquals(column.Value, comparedValue, columnType);

				case CompareOperator.Equals:
					return ColumnEquals(column.Value, comparedValue, columnType);

				case CompareOperator.NotEquals:
					return ColumnNotEquals(column.Value, comparedValue, columnType);

				default:
					return false;
			}
		}

		private bool ColumnEquals(string columnValue, string comparedValue, ColumnDataType dataType)
		{
			switch (dataType)
			{
				case ColumnDataType.String:
					return columnValue.Equals(comparedValue);
				case ColumnDataType.Long:
					return long.Parse(columnValue) == long.Parse(comparedValue);
				case ColumnDataType.Float:
					return float.Parse(columnValue) == float.Parse(comparedValue);
				case ColumnDataType.Double:
					return double.Parse(columnValue) == double.Parse(comparedValue);
				case ColumnDataType.Short:
				case ColumnDataType.Byte:
				case ColumnDataType.Int:
					return int.Parse(columnValue) == int.Parse(comparedValue);
				case ColumnDataType.Decimal:
					return decimal.Parse(columnValue) == decimal.Parse(comparedValue);
				case ColumnDataType.DateTime:
					return DateTime.Parse(columnValue) == DateTime.Parse(comparedValue);
				case ColumnDataType.Bool:
					return bool.Parse(columnValue) == bool.Parse(comparedValue);
				default:
					return false;
			}
		}

		private bool ColumnBiggerThan(string columnValue, string comparedValue, ColumnDataType dataType)
		{
			switch (dataType)
			{
				case ColumnDataType.String:
					return string.Compare(columnValue, comparedValue) > 0;
				case ColumnDataType.Long:
					return long.Parse(columnValue) > long.Parse(comparedValue);
				case ColumnDataType.Float:
					return float.Parse(columnValue) > float.Parse(comparedValue);
				case ColumnDataType.Double:
					return double.Parse(columnValue) > double.Parse(comparedValue);
				case ColumnDataType.Short:
				case ColumnDataType.Byte:
				case ColumnDataType.Int:
					return int.Parse(columnValue) > int.Parse(comparedValue);
				case ColumnDataType.Decimal:
					return decimal.Parse(columnValue) > decimal.Parse(comparedValue);
				case ColumnDataType.DateTime:
					return DateTime.Parse(columnValue) > DateTime.Parse(comparedValue);
				default:
					return false;
			}
		}

		private bool ColumnSmallerThan(string columnValue, string comparedValue, ColumnDataType dataType)
		{
			switch (dataType)
			{
				case ColumnDataType.String:
					return string.Compare(columnValue, comparedValue) < 0;
				case ColumnDataType.Long:
					return long.Parse(columnValue) < long.Parse(comparedValue);
				case ColumnDataType.Float:
					return float.Parse(columnValue) < float.Parse(comparedValue);
				case ColumnDataType.Double:
					return double.Parse(columnValue) < double.Parse(comparedValue);
				case ColumnDataType.Short:
				case ColumnDataType.Byte:
				case ColumnDataType.Int:
					return int.Parse(columnValue) < int.Parse(comparedValue);
				case ColumnDataType.Decimal:
					return decimal.Parse(columnValue) < decimal.Parse(comparedValue);
				case ColumnDataType.DateTime:
					return DateTime.Parse(columnValue) < DateTime.Parse(comparedValue);
				default:
					return false;
			}
		}

		private bool ColumnBiggerThanOrEquals(string columnValue, string comparedValue, ColumnDataType dataType)
		{
			switch (dataType)
			{
				case ColumnDataType.String:
					return string.Compare(columnValue, comparedValue) >= 0;
				case ColumnDataType.Long:
					return long.Parse(columnValue) >= long.Parse(comparedValue);
				case ColumnDataType.Float:
					return float.Parse(columnValue) >= float.Parse(comparedValue);
				case ColumnDataType.Double:
					return double.Parse(columnValue) >= double.Parse(comparedValue);
				case ColumnDataType.Short:
				case ColumnDataType.Byte:
				case ColumnDataType.Int:
					return int.Parse(columnValue) >= int.Parse(comparedValue);
				case ColumnDataType.Decimal:
					return decimal.Parse(columnValue) >= decimal.Parse(comparedValue);
				case ColumnDataType.DateTime:
					return DateTime.Parse(columnValue) >= DateTime.Parse(comparedValue);
				case ColumnDataType.Bool:
					return bool.Parse(columnValue) == bool.Parse(comparedValue);
				default:
					return false;
			}
		}

		private bool ColumnSmallerThanOrEquals(string columnValue, string comparedValue, ColumnDataType dataType)
		{
			switch (dataType)
			{
				case ColumnDataType.String:
					return string.Compare(columnValue, comparedValue) <= 0;
				case ColumnDataType.Long:
					return long.Parse(columnValue) <= long.Parse(comparedValue);
				case ColumnDataType.Float:
					return float.Parse(columnValue) <= float.Parse(comparedValue);
				case ColumnDataType.Double:
					return double.Parse(columnValue) <= double.Parse(comparedValue);
				case ColumnDataType.Short:
				case ColumnDataType.Byte:
				case ColumnDataType.Int:
					return int.Parse(columnValue) <= int.Parse(comparedValue);
				case ColumnDataType.Decimal:
					return decimal.Parse(columnValue) <= decimal.Parse(comparedValue);
				case ColumnDataType.DateTime:
					return DateTime.Parse(columnValue) <= DateTime.Parse(comparedValue);
				default:
					return false;
			}
		}

		private bool ColumnNotEquals(string columnValue, string comparedValue, ColumnDataType dataType)
		{
			switch (dataType)
			{
				case ColumnDataType.String:
					return !columnValue.Equals(comparedValue);
				case ColumnDataType.Long:
					return long.Parse(columnValue) != long.Parse(comparedValue);
				case ColumnDataType.Float:
					return float.Parse(columnValue) != float.Parse(comparedValue);
				case ColumnDataType.Double:
					return double.Parse(columnValue) != double.Parse(comparedValue);
				case ColumnDataType.Short:
				case ColumnDataType.Byte:
				case ColumnDataType.Int:
					return int.Parse(columnValue) != int.Parse(comparedValue);
				case ColumnDataType.Decimal:
					return decimal.Parse(columnValue) != decimal.Parse(comparedValue);
				case ColumnDataType.DateTime:
					return DateTime.Parse(columnValue) != DateTime.Parse(comparedValue);
				case ColumnDataType.Bool:
					return bool.Parse(columnValue) != bool.Parse(comparedValue);
				default:
					return false;
			}
		}
	}

	public class CompareCondition
	{
		private static Dictionary<string, CompareOperator> stringToOperatorsDict = new Dictionary<string, CompareOperator>()
			{
				{ ">", CompareOperator.BiggerThan},
				{ "<", CompareOperator.SmallerThan},
				{ ">=", CompareOperator.BiggerThanOrEquals},
				{ "<=", CompareOperator.SmallerThanOrEquals},
				{ "=", CompareOperator.Equals},
				{ "!=", CompareOperator.NotEquals},
			};

		public CompareCondition(string fieldName, string op, string value, ConditionSwitch nextSwitch = ConditionSwitch.None)
		{
			FiledName = fieldName;
			Operator = stringToOperatorsDict[op];
			ComparedValue = value;
			NextConditionSwitch = nextSwitch;
		}

		public ConditionSwitch NextConditionSwitch { get; set; }

		public bool NeedCombineNextCondition
		{
			get
			{
				return NextConditionSwitch != ConditionSwitch.None;
			}
		}

		public string FiledName { get; set; }

		public CompareOperator Operator { get; set; }

		public string ComparedValue { get; set; }
	}
}
