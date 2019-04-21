using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DEKafkaMessageGenerator
{
	class Program
	{
		static void Main(string[] args)
		{
			Console.WriteLine("How many messages do you want to generate? Input a number and then press <ENTER> to continue...");
			var messageCount = 0;
			if (int.TryParse(Console.ReadLine(), out messageCount))
			{
				return;
			}


		}
	}
}
