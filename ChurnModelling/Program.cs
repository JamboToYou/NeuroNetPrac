using System;
using System.IO;
using ChurnModelling.Core;
using Common.Utils;

namespace ChurnModelling
{
	class Program
	{
		static void Main(string[] args)
		{
			var cm = new ChurnModellingNN();
			// cm.Study();
			Console.WriteLine(cm.Execute(Directory.GetCurrentDirectory() + @"\input.csv"));
		}
	}
}
