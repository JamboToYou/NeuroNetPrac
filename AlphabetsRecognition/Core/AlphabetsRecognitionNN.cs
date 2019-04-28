using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using NeuroNet.Entities;
using Common.Utils;
using ImageHelper;
using System.Text;

namespace AlphabetsRecognition.Core
{
	public static class AlphabetsRecognitionNN
	{
		private static readonly int IMG_THUMB_WIDTH = 28;
		private static readonly int IMG_THUMB_HEIGHT = 28;
		private static readonly int NN_INPUT_SIZE = IMG_THUMB_WIDTH * IMG_THUMB_HEIGHT;
		private static readonly string TEACHING_DATA_PATH = Directory.GetCurrentDirectory() + @"\TeachingData\handwritten_data_785.csv";
		private static readonly string NORMALIZED_DATA_PATH = Directory.GetCurrentDirectory() + @"\TeachingData\NormalizedData";
		private static readonly char[] ALPHABETS = new char[] { 'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I', 'G', 'K', 'L', 'M', 'N', 'O', 'P', 'Q', 'R', 'S', 'T', 'U', 'V', 'W', 'X', 'Y', 'Z' };
		// private static readonly char[] ALPHABETS = new char[] { 'A' };
		private static readonly int NN_OUTPUT_SIZE = ALPHABETS.Length;
		private static readonly int[] NN_SIZE = new int[] { NN_INPUT_SIZE, 15, 5,  NN_OUTPUT_SIZE };

		private static int _currentEpoche { get; set; }
		private static int _epochesCount { get; }
		private static int _partOfcountForTests { get; }
		private static int _countOfAlphabetsByType { get; }
		private static bool _isStudied { get; set; }
		private static NeuroNetwork _neuroNet { get; set; }
		private static BinaryFormatter _bf { get; set; }
		private static string _normalizedSnapsFileName { get; }
		private static Func<double, double, double> Th = (x, y) => (Math.Exp(2 * x * y) - 1) / (Math.Exp(2 * x * y) + 1);
		private static Func<double, double, double> ThDrv = (x, y) => (1 + x * x);

		// TODO: use config files to configure NN
		static AlphabetsRecognitionNN()
		{
			// _neuroNet = new NeuroNetwork(NN_SIZE, Th, ThDrv, speed: .4);
			_neuroNet = new NeuroNetwork(NN_SIZE, activationParam: 0.01);
			_epochesCount = 150;
			_partOfcountForTests = 2;
			_countOfAlphabetsByType = 201;
			_normalizedSnapsFileName = NORMALIZED_DATA_PATH +
				$@"\{IMG_THUMB_WIDTH}x{IMG_THUMB_HEIGHT}-{NN_OUTPUT_SIZE}.dat";

			_bf = new BinaryFormatter();

			// TODO: add analytics
			_neuroNet.OnStudyingStart += () => Console.WriteLine($"[running]: Starting the epoche {_currentEpoche}");
			_neuroNet.OnStudyingEnd += (x, y) => Console.WriteLine($"[running]: End of the epoche {_currentEpoche}");
			_neuroNet.OnProcessingSnapEnd += (x, errs, testErrs) => Console.WriteLine($"[running]: Finnished processing snap {x}\n[info][err]: \tSnaps: {errs.Sum() / errs.Length}\tTests: {testErrs.Sum() / testErrs.Length}");
			// _neuroNet.OnStudyingEnd += LogEndOfEpoche;
		}

		public static void LogEndOfEpoche(double[][] err, double[][] testErr)
		{
			if (_currentEpoche > 3)
			{
				err = err.Select(el => el.Select(x => Math.Pow(1 + x, 2)).ToArray()).ToArray();
				testErr = testErr.Select(el => el.Select(x => Math.Pow(1 + x, 2)).ToArray()).ToArray();
				var avgSnapErrors = err.Select(el => el.Sum() / el.Length);
				var avgTestErrors = testErr.Select(el => el.Sum() / el.Length);
				var ase = avgSnapErrors.Sum() / avgSnapErrors.Count();
				var ate = avgTestErrors.Sum() / avgTestErrors.Count();

				var diff = Math.Abs(ase - ate);
				if (diff > 0.3)
					_isStudied = true;
				
				Console.WriteLine($"[info]: Difference between teaching and testing errors: {diff}");
			}
		}

		public static NeuroNetwork Load()
		{
			NeuroNetState state;

			using (var fs = new FileStream(Directory.GetFiles(Directory.GetCurrentDirectory() + @"\NeuroNetworks").Last(), FileMode.Open))
			{
				state = (NeuroNetState)_bf.Deserialize(fs);
			}

			_neuroNet.Weights = state.Weights;

			return _neuroNet;
		}

