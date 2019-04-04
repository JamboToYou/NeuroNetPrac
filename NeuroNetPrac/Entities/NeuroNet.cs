using System;

namespace NeuroNet.Entities
{
	public class NeuroNet
	{
		private static Func<double, double> DefaultActivationFunc = x => 1 / 1 + Math.Exp(-x);
		public double[][] Neurons { get; set; }
		public double[][,] Weights { get; set; }
		public Func<double, double> ActivationFunc { get; set; }

		public NeuroNet(int[] neuronsCount, Func<double, double> activationFunc = null)
		{
			ActivationFunc = activationFunc ?? DefaultActivationFunc;

			var rand = new Random();
			Neurons = new double[neuronsCount.Length][];
			Weights = new double[neuronsCount.Length][,];

			Neurons[0] = new double[neuronsCount[0]];
			for (int layerIdx = 1; layerIdx < neurons.Length; layerIdx++)
			{
				Neurons[layerIdx] = new double[neuronsCount[layerIdx]];
				Weights[layerIdx] = new double
					[
						neuronsCount[layerIdx],
						neuronsCount[layerIdx - 1]
					];
				
				for (int neuronIdx = 0; neuronIdx < neuronsCount[layerIdx]; neuronIdx++)
					for (int synapsIdx = 0; synapsIdx < neuronsCount[layerIdx - 1]; synapsIdx++)
						Weights[layerIdx][neuronIdx, synapsIdx] = rand.NextDouble();
			}
		}
	}
}