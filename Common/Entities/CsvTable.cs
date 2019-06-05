using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;

namespace Common.Entities
{
	public class CsvRecord : Dictionary<string, string>
	{
	}
	public class CsvTable
	{
		public List<string> ColumnTitles { get; }
		public string[][] Data { get; }

		public CsvTable(List<string> colTitles, string[][] data)
		{
			ColumnTitles = colTitles;
			Data = data;
		}

		public IEnumerable<CsvRecord> GetRecords()
		{
			foreach (var record in Data)
			{
				var result = new CsvRecord();
				for (int i = 0; i < ColumnTitles.Count; i++)
					result.Add(ColumnTitles[i], record[i]);
				
				yield return result;
			}
		}
	}
}