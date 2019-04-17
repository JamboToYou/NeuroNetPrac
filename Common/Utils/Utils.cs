using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Common.Utils
{
	public static class Utils
	{
		private static readonly Random RND = new Random();
		private static int[] GetRandomizedRange(int count)
		{
			var result = new int[count];
			for (int i = 0; i < count; result[i] = i++);
			return result.OrderBy(x => RND.Next()).ToArray();
		}
		private static T[] Shuffle<T>(T[] array, int[] indexes)
		{
			var result = new T[array.Length];

			for (int i = 0; i < result.Length; i++)
				result[i] = array[indexes[i]];

			return result;
		}
		public static T[] Randomize<T>(T[] array)
			=> Shuffle(array, GetRandomizedRange(array.Length));
		public static (T[], R[]) Randomize<T, R>(T[] arr1, R[] arr2)
		{
			var min = arr1.Length > arr2.Length ? arr2.Length : arr1.Length;
			var indexes = GetRandomizedRange(min);
			arr1 = arr1.Take(min).ToArray();
			arr2 = arr2.Take(min).ToArray();

			return (Shuffle(arr1, indexes),
					Shuffle(arr2, indexes));
		}
		public static string Capitalize(this string row)
			=> Regex.Replace(row.ToLower(), @"\b[a-z]", m => m.Value.ToUpper());
	}
}
