using DEKafkaMessageViewer.Common;
using Prism.Mvvm;
using System;
using System.Collections.Generic;

namespace DEKafkaMessageViewer.ViewModels
{
	public class TableColumnViewModel : BindableBase
    {
		private bool isVisible = true;
		public static readonly Dictionary<string, ColumnDataType> TypeStringToColumnTypeDict = new Dictionary<string, ColumnDataType>()
		{
			{ typeof(string).FullName.ToLower(), ColumnDataType.String },
			{ typeof(long).FullName.ToLower(), ColumnDataType.Long },
			{ typeof(long?).FullName.ToLower(), ColumnDataType.LongNullable},
			{ typeof(float).FullName.ToLower(), ColumnDataType.Float },
			{ typeof(float?).FullName.ToLower(), ColumnDataType.FloatNullable },
			{ typeof(double).FullName.ToLower(), ColumnDataType.Double },
			{ typeof(double?).FullName.ToLower(), ColumnDataType.DoubleNullable },
			{ typeof(short).FullName.ToLower(), ColumnDataType.Short },
			{ typeof(short?).FullName.ToLower(), ColumnDataType.ShortNullable },
			{ typeof(byte).FullName.ToLower(), ColumnDataType.Byte },
			{ typeof(byte?).FullName.ToLower(), ColumnDataType.ByteNullable },
			{ typeof(int).FullName.ToLower(), ColumnDataType.Int },
			{ typeof(int?).FullName.ToLower(), ColumnDataType.IntNullable },
			{ typeof(bool).FullName.ToLower(), ColumnDataType.Bool },
			{ typeof(bool?).FullName.ToLower(), ColumnDataType.BoolNullable },
			{ typeof(byte[]).FullName.ToLower(), ColumnDataType.ByteArray },
			{ typeof(decimal).FullName.ToLower(), ColumnDataType.Decimal },
			{ typeof(decimal?).FullName.ToLower(), ColumnDataType.DecimalNullable },
			{ typeof(DateTime).FullName.ToLower(), ColumnDataType.DateTime },
			{ typeof(DateTime?).FullName.ToLower(), ColumnDataType.DateTimeNullable },
			{ "group", ColumnDataType.Group },
		};

		public TableColumnViewModel(string header, string columnType)
		{
            _header = header;
			string typeString = columnType.ToLower();
			ColumnDataType columnValueType = ColumnDataType.String;
			TypeStringToColumnTypeDict.TryGetValue(typeString, out columnValueType);
            _valueType = columnValueType;
		}

		public static bool CanCompare(ColumnDataType columnType)
		{
			return columnType != ColumnDataType.ByteArray;
		}

        private string _header;
		public string Header {
            get { return _header; }
            set { SetProperty(ref _header, value); RaisePropertyChanged("Header"); }
        }

        private ColumnDataType _valueType;
		public ColumnDataType ValueType {
            get { return _valueType; }
            set { SetProperty(ref _valueType, value); RaisePropertyChanged("ValueType"); }
        }

		public void Hide()
		{
			IsVisible = false;
		}

		public bool IsVisible
		{
			get { return isVisible; }
			set { SetProperty(ref isVisible, value); RaisePropertyChanged("IsVisible"); }
		}

		public void Show()
		{
			IsVisible = true;
		}
	}
}
