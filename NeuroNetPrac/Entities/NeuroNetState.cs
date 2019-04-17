using System;

namespace NeuroNet.Entities
{
	[Serializable]
	public class NeuroNetState
	{
		// public double[][] Neurons { get; set; }
		public double[][][] Weights { get; set; }
	}
}