		public static string Execute(string filename)
		{
			var sb = new StringBuilder();
			_neuroNet = Load();
			var snap = ImageNormalizer.GetNormalizedBWInput(
				filename,
				IMG_THUMB_WIDTH,
				IMG_THUMB_HEIGHT
			);
			// double[] snap;

			// using (var fs = new FileStream(TEACHING_DATA_PATH, FileMode.Open))
			// using (var sr = new StreamReader(fs))
			// {
			// 	snap = sr.ReadLine().Split(',').Skip(1).Select(x => double.Parse(x)).ToArray();
			// }

			var result = _neuroNet.Execute(snap);
			sb.AppendLine($"Result: {GetAlphabetFromOutput(result)}");
			for (int i = 0; i < result.Length; i++)
			{
				sb.AppendLine($"[{ALPHABETS[i]}]: {result[i]}");
			}

			return sb.ToString();
		}

		private static char GetAlphabetFromOutput(double[] result)
		{
			var mx = 0;
			var mxv = .0;

			for (int i = 0; i < result.Length; i++)
			{
				if (result[i] > mxv)
				{
					mxv = result[i];
					mx = i;
				}
			}

			return ALPHABETS[mx];
		}

		public static void Study()
		{
			(var snaps, var expected) = GetNormalizedData();
			(snaps, expected) = Utils.Randomize(snaps, expected);

			var testCount = snaps.Length / _partOfcountForTests;

			var eduSnaps = snaps.Take(snaps.Length - testCount).ToArray();
			var eduExpected = expected.Take(expected.Length - testCount).ToArray();

			var testSnaps = snaps.TakeLast(testCount).ToArray();
			var testExpected = expected.TakeLast(testCount).ToArray();
			_isStudied = false;

			for (_currentEpoche = 0; /*!_isStudied && */_currentEpoche < _epochesCount; _currentEpoche++)
			{
				(eduSnaps, eduExpected) = Utils.Randomize(eduSnaps, eduExpected);
				 

				_neuroNet.Study(
					eduSnaps,
					eduExpected,
					testSnaps,
					testExpected
				);
			}

			SaveNeuroNetwork();
		}

		private static void SaveNeuroNetwork()
		{
			var fn = Directory.GetCurrentDirectory() + @"\NeuroNetworks\" + DateTimeOffset.Now.Ticks + ".dat";
			var nn = new NeuroNetState
			{
				Weights = _neuroNet.Weights
			};

			using (var fs = new FileStream(fn, FileMode.OpenOrCreate))
			{
				_bf.Serialize(fs, nn);
			}
			Console.WriteLine($"Network saved at \n{fn}");
		}

		private static (double[][], double[][]) GetNormalizedData()
		{
			double[][] snaps;
			double[][] results;
			if (Directory.GetFiles(NORMALIZED_DATA_PATH).Contains(_normalizedSnapsFileName))
			{
				using (var fs = new FileStream(_normalizedSnapsFileName, FileMode.Open))
				{
					(snaps, results) = ((double[][], double[][]))_bf.Deserialize(fs);
				}

				Console.WriteLine("[info]: Using saved normalized data");
			}
			else
			{
				(snaps, results) = Normalize();
			}

			return (snaps, results);
		}

		private static (double[][], double[][]) Normalize()
		{
			double[] result;
			var snaps = new List<double[]>();
			var results = new List<double[]>();
			var csv = GetCsvAsArray();

			// TODO: use parallel tasks
			foreach (var item in csv)
			{
				result = new double[NN_OUTPUT_SIZE];
				result[item[0]] = 1;
				snaps.Add(item.Skip(1).Select(x => x / 255.0).ToArray());
				results.Add(result);
				//tmp
				Console.WriteLine(item[0]);
				//~tmp
			}

			var sn = snaps.ToArray();
			var rs = results.ToArray();

			using (var fs = new FileStream(_normalizedSnapsFileName, FileMode.OpenOrCreate))
			{
				_bf.Serialize(fs, (sn, rs));
			}

			return (sn, rs);
		}

		private static int[][] GetCsvAsArray()
		{
			int[] pxs;
			int currType = 0;
			int currTypeCnt = 0;
			var result = new List<int[]>();

			using (var fs = new FileStream(TEACHING_DATA_PATH, FileMode.Open))
			using (var sr = new StreamReader(fs))
			{
				while (!sr.EndOfStream)
				{
					// TODO: optimize
					pxs = sr.ReadLine().Split(',').Select(x => int.Parse(x)).ToArray();

					if (currType < NN_OUTPUT_SIZE)
					{
						if (pxs[0] == currType && currTypeCnt++ < _countOfAlphabetsByType)
						{
							result.Add(pxs);
						}
						else if (pxs[0] != currType++ && currType < NN_INPUT_SIZE)
						{
							result.Add(pxs);
							currTypeCnt = 1;
						}
					}
					else
						break;
				}
			}
			return result.ToArray();
		}
	}
}