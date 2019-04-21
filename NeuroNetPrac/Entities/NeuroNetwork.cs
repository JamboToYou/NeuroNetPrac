using System;

namespace NeuroNet.Entities
{
	[Serializable]
	public class NeuroNetwork
	{
		public double[][] Neurons { get; }
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
			ActivationParam = activationParam;
			AllowableError = allowableError;
			ActivationFunc = activationFunc ?? ((x, y) => 1 / (1 + Math.Exp(-x * y)) );
			ActivationFuncDerivative = activationFuncDerivative ?? ((x, y) => y * x * (1 - x) );
			Speed = speed;

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