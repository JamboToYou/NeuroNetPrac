using System;
using System.IO;
using System.Linq;
using AlphabetsRecognition.Core;

namespace AlphabetsRecognition
{
	class Program
	{
		static void Main(string[] args)
		{
			if (args[0] == "-s")
				AlphabetsRecognitionNN.Study();
			else
				Console.WriteLine(AlphabetsRecognitionNN.Execute(args[0]));
			// string[] g = null;

			// using (var fs = new FileStream(Directory.GetCurrentDirectory() + @"\TeachingData\handwritten_data_785.csv", FileMode.Open))
			// using (var sr = new StreamReader(fs))
			// {
			// 	for (int l = 0; l < 30; l++)
			// 	{

			// 		g = sr.ReadLine().Split(',').Skip(1).Select(x => x == "0" ? x : "*").ToArray();

			// 		for (int i = 0; i < 28; i++)
			// 		{
			// 			for (int k = 0; k < 28; k++)
			// 			{
			// 				Console.Write(g[i * 28 + k] + " ");
			// 			}
			// 			Console.WriteLine();
			// 		}
			// 	}
			// }
		}
	}
}
