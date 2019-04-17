using System;
using System.Collections.Generic;

namespace DogsBreedRecognition.Entities
{
	public class ErrorsAnalyzer
	{
		private int _epochesCount { get; set; }
		private List<double> _normalizedSnapErrors { get; }
		private List<double> _normalizedTestErrors { get; }

		public ErrorsAnalyzer(int epochesCount)
		{
			_epochesCount = epochesCount;
			_normalizedSnapErrors = new List<double>();
			_normalizedTestErrors = new List<double>();
		}

		public void ConsiderErrors(double[][] snapsErrors, double[][] testErrors)
		{

		}

		public double GetAverageErrorEstimate()
		{
			// TODO: do
			return 0;
		}
	}
}
