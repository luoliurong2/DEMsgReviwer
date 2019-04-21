using System.IO;
using System.Text;
using System.Xml;

namespace DEKafkaMessageViewer.Kafka
{
	public static class KafkaMessageXMLFormatter
	{
		public static string Fromat(string xml)
		{
			XmlDocument xd = new XmlDocument();
			xd.LoadXml(xml);
			StringBuilder sb = new StringBuilder();
			using (StringWriter sw = new StringWriter(sb))
			{
				XmlTextWriter xtw = null;
				using (xtw = new XmlTextWriter(sw))
				{
					xtw.Formatting = Formatting.Indented;
					xtw.Indentation = 1;
					xtw.IndentChar = '\t';
					xd.WriteTo(xtw);
					xtw.Close();
				}
				sw.Close();
			}
				
			return sb.ToString();
		}
	}
}
