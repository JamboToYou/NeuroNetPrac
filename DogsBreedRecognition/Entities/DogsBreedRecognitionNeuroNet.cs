using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Collections.Concurrent;

using NeuroNet.Entities;
using Common.Utils;
using ImageHelper;
using System.Runtime.Serialization.Formatters.Binary;

namespace DogsBreedRecognition.Entities
{
	public static class DogsBreedRecognitionNeuroNet
	{
		private static readonly int IMG_THUMB_WIDTH = 70;
		private static readonly int IMG_THUMB_HEIGHT = 70;
		private static readonly int NN_INPUT_SIZE = IMG_THUMB_WIDTH * IMG_THUMB_HEIGHT;
		private static readonly int NN_OUTPUT_SIZE = 120;
		private static readonly int[] NN_SIZE = new int[] { NN_INPUT_SIZE, 200, NN_OUTPUT_SIZE };
		private static readonly string TEACHING_DATA_PATH = Directory.GetCurrentDirectory() + @"\TeachingData\Images";
		private static readonly string NORMALIZED_DATA_PATH = Directory.GetCurrentDirectory() + @"\TeachingData\NormalizedData";

		private static int _currentEpoche { get; set; }
		private static int _countOfImagesByBreed { get; }
		private static int _epochesCount { get; }
		private static int _partOfcountForTests { get; }
		private static NeuroNetwork _neuroNet { get; set; }
		private static string[] _breedNames { get; }
		private static Dictionary<string, string[]> _breedImagesPathsByName { get; }
		private static Dictionary<string, int> _breedIndexesByName { get; }
		private static BinaryFormatter _bf { get; set; }

		private static string _normalizedSnapsFileName { get; }

		// TODO: use config files to configure NN
		static DogsBreedRecognitionNeuroNet()
		{
			_neuroNet = new NeuroNetwork(NN_SIZE);
			_breedImagesPathsByName = new Dictionary<string, string[]>();
			_breedIndexesByName = new Dictionary<string, int>();
			_countOfImagesByBreed = 30;
			_epochesCount = 20;
			_partOfcountForTests = 6;
			_breedNames = new string[NN_OUTPUT_SIZE];
			_normalizedSnapsFileName = NORMALIZED_DATA_PATH +
				$@"\{IMG_THUMB_WIDTH}x{IMG_THUMB_HEIGHT}-{NN_OUTPUT_SIZE}-{_countOfImagesByBreed}.dat";

			_bf = new BinaryFormatter();

			// TODO: add analytics
			_neuroNet.OnStudyingStart += () => Console.WriteLine($"[running]: Starting the epoche {_currentEpoche}");
			_neuroNet.OnStudyingEnd += (x, y) => Console.WriteLine($"[running]: End of the epoche {_currentEpoche}");
			_neuroNet.OnProcessingSnapEnd += (x, errs) => Console.WriteLine($"[running]: Finnished processing snap {x}\n[info][err]: \tSnaps: {errs.Sum()/errs.Length}");

			string name;
			var breedImagesPaths = Directory.GetDirectories(TEACHING_DATA_PATH);

			for (int pathIdx = 0; pathIdx < breedImagesPaths.Length; pathIdx++)
			{
				name = string.Join('-', breedImagesPaths[pathIdx].Split('\\').Last().Split('-').Skip(1))
						.Replace("_", " ").Capitalize();

				_breedNames[pathIdx] = name;
				_breedImagesPathsByName.Add(name, Directory.GetFiles(breedImagesPaths[pathIdx]));
				_breedIndexesByName.Add(name, pathIdx);
			}
		}

		public static string Execute(string filename)
		{
			_neuroNet = Load();
			var snap = ImageNormalizer.GetNormalizedInput(
				filename,
				IMG_THUMB_WIDTH,
				IMG_THUMB_HEIGHT
			);

			var result = _neuroNet.Execute(snap);
			return GetBreedFromOutput(result);
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

			for (_currentEpoche = 0; _currentEpoche < _epochesCount; _currentEpoche++)
			{
				(eduSnaps, eduExpected) = Utils.Randomize(eduSnaps, eduExpected);
				(testSnaps, testExpected) = Utils.Randomize(testSnaps, testExpected);

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

		private static NeuroNetwork Load()
		{
			var path = Directory.GetCurrentDirectory() + @"\NeuroNetworks";
			var fn = Directory.GetFiles(path).Last();
			NeuroNetState res = null;

			System.Console.WriteLine(fn);

			using (var fs = new FileStream(fn, FileMode.Open))
			{
				res = (NeuroNetState)_bf.Deserialize(fs);
			}

			var neuroNet = new NeuroNetwork(NN_SIZE);
			neuroNet.Weights = res.Weights;

			return neuroNet;
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
			double[] snap;
			var snaps = new List<double[]>();
			var results = new List<double[]>();

			// TODO: use parallel tasks
			foreach ((var breedName, var breedImagePaths) in _breedImagesPathsByName)
			{
				foreach (var breedImagePath in breedImagePaths.Take(_countOfImagesByBreed))
				{
					//tmp
					Console.WriteLine(breedImagePath);
					//!tmp
					result = new double[NN_OUTPUT_SIZE];
					result[_breedIndexesByName[breedName]] = 1;

					snap = ImageNormalizer.GetNormalizedInput(
						breedImagePath,
						IMG_THUMB_WIDTH,
						IMG_THUMB_HEIGHT
					);

					results.Add(result);
					snaps.Add(snap);
				}
			}
			var asnaps = snaps.ToArray();
			var aresults = results.ToArray();

			using (var fs = new FileStream(_normalizedSnapsFileName, FileMode.OpenOrCreate))
			{
				_bf.Serialize(fs, (asnaps, aresults));
			}

			return (asnaps, aresults);
		}

		private static string GetBreedFromOutput(double[] output)
		{
			var max = 0;
			var maxVal = .0;
			for (int i = 0; i < output.Length; i++)
			{
				if (output[i] > maxVal)
				{
					max = i;
					maxVal = output[i];
				}
			}

			return _breedNames[max];
		}
	}
}
