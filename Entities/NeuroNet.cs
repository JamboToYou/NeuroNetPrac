namespace NeuroNet.Entities
{
    public class NeuroNet
    {
        public double[][] Neurons { get; set; }
        public double[][,] Weights { get; set; }

        public NeuroNet(int[] neuronsCount)
        {
            Neurons = new double[neuronsCount.Length][];
            Weights = new double[neuronsCount.Length][,];

            for (int layerIdx = 0; layerIdx < neurons.Length; layerIdx++)
            {
                Neurons[layerIdx] = new double[neuronsCount[layerIdx]];
                Weights[layerIdx] = new double[neuronsCount[layerIdx]];
            }
        }
    }
}