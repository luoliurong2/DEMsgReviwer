namespace DEKafkaMessageViewer.Common
{
	public enum CompareOperator
	{
		BiggerThan,
		SmallerThan,
		BiggerThanOrEquals,
		SmallerThanOrEquals,
		Equals,
		NotEquals,
	}

	public enum ConditionSwitch
	{
		None,
		And,
		Or
	}

	public enum ColumnDataType
	{
		String,
		Long,
		LongNullable,
		Int,
		IntNullable,
		Decimal,
		DecimalNullable,
		Float,
		FloatNullable,
		Double,
		DoubleNullable,
		DateTime,
		DateTimeNullable,
		Short,
		ShortNullable,
		Bool,
		BoolNullable,
		Byte,
		ByteNullable,
		ByteArray,
		Group,

	}
}
