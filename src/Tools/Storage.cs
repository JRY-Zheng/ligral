using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System;


namespace Ligral.Tools
{
    public class Storage
    {
        public List<string> Columns { get; private set; }
        public List<List<double>> Data { get; private set; }
        private Regex columnRegex;
        private Regex headerRegex;
        private Regex doubleRegex;
        private Regex dataRegex;
        public Storage()
        {
            string columnString = @"[^,\s]+";
            columnRegex = new Regex(columnString);
            string doubleString = @"[+-]?\d+(\.\d+)?(e[+-]*\d+)?";
            doubleRegex = new Regex(doubleString);
            headerRegex = LineRegex(columnString);
            dataRegex = LineRegex(doubleString);
        }
        private Regex LineRegex(string item)
        {
            return new Regex($"\\s*{item}\\s*(,\\s*{item}\\s*)*");
        }
        public Storage(List<string> columns, List<List<double>> data) : this()
        {
            Columns = columns;
            Data = data;
        }
        public Storage(string fileName, bool hasHeader = false) : this()
        {
            LoadFile(fileName, hasHeader);
        }
        public void LoadFile(string fileName, bool hasHeader = false)
        {
            List<string> text = File.ReadAllLines(fileName).ToList();
            if (hasHeader)
            {
                if (text.Count == 0)
                {
                    throw new CSVFormatError("CSV file has no line, cannot read header");
                }
                Match headerMatch = headerRegex.Match(text[0]);
                if (!headerMatch.Success)
                {
                    throw new CSVFormatError("Invalid CSV header");
                }
                Columns = columnRegex.Matches(text[0]).OfType<Match>().Select(m => m.Value).ToList();
                text.RemoveAt(0);
            }
            if (text.Count == 0)
            {
                throw new CSVFormatError("CSV file has no data");
            }
            else
            {
                Data = new List<List<double>>();
            }
            foreach (string line in text)
            {
                Match dataMatch = dataRegex.Match(line);
                if (!dataMatch.Success)
                {
                    throw new CSVFormatError($"Data format error in line {Data.Count + (hasHeader ? 2 : 1)}: {line}");
                }
                List<double> row = doubleRegex.Matches(line).OfType<Match>().Select(m => double.Parse(m.Value)).ToList();
                if (row.Count != Columns.Count)
                {
                    throw new CSVFormatError($"Column number inconsistency in line {Data.Count + (hasHeader ? 2 : 1)}: {line}");
                }
                Data.Add(row);
            }
        }
        public void DumpFile(string fileName, bool writeHeader = false)
        {
            List<string> text = new List<string>();
            if (writeHeader)
            {
                text.Add(string.Join(", ", Columns));
            }
            text.AddRange(Data.ConvertAll(row => string.Join(", ", row.ConvertAll(IEqualityComparer => IEqualityComparer.ToString()))));
            File.WriteAllLines(fileName, text);
        }
        private void AddToList<T>(List<T> list, T item, int pos)
        {
            if (pos >= 0 && pos < list.Count)
            {
                list.Insert(pos, item);
            }
            else if (pos == list.Count || pos == -1)
            {
                list.Add(item);
            }
            else if (pos < -1 && -pos <= list.Count + 1)
            {
                list.Insert(Data.Count - pos - 1, item);
            }
            else
            {
                throw new InvalidOperationException($"Insert position {pos} is invalid");
            }
        }
        public void AddRow(List<double> row, int pos = -1)
        {
            if (row.Count != Columns.Count)
            {
                throw new CSVFormatError("Column number inconsistency");
            }
            AddToList(Data, row, pos);
        }
        public void AddColumn(string column, List<double> values, int pos = -1)
        {
            if (values.Count != Data.Count)
            {
                throw new CSVFormatError("row number inconsistency");
            }
            AddToList(Columns, column, pos);
            Data.Zip(values, (row, value) => { AddToList(row, value, pos); return 0; }).ToList();
        }
        private T GetItem<T>(List<T> list, int index)
        {
            if (index >= 0 && index < list.Count)
            {
                return list[index];
            }
            else if (index < 0 && -index <= list.Count)
            {
                return list[list.Count + index];
            }
            else
            {
                throw new InvalidOperationException($"Number {index} exceeds limits");
            }
        }
        public List<double> GetRow(int index)
        {
            return GetItem(Data, index);
        }
        public List<double> GetColumn(int index)
        {
            return Data.ConvertAll(row => GetItem(row, index));
        }
        public string GetColumnName(int index)
        {
            return GetItem(Columns, index);
        }
    }
}
