using System.Collections.Generic;

namespace DE.Kafka
{
	public class CRTSG
	{

		public static readonly string FormatVersion = "4/22/2019 12:44:48 PM";

		public long IDX { get; set; }
		public short? R_SMINT { get; set; }
		public int? R_INT { get; set; }
		public string R_CHAR { get; set; }
		public string R_NCHAR { get; set; }
		public string R_UCS2 { get; set; }
		public decimal? R_NUMERIC { get; set; }
		public decimal? R_DCMAL { get; set; }
		public double? R_DOUBLE { get; set; }
		public float? R_REAL { get; set; }
		public float? R_FLOAT { get; set; }
		public string R_DATE { get; set; }
		public string R_TIME { get; set; }
		public string R_TMSP { get; set; }
	}
}
