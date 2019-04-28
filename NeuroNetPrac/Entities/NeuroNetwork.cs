using System;

namespace NeuroNet.Entities
{
	[Serializable]
	public class NeuroNetwork
	{
		public double[][] Neurons { get; }
		public int[] NeuronsCount { get; }
		public double[][][] Weights { get; set; }
		public double Speed { get; set; }
		public double ActivationParam { get; set; }
		public double AllowableError { get; set; }
		public Func<double, double, double> ActivationFunc { get; }
		public Func<double, double, double> ActivationFuncDerivative { get; }

		public event Action OnStudyingStart;
		public event Action<double[][], double[][]> OnStudyingEnd;
		public event Action<int> OnProcessingSnapStart;
		public event Action<int, double[], double[]> OnProcessingSnapEnd;

		public NeuroNetwork(
			int[] neuronsCount,
			Func<double, double, double> activationFunc = null,
			Func<double, double, double> activationFuncDerivative = null,
			double activationParam = 1,
			double speed = 1,
			double allowableError = 0.05)
		{
			NeuronsCount = neuronsCount;
			ActivationParam = activationParam;
			AllowableError = allowableError;
			ActivationFunc = activationFunc ?? ((x, y) => 1 / (1 + Math.Exp(-x * y)) );
			ActivationFuncDerivative = activationFuncDerivative ?? ((x, y) => y * x * (1 - x) );
			Speed = speed;

			var rand = new Random();
			Neurons = new double[neuronsCount.Length][];
			Weights = new double[neuronsCount.Length][][];

			Neurons[0] = new double[neuronsCount[0] + 1];
			Neurons[0][neuronsCount[0]] = 1;
			for (int layerIdx = 1; layerIdx < Neurons.Length - 1; layerIdx++)
			{
				Neurons[layerIdx] = new double[neuronsCount[layerIdx] + 1];
				Weights[layerIdx] = new double[neuronsCount[layerIdx] + 1][];

				Neurons[layerIdx][neuronsCount[layerIdx]] = 1;

				for (int neuronIdx = 0; neuronIdx < neuronsCount[layerIdx] + 1; neuronIdx++)
				{
					Weights[layerIdx][neuronIdx] = new double[neuronsCount[layerIdx - 1] + 1];
					for (int synapsIdx = 0; synapsIdx < neuronsCount[layerIdx - 1] + 1; synapsIdx++)
						Weights[layerIdx][neuronIdx][synapsIdx] = rand.NextDouble();
				}
			}

			Neurons[neuronsCount.Length - 1] = new double[neuronsCount[neuronsCount.Length - 1]];
			Weights[neuronsCount.Length - 1] = new double[neuronsCount[neuronsCount.Length - 1]][];

			for (int neuronIdx = 0; neuronIdx < Weights[neuronsCount.Length - 1].Length; neuronIdx++)
			{
				Weights[neuronsCount.Length - 1][neuronIdx] = new double[neuronsCount[neuronsCount.Length - 2] + 1];
				for (int synapsIdx = 0; synapsIdx < neuronsCount[neuronsCount.Length - 2]; synapsIdx++)
					Weights[neuronsCount.Length - 1][neuronIdx][synapsIdx] = rand.NextDouble();
			}

			for (int i = 0; i < Weights[1].Length; i++)
				Weights[1][i][neuronsCount[0]] = -40;
		}

		public void LogOnStudyingStart() => OnStudyingStart();
		public void LogOnStudyingEnd(double[][] errors, double[][] testErrors) => OnStudyingEnd(errors, testErrors);
		public void LogOnProcessingSnapStart(int snap)
		{
			if (OnProcessingSnapStart != null)
			{
				OnProcessingSnapStart(snap);
			}
		}
		public void LogOnProcessingSnapEnd(int snap, double[] errs, double[] testErrs) => OnProcessingSnapEnd(snap, errs, testErrs);
	}
}