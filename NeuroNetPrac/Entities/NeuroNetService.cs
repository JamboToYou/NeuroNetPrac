using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using NeuroNet.Entities;
using Common.Utils;
using System.Text;

namespace NeuroNet.Entities
{
	public abstract class NeuroNetService
	{
		protected int _nnInputSize { get; set; }
		protected string _teachingDataPath { get; set; }
		protected string _normalizedDataPath { get; set; }
		protected int _nnOutputSize { get; set; }
		protected int[] _nnSize { get; set; }
		protected int _currentEpoche { get; set; }
		protected int _epochesCount { get; set; }
		protected int _partOfcountForTests { get; set; }
		protected bool _isStudied { get; set; }
		protected NeuroNetwork _neuroNet { get; set; }
		protected BinaryFormatter _bf { get; }
		protected abstract string _normalizedSnapsFileName { get; }
		protected static Func<double, double, double> Th = (x, y) => (Math.Exp(2 * x * y) - 1) / (Math.Exp(2 * x * y) + 1);
		protected static Func<double, double, double> ThDrv = (x, y) => (1 + x * x);

		public NeuroNetService()
		{
			_bf = new BinaryFormatter();
		}

		public virtual void LogEndOfEpoche(double[][] err, double[][] testErr)
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

		public NeuroNetwork Load()
		{
			NeuroNetState state;

			using (var fs = new FileStream(Directory.GetFiles(Directory.GetCurrentDirectory() + @"\NeuroNetworks").Last(), FileMode.Open))
			{
				state = (NeuroNetState)_bf.Deserialize(fs);
			}

			_neuroNet.Weights = state.Weights;

			return _neuroNet;
		}

		public abstract string Execute(string filename);

		public virtual void Study()
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

		protected void SaveNeuroNetwork()
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

		protected virtual (double[][], double[][]) GetNormalizedData()
		{
			double[][] snaps;
			double[][] results;
			if (Directory.GetFiles(_normalizedDataPath).Contains(_normalizedSnapsFileName))
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

		protected abstract (double[][], double[][]) Normalize();
	}
}