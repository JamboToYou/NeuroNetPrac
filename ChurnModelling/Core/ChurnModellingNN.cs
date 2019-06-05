using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Common.Entities;
using Common.Utils;
using NeuroNet.Entities;

namespace ChurnModelling.Core
{
	public class ChurnModellingNN : NeuroNetService
	{
		private readonly static string _outputTitle = "Exited";
		private readonly static List<string> _notInputColumns = new List<string> { "RowNumber", "CustomerId", _outputTitle };
		private List<string> _titles { get; set; }

		protected override string _normalizedSnapsFileName => $"{_nnInputSize}-{_outputTitle}.dat";

		public ChurnModellingNN()
		{
			_nnInputSize = 11;
			_nnOutputSize = 1;

			_teachingDataPath = Directory.GetCurrentDirectory() + @"\Data";
			_normalizedDataPath = _teachingDataPath + @"\NormalizedData";

			_nnSize = new int[] { _nnInputSize, 5, _nnOutputSize };

			_neuroNet = new NeuroNetwork(_nnSize);

			_epochesCount = 100;
			_partOfcountForTests = 2;

			_neuroNet.OnStudyingStart += () => Console.WriteLine($"[running]: Starting the epoche {_currentEpoche}");
			_neuroNet.OnStudyingEnd += (x, y) => Console.WriteLine($"[running]: End of the epoche {_currentEpoche}");
			_neuroNet.OnProcessingSnapEnd += (x, errs, testErrs) => Console.WriteLine($"[running]: Finnished processing snap {x}\n[info][err]: \tSnaps: {errs.Sum() / errs.Length}\tTests: {testErrs.Sum() / testErrs.Length}");
		}
		public override string Execute(string filename)
		{
			_neuroNet = Load();

			//tmp
			using (var fs = new FileStream(Directory.GetCurrentDirectory() + @"\input.csv", FileMode.Open))
			using (var sr = new StreamReader(fs))
			{
				sr.ReadLine();
				_titles = sr.ReadLine().Split(',').ToList();
			}
			//~tmp

			var input = GetNormalizedInputRecord(File.ReadAllLines(filename)[0].Split(','));

			var result = _neuroNet.Execute(input);

			return result[0].ToString();
		}

		protected override (double[][], double[][]) Normalize()
		{
			double[] input, output;
			var path = _normalizedDataPath + @"\Churn_Modelling.csv";

			var inputs = new List<double[]>();
			var outputs = new List<double[]>();

			ParsingUtils.ParseCsvByCallback(
				path,
				title => _titles = title.Split(',').ToList(),
				record =>
				{
					(input, output) = GetNormalizedRecord(record.Split(','));
					inputs.Add(input);
					outputs.Add(output);
				}
			);

			return (inputs.ToArray(), outputs.ToArray());
		}

		private (double[], double[]) GetNormalizedRecord(string[] rawInputRecord)
		{
			var input = new List<double>();
			var output = new List<double>();

			for (int i = 0; i < _titles.Count; i++)
			{
				if (!_notInputColumns.Contains(_titles[i]))
					input.Add(NormalizeValue(_titles[i], rawInputRecord[i]));
				else if (_titles[i] == _outputTitle)
					output.Add(NormalizeValue(_titles[i], rawInputRecord[i]));
			}

			return (input.ToArray(), output.ToArray());
		}

		private double[] GetNormalizedInputRecord(string[] rawInputRecord) => GetNormalizedRecord(rawInputRecord).Item1;

		private double NormalizeRange(double value, double min, double max) => (value - min) / (max - min);
		private double NormalizeValue(string title, string value)
		{
			var countries = new List<string>{ "France", "Spain", "Germany" };
			switch (title)
			{
				case "RowNumber":
					return NormalizeRange(int.Parse(value), 1, 10000);
				case "CustomerId":
					return NormalizeRange(int.Parse(value), 15565701, 15815690);
				case "Surname":
					return 1 / value.GetHashCode();
				case "CreditScore":
					return NormalizeRange(int.Parse(value), 350, 850);
				case "Geography":
					return (countries.IndexOf(value) + 1) / 3;
				case "Gender":
					return value == "Female" ? 0 : 1;
				case "Age":
					return NormalizeRange(int.Parse(value), 18, 92);
				case "Tenure":
					return NormalizeRange(int.Parse(value), 0, 10);
				case "Balance":
					return NormalizeRange(double.Parse(value.Replace('.', ',')), 0, 250898.09);
				case "NumOfProducts":
					return NormalizeRange(int.Parse(value), 1, 4);
				case "EstimatedSalary":
					return NormalizeRange(double.Parse(value.Replace('.', ',')), 11.58, 199992.48);
				case "HasCrCard":
				case "IsActiveMember":
				case "Exited":
					return int.Parse(value);
				default:
					throw new ArgumentException("[error]: There`s no column with given title");
			}
		}
	}
}