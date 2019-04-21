using Microsoft.CSharp;
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Web.Script.Serialization;
using System.Xml.Serialization;

namespace DEKafkaMessageViewer.Kafka
{
	public static class DEKafkaMessageParser
	{
		private static Dictionary<string, Type> entityNameToTypeDict;
		private static string entityClassesFilesPath;

		private static Type GetClassType(string filePath, string typeName)
		{
			if (!File.Exists(filePath))
			{
				throw new FileNotFoundException($"Error: {filePath} does not exist!");
			}

			CodeDomProvider compiler = new CSharpCodeProvider(); //CodeDomProvider.CreateProvider("CSharp");
			CompilerParameters config = new CompilerParameters();
			config.IncludeDebugInformation = true;
			config.TreatWarningsAsErrors = false;
			config.ReferencedAssemblies.Add("System.dll");
			config.ReferencedAssemblies.Add("System.Xml.dll");
			config.GenerateExecutable = false;
			config.GenerateInMemory = true;
			var cr = compiler.CompileAssemblyFromFile(config, filePath);

			if (cr.Errors.Count > 0)
			{
				foreach (CompilerError err in cr.Errors)
				{
					Debug.WriteLine(err.ErrorText);
				}
				throw new Exception("Compile class file error!");
			}
			else
			{
				Assembly objAssembly = cr.CompiledAssembly;
				return objAssembly.GetType(typeName);
			}
		}

		public static object DeserializePayloadData(string classifierName, string beforeImage)
		{
			var dataObjType = entityNameToTypeDict[classifierName];
			JavaScriptSerializer jsonSerializer = new JavaScriptSerializer();
			var dataObj = jsonSerializer.Deserialize(beforeImage, dataObjType);
			return dataObj;
		}

		public static void InitializeEntityClassesTypes(string eneityClassesDirectoyPath)
		{
			entityClassesFilesPath = eneityClassesDirectoyPath;
			entityNameToTypeDict = new Dictionary<string, Type>();
			DirectoryInfo direct = new DirectoryInfo(entityClassesFilesPath);
			foreach (var file in direct.EnumerateFiles())
			{
				if (file.Extension.ToUpper(CultureInfo.InvariantCulture) == ".CS")
				{
					var className = file.Name.Replace(".cs", string.Empty);
					var type = GetClassType(file.FullName, $"DE.Kafka.{className}");
					entityNameToTypeDict.Add(className, type);
				}
			}
			if (!entityNameToTypeDict.ContainsKey(DEKafkaMessageContract.DEKafkaMessage))
			{
				throw new FileNotFoundException($"Error: DEKafkaMessage class definition file is not found! Class Name: {DEKafkaMessageContract.DEKafkaMessage}");
			}
		}

		public static IEnumerable<string> GetPropertiesOfEntityClass(string className)
		{
			if(!entityNameToTypeDict.ContainsKey(className))
			{
				throw new FileNotFoundException($"Error: DEKafkaMessage class definition file is not found! Class Name: {className}");
			}
			return entityNameToTypeDict[className].GetProperties(BindingFlags.Instance | BindingFlags.Public).Select(p => p.Name);
		}

		public static Dictionary<string, string> GetEntityPropertiesTypes(string className)
		{
			if (!entityNameToTypeDict.ContainsKey(className))
			{
				throw new FileNotFoundException($"Error: DEKafkaMessage class definition file is not found! Class Name: {className}");
			}
			Dictionary<string, string> result = new Dictionary<string, string>();
			var props = entityNameToTypeDict[className].GetProperties(BindingFlags.Instance | BindingFlags.Public);
			foreach (var prop in props)
			{
				if(prop.PropertyType.IsNested)
				{
					result.Add(prop.Name, "Group");
				}
				else
				{
					result.Add(prop.Name, prop.PropertyType.FullName.ToLower());
				}
			}
			return result;
		}

		public static object DeSerializeDEKafkaMessage(string xml)
		{
			var msgType = entityNameToTypeDict[DEKafkaMessageContract.DEKafkaMessage];
			XmlSerializer serializer = new XmlSerializer(msgType);
			using (var reader = new StringReader(xml))
			{
				return serializer.Deserialize(reader);
			}
		}

		internal static void Cleanup()
		{
			entityNameToTypeDict.Clear();
			entityClassesFilesPath = string.Empty;
		}
	}
}
