using System;

namespace NeuroNet.Entities
{
	public class NeuroNet
	{
		public double[][] Neurons { get; set; }
		public double[][,] Weights { get; set; }
		public Func<double, double> ActivationFunc { get; set; }

		public NeuroNet(int[] neuronsCount)
		{
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
						Weights[layerIdx][neuronIdx, synapsIdx] = rand.NextDouble() - 0.5;
			}
		}

		public void InitInput(double[] inputs)
		{

		}
	}
}