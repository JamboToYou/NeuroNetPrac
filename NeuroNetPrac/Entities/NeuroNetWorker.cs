using System;
using System.Linq;
using System.Collections.Generic;

namespace NeuroNet.Entities
{
	public static class NeuroNetWorker
	{
		public static double[] Execute(this NeuroNetwork neuroNet, double[] snap)
		{
			var neurons = neuroNet.Neurons;
			var weights = neuroNet.Weights;
			var result = new double[neurons[neurons.Length - 1].Length];

			if (snap.Length != neurons[0].Length)
				throw new ArgumentException("[warn]: Wrong count of values in snap {0}");

			neurons[0] = snap;

			for (int i = 1; i < neurons.Length; i++)
				for (int j = 0; j < neurons[i].Length; j++)
					neurons[i][j] = ExecuteOne(i, neurons[i - 1], weights[i][j], neuroNet.ActivationFunc);

			return neurons[neurons.Length - 1];
		}

		private static double ExecuteOne(int currLayer, double[] prevLayer, double[] weights, Func<double, double> activationFunc)
		{
			var sum = 0.0;

			for (int i = 0; i < prevLayer.Length; i++)
				sum += prevLayer[i] * weights[i];

			sum = currLayer != 1 ? sum : sum * 0.002;

			return activationFunc(sum);
		}

		public static void Study(
			this NeuroNetwork neuroNet,
			double[][] snaps,
			double[][] expected,
			double[][] testSnaps,
			double[][] testExpected)
		{
			double[] output;
			var comparer = new NeuroNetResultsComparer(neuroNet.StudyingSensetivity);
			var neurons = neuroNet.Neurons;
			var weights = neuroNet.Weights;
			var outputLength = neurons[neurons.Length - 1].Length;

			var errors = new double[snaps.Length][];
			var testErrors = new double[testSnaps.Length][];
			for (int i = 0; i < errors.Length; errors[i++] = new double[outputLength]) ;
			for (int i = 0; i < testErrors.Length; testErrors[i++] = new double[outputLength]) ;

			var deltas = new double[neurons.Length][];

			neuroNet.LogOnStudyingStart();

			// Teaching
			for (int snapIdx = 0; snapIdx < snaps.Length; snapIdx++)
			{
				neuroNet.LogOnProcessingSnapStart(snapIdx);

				#region Executing snap

				if (outputLength != expected[snapIdx].Length)
				{
					Console.WriteLine($"[warn]: The length of teaching output doesn`t match to length of neuroNet`s output in snap {snapIdx}");
					continue;
				}

				try
				{
					output = neuroNet.Execute(snaps[snapIdx]);
					// for (int i = 0; i < output.Length; Console.Write(output[i++] + " "));
					// Console.WriteLine();
				}
				catch (ArgumentException ex)
				{
					Console.WriteLine(ex.Message, snapIdx);
					continue;
				}

				#endregion

				// Saving error
				for (int i = 0; i < outputLength; errors[snapIdx][i] = expected[snapIdx][i] - output[i++]) ;

				if (!output.SequenceEqual(expected[snapIdx], comparer))
				{
					#region Last layer deltas

					var lastLayerNeurons = neurons.Last();
					deltas[deltas.Length - 1] = new double[neurons.Last().Length];
					var lastLayerDeltas = deltas[deltas.Length - 1];

					for (int r = 0; r < lastLayerDeltas.Length; r++)
						lastLayerDeltas[r] = (expected[snapIdx][r] - output[r]) * neuroNet.ActivationFuncDerivative(lastLayerNeurons[r]);

					#endregion

					#region Correcting last layer weights

					for (int neuronIdx = 0; neuronIdx < outputLength; neuronIdx++)
					{
						for (int prevLayerNeuronIdx = 0; prevLayerNeuronIdx < neurons[neurons.Length - 2].Length; prevLayerNeuronIdx++)
						{
							weights[neurons.Length - 1][neuronIdx][prevLayerNeuronIdx] +=
								lastLayerDeltas[neuronIdx] *
								neurons[neurons.Length - 2][prevLayerNeuronIdx] *
								neuroNet.Speed;
						}
					}

					#endregion

					#region Other weights

					for (int layerIdx = neurons.Length - 2; layerIdx > 0; layerIdx--)
					{
						#region Calculating deltas for current layer

						deltas[layerIdx] = new double[neurons[layerIdx].Length];
						for (int neuronIdx = 0; neuronIdx < neurons[layerIdx].Length; neuronIdx++)
						{
							for (int nextLayerNeuronIdx = 0; nextLayerNeuronIdx < neurons[layerIdx + 1].Length; nextLayerNeuronIdx++)
							{
								deltas[layerIdx][neuronIdx] +=
									deltas
										[layerIdx + 1]
										[nextLayerNeuronIdx]
										+
									weights
										[layerIdx + 1]
										[nextLayerNeuronIdx]
										[neuronIdx];
							}
							deltas[layerIdx][neuronIdx] *= neuroNet.ActivationFuncDerivative(neurons[layerIdx][neuronIdx]);
						}

						#endregion

						#region Correcting weights for current layer

						for (int neuronIdx = 0; neuronIdx < neurons[layerIdx].Length; neuronIdx++)
						{
							for (int prevLayerNeuronIdx = 0; prevLayerNeuronIdx < neurons[layerIdx - 1].Length; prevLayerNeuronIdx++)
							{
								weights[layerIdx][neuronIdx][prevLayerNeuronIdx] +=
									deltas[layerIdx][neuronIdx] *
									neurons[layerIdx - 1][prevLayerNeuronIdx] *
									neuroNet.Speed;
							}
						}

						#endregion
					}

					#endregion
				}

				neuroNet.LogOnProcessingSnapEnd(snapIdx, errors[snapIdx]);
			}

			// Testing by test snaps
			for (int snapIdx = 0; snapIdx < testSnaps.Length; snapIdx++)
			{
				#region Executing

				if (outputLength != testExpected[snapIdx].Length)
				{
					Console.WriteLine($"[warn]: The length of testing output doesn`t match to length of neuroNet`s output in test snap {snapIdx}");
					continue;
				}

				try
				{
					output = neuroNet.Execute(snaps[snapIdx]);
				}
				catch (ArgumentException ex)
				{
					Console.WriteLine(ex.Message, snapIdx);
					continue;
				}

				#endregion

				// Saving test errors
				for (int i = 0; i < outputLength; testErrors[snapIdx][i] = testExpected[snapIdx][i] - output[i++]) ;
			}

			neuroNet.LogOnStudyingEnd(errors, testErrors);
		}
	}

	public class NeuroNetResultsComparer : IEqualityComparer<double>
	{
		private readonly double _minDiff;

		public NeuroNetResultsComparer(double minDiff) => _minDiff = minDiff;
		public bool Equals(double x, double y) => Math.Abs(x - y) <= _minDiff;
		public int GetHashCode(double obj) => obj.GetHashCode();
	}
}