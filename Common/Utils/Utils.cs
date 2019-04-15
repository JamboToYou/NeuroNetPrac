using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Common.Utils
{
	public static class Utils
	{
		private static readonly Random RND = new Random();
		public static T[] Randomize<T>(T[] array)
		{
			T tmp;
			var result = new T[array.Length];
			var vals = new List<T>(array);

			for (int i = 0; i < result.Length; i++)
			{
				tmp = vals[RND.Next(vals.Count)];
				result[i] = tmp;
				vals.Remove(tmp);
			}

			return result;
		}
		public static (T[], R[]) Randomize<T, R>(T[] arr1, R[] arr2)
		{
			int min = arr1.Length > arr2.Length ? arr2.Length : arr1.Length;

			T tmp1;
			R tmp2;
			int rnd;
			var vals1 = new List<T>(arr1).Take(min).ToList();
			var vals2 = new List<R>(arr2).Take(min).ToList();
			var result1 = new T[min];
			var result2 = new R[min];

			for (int i = 0; i < min; i++)
			{
				rnd = RND.Next(vals1.Count);
				tmp1 = vals1[rnd];
				tmp2 = vals2[rnd];

				result1[i] = tmp1;
				result2[i] = tmp2;

				vals1.Remove(tmp1);
				vals2.Remove(tmp2);
			}

			return (result1, result2);
		}
		public static string Capitalize(this string row)
			=> Regex.Replace(row.ToLower(), @"\b[a-z]", m => m.Value.ToUpper());
	}
}
