using System;

namespace NeuroNet.Entities
{
	[Serializable]
	public class NeuroNetwork
	{
		public double[][] Neurons { get; }
		public double[][][] Weights { get; }
		public double Speed { get; set; }
		public double ActivationParam { get; set; }
		public double StudyingSensetivity { get; set; }
		public Func<double, double> ActivationFunc { get; }
		public Func<double, double> ActivationFuncDerivative { get; }

		public event Action OnStudyingStart;
		public event Action<double[][], double[][]> OnStudyingEnd;

		public NeuroNetwork(
			int[] neuronsCount,
			Func<double, double> activationFunc = null,
			Func<double, double> activationFuncDerivative = null,
			double activationParam = 1,
			double speed = 1)
		{
			ActivationParam = activationParam;
			ActivationFunc = activationFunc ?? (x => 1 / 1 + Math.Exp(-x * ActivationParam) );
			ActivationFuncDerivative = activationFuncDerivative ?? (x => ActivationParam * x * (1 - x) );

			var rand = new Random();
			Neurons = new double[neuronsCount.Length][];
			Weights = new double[neuronsCount.Length][][];

			Neurons[0] = new double[neuronsCount[0]];
			for (int layerIdx = 1; layerIdx < Neurons.Length; layerIdx++)
			{
				Neurons[layerIdx] = new double[neuronsCount[layerIdx]];
				Weights[layerIdx] = new double[neuronsCount[layerIdx]][];
				
				for (int neuronIdx = 0; neuronIdx < neuronsCount[layerIdx]; neuronIdx++)
				{
					Weights[layerIdx][neuronIdx] = new double[neuronsCount[layerIdx - 1]];
					for (int synapsIdx = 0; synapsIdx < neuronsCount[layerIdx - 1]; synapsIdx++)
						Weights[layerIdx][neuronIdx][synapsIdx] = rand.NextDouble();
				}
			}
		}

		public void LogOnStudyingStart() => OnStudyingStart();
		public void LogOnStudyingEnd(double[][] errors, double[][] testErrors) => OnStudyingEnd(errors, testErrors);
	}
}