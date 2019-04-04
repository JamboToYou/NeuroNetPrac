using System;

namespace NeuroNet.Entities
{
    public static class NeuroNetWorker
    {
        public static double[] Execute(this NeuroNet neuroNet, double[] snaps)
        {
            var neurons = neuroNet.Neurons;
            var weights = neuroNet.Weights;
            var result = new double[];

            if (snaps[i].Length != neurons[0].Length)
            {
                throw new ArgumentException("[warn]: Wrong count of values in snap {0}");
            }

            neurons[0] = snaps;

            for (int i = 1; i < neurons.Length; i++)
            {
                for (int j = 0; j < neurons[i].Length; j++)
                {
                    neurons[i][j] = ExecuteOne(neurons[i - 1], weights[i][j], neuroNet.ActivationFunc);
                }
            }
        }

        private static double ExecuteOne(double[] prevLayer, double[] weights, Func<double, double> activationFunc)
        {
            var sum = 0;

            for (int i = 0; i < prevLayer.Length; i++)
                sum += prevLayer[i] * weights[i];

            return activationFunc(sum);
        }
    }
}