using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Common.Entities;

namespace Common.Utils
{
	public static class ParsingUtils
	{
		public static async Task<CsvTable> ParseCsvAsync(string fileName, char separator = ',')
		{
			var columnTitles = new List<string>();
			var rows = new List<string[]>();
			string tmpLine;

			using (var fs = new FileStream(fileName, FileMode.Open))
			using (var sr = new StreamReader(fs))
			{
				columnTitles.AddRange(sr.ReadLine().Split(separator));

				while (!sr.EndOfStream)
				{
					tmpLine = await sr.ReadLineAsync();
					if (tmpLine.Length != columnTitles.Count)
						continue;

					rows.Add(tmpLine.Split(separator));
				}
			}

			return new CsvTable(columnTitles, rows.ToArray());
		}

		public static void ParseCsvByCallback(string path, Action<string> operateTitles, Action<string> parserCallback)
		{
			using (var fs = new FileStream(path, FileMode.Open))
			using (var sr = new StreamReader(fs))
			{
				operateTitles(sr.ReadLine());
				while(!sr.EndOfStream)
				{
					parserCallback(sr.ReadLine());
				}
			}
		}
	}
}