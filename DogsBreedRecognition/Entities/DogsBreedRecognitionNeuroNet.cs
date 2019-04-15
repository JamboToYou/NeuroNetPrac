using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Collections.Concurrent;

using NeuroNet.Entities;
using Common.Utils;
using ImageHelper;

namespace DogsBreedRecognition.Entities
{
	public static class DogsBreedRecognitionNeuroNet
	{
		private static readonly int IMG_THUMB_WIDTH = 200;
		private static readonly int IMG_THUMB_HEIGHT = 200;
		private static readonly int NN_INPUT_SIZE = IMG_THUMB_WIDTH * IMG_THUMB_HEIGHT;
		private static readonly int NN_OUTPUT_SIZE = 120;
		private static readonly int[] NN_SIZE = new int[] { NN_INPUT_SIZE, 300, 200, NN_OUTPUT_SIZE };
		private static readonly string TEACHING_DATA_PATH = Directory.GetCurrentDirectory() + @"\TeachingData\Images";

		private static int _countOfImagesByBreed { get; }
		private static int _epochesCount { get; }
		private static int _partOfcountForTests { get; }
		private static NeuroNetwork _neuroNet { get; }
		private static Dictionary<string, string[]> _breedImagesPathsByNames { get; }
		private static Dictionary<string, int> _breedIndexesByNames { get; }

		// TODO: use config files to configure NN
		static DogsBreedRecognitionNeuroNet()
		{
			_neuroNet = new NeuroNetwork(NN_SIZE);
			_breedImagesPathsByNames = new Dictionary<string, string[]>();
			_breedIndexesByNames = new Dictionary<string, int>();
			_countOfImagesByBreed = 30;
			_epochesCount = 50;
			_partOfcountForTests = 6;

			string name;
			var breedImagesPaths = Directory.GetDirectories(TEACHING_DATA_PATH);

			for (int pathIdx = 0; pathIdx < breedImagesPaths.Length; pathIdx++)
			{
				name = breedImagesPaths[pathIdx].Split('\\').Last()
						.Split('-').Last()
						.Replace("_", " ").Capitalize();

				_breedImagesPathsByNames.Add(name, Directory.GetFiles(breedImagesPaths[pathIdx]));
				_breedIndexesByNames.Add(name, pathIdx);
			}
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

			for (int ep = 0; ep < _epochesCount; ep++)
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
		}

		private static (double[][], double[][]) GetNormalizedData()
		{
			double[] result;
			double[] snap;
			var snaps = new List<double[]>();
			var results = new List<double[]>();

			// TODO: use parallel tasks
			foreach ((var breedName, var breedImagePaths) in _breedImagesPathsByNames)
			{
				foreach (var breedImagePath in breedImagePaths.Take(_countOfImagesByBreed))
				{
					result = new double[NN_OUTPUT_SIZE];
					result[_breedIndexesByNames[breedName]] = 1;

					snap = ImageNormalizer.GetNormalizedInputs(
						breedImagePath,
						IMG_THUMB_WIDTH,
						IMG_THUMB_HEIGHT
					);

					results.Add(result);
					snaps.Add(snap);
				}
			}

			return (snaps.ToArray(), results.ToArray());
		}
	}
}
