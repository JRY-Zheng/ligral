/* Copyright (C) 2019-2021 Junruoyu Zheng. Home page: https://junruoyu-zheng.gitee.io/ligral

   Distributed under MIT license.
   See file LICENSE for detail or copy at https://opensource.org/licenses/MIT
*/

﻿using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using MathNet.Numerics.LinearAlgebra;


namespace Ligral.Tools
{
    public class Storage
    {
        public List<string> Columns { get; private set; }
        public List<List<double>> Data { get; private set; }
        public int ColumnCount { get => Columns!=null?Columns.Count:Data!=null && Data.Count>=1?Data[0].Count:-1; }
        private Regex columnRegex;
        private Regex headerRegex;
        private Regex doubleRegex;
        private Regex dataRegex;
        private Logger logger = new Logger("Storage");
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
        public Storage(Matrix<double> matrix) : this()
        {
            Data = new List<List<double>>();
            foreach (var vector in matrix.ToRowArrays())
            {
                Data.Add(vector.ToList());
            }
        }
        public Matrix<double> ToMatrix()
        {
            var mat = Matrix<double>.Build.Dense(Data.Count, ColumnCount);
            for (int i=0; i<Data.Count; i++)
            {
                mat.SetRow(i, Data[i].ToArray());
            }
            return mat;
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
                    throw logger.Error(new CSVFormatError("CSV file has no line, cannot read header"));
                }
                Match headerMatch = headerRegex.Match(text[0]);
                if (!headerMatch.Success)
                {
                    throw logger.Error(new CSVFormatError("Invalid CSV header"));
                }
                Columns = columnRegex.Matches(text[0]).OfType<Match>().Select(m => m.Value).ToList();
                text.RemoveAt(0);
            }
            if (text.Count == 0)
            {
                throw logger.Error(new CSVFormatError("CSV file has no data"));
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
                    throw logger.Error(new CSVFormatError($"Data format error in line {Data.Count + (hasHeader ? 2 : 1)}: {line}"));
                }
                List<double> row = doubleRegex.Matches(line).OfType<Match>().Select(m => double.Parse(m.Value)).ToList();
                if (row.Count != ColumnCount)
                {
                    throw logger.Error(new CSVFormatError($"Column number inconsistency in line {Data.Count + (hasHeader ? 2 : 1)}: {line}"));
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
                throw logger.Error(new LigralException($"Insert position {pos} is invalid"));
            }
        }
        public void AddRow(List<double> row, int pos = -1)
        {
            if (row.Count != ColumnCount)
            {
                throw logger.Error(new CSVFormatError("Column number inconsistency"));
            }
            AddToList(Data, row, pos);
        }
        public void AddColumn(string column, List<double> values, int pos = -1)
        {
            if (Data == null)
            {
                Data = values.ConvertAll(item => new List<double>(){ item });
                Columns = new List<string>(){ column };
            }
            else if (values.Count != Data.Count)
            {
                throw logger.Error(new CSVFormatError("row number inconsistency"));
            }
            else
            {
                AddToList(Columns, column, pos);
                Data.Zip(values, (row, value) => { AddToList(row, value, pos); return 0; }).ToList();
            }
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
                throw logger.Error(new LigralException($"Number {index} exceeds limits"));
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
        public double Interpolate2D(double colVal, double rowVal)
        {
            List<double> before = Data.Skip(1).LastOrDefault(row => row[0] < colVal);
            List<double> after = Data.Skip(1).FirstOrDefault(row => row[0] > colVal);
            List<double> current = Data.Skip(1).FirstOrDefault(row => row[0] == colVal);
            if (current == null && before != null && after != null)
            {
                // Results.Add(before.Data+(after.Data-before.Data)/(after.Time-before.Time)*(time-before.Time));
                double tb = before[0];
                double ta = after[0];
                current = before.Zip(after, (b, a) => b + (a - b) / (ta - tb) * (colVal - tb)).ToList();
            }
            else if (before == null && after != null)
            {
                current = after.ToList();
            }
            else if (before != null && after == null)
            {
                current = before.ToList();
            }
            else
            {
                throw new System.ArgumentException($"Invalid interpolation input at col value {colVal}");
            }
            int left = Data[0].Skip(1).ToList().FindLastIndex(item => item < rowVal) + 1;
            int right = Data[0].Skip(1).ToList().FindLastIndex(item => item > rowVal) + 1;
            int mid = Data[0].Skip(1).ToList().FindLastIndex(item => item == rowVal) + 1;
            if (mid > 0)
            {
                return current[mid];
            }
            else if (left > 0 && right > 0)
            {
                double tb = Data[0][left];
                double ta = Data[0][right];
                double b = current[left];
                double a = current[right];
                return b + (a - b) / (ta - tb) * (rowVal - tb);
            }
            else if (left > 0 && right <= 0)
            {
                return current[left];
            }
            else if (left <= 0 && right > 0)
            {
                return current[right];
            }
            else
            {
                throw new System.ArgumentException($"Invalid interpolation input at row value {rowVal}");
            }
        }
        public List<double> ColumnInterpolate(double val)
        {
            return ColumnInterpolate(val, 0, ColumnCount-1);
        }
        public List<double> ColumnInterpolate(double val, int skip, int take)
        {
            List<double> before = Data.FindLast(row => row[0] < val);
            List<double> after = Data.Find(row => row[0] > val);
            List<double> current = Data.Find(row => row[0] == val);
            if (current != null)
            {
                return current;
            }
            else if (before != null && after != null)
            {
                // Results.Add(before.Data+(after.Data-before.Data)/(after.Time-before.Time)*(time-before.Time));
                double tb = before[0];
                double ta = after[0];
                return before.Skip(skip+1).Take(take).Zip(after.Skip(skip+1).Take(take), (b, a) => b + (a - b) / (ta - tb) * (val - tb)).ToList();
            }
            else if (before == null && after != null)
            {
                return after.Skip(skip+1).Take(take).ToList();
            }
            else if (before != null && after == null)
            {
                return before.Skip(skip+1).Take(take).ToList();
            }
            else
            {
                throw new System.ArgumentException($"Invalid interpolation input at value {val}");
            }
        }
    }
}
