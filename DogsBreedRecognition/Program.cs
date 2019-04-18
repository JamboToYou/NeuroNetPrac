using System;
using DogsBreedRecognition.Entities;

namespace DogsBreedRecognition
{
	class Program
	{
		static void Main(string[] args)
		{
			if (args[0] == "-s")
				DogsBreedRecognitionNeuroNet.Study();
			else
				Console.WriteLine(DogsBreedRecognitionNeuroNet.Execute(args[0]));
		}
	}
}
