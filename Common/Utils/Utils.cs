using System;
using System.Collections.Generic;

namespace Common.Utils
{
	public static class Utils
	{
		public static double[] Randomize(double[] array)
		{
			double tmp;
			var result = new double[array.Length];
			var vals = new List<double>(array);
			var rnd = new Random();

			for (int i = 0; i < result.Length; i++)
			{
				tmp = vals[rnd.Next(vals.Count)];
				result[i] = tmp;
				vals.Remove(tmp);
			}

			return result;
		}
	}
}
