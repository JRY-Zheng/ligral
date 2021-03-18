/* Copyright (C) 2019-2021 Junruoyu Zheng. Home page: https://junruoyu-zheng.gitee.io/ligral

   Distributed under MIT license.
   See file LICENSE for detail or copy at https://opensource.org/licenses/MIT
*/

using System;
using System.Linq;
using System.Collections.Generic;
using MathNet.Numerics.LinearAlgebra;

namespace Ligral.Component 
{
    public static class MatrixIteration
    {
        public static double RankTolerant = 1e-10;
        public static List<double> ToList(this Matrix<double> matrix)
        {
            return new List<double>(matrix.ToRowMajorArray());
        }
        public static Matrix<T> ToColumnVector<T>(this List<T> list) where T : struct, IEquatable<T>, IFormattable
        {
            MatrixBuilder<T> m = Matrix<T>.Build;
            return m.DenseOfRowMajor(list.Count, 1, list);
        }
        public static string ToStringInLine(this Matrix<double> matrix)
        {
            var arrays = matrix.ToRowArrays();
            var strArr = new string[arrays.Length];
            for(int i=0; i<arrays.Length; i++)
            {
                strArr[i] = string.Join(" ", arrays[i]);
            }
            return $"({matrix.RowCount}x{matrix.ColumnCount})  " + string.Join("  ", strArr);
        }
        public static bool CheckShape(this Matrix<double> matrix, int rowNo, int colNo)
        {
            return rowNo == matrix.RowCount && colNo == matrix.ColumnCount;
        }
        public static bool CheckShape(this Matrix<double> matrix, Matrix<double> other)
        {
            return other.RowCount == matrix.RowCount && other.ColumnCount == matrix.ColumnCount;
        }
        public static int Count(this Matrix<double> matrix)
        {
            return matrix.RowCount * matrix.ColumnCount;
        }
        public static int TolerantRank(this Matrix<double> matrix)
        {
            var factor = matrix.Svd();
            return factor.S.ToArray().Count(x => x>RankTolerant || x<-RankTolerant);
        }
        public static Matrix<double> Broadcast(this Matrix<double> left, Matrix<double> right, Func<double, double, double> func)
        {
            if (left.IsScalar())
            {
                return right.Map(x => func(left[0,0], x));
            }
            else if (right.IsScalar())
            {
                return left.Map(x => func(x, right[0,0]));
            }
            else if (left.IsColumnVector())
            {
                if (left.RowCount != right.RowCount)
                {
                    throw new ArgumentOutOfRangeException($"cannot broadcast matrix {left.ShapeString()} to {right.ShapeString()}");
                }
                var result = Matrix<double>.Build.DenseOfMatrix(right);
                for (int i = 0; i < result.RowCount; i++)
                {
                    result.SetRow(i, result.Row(i).Map(x => func(left[i, 0], x)));
                }
                return result;
            }
            else if (right.IsColumnVector())
            {
                if (left.RowCount != right.RowCount)
                {
                    throw new ArgumentOutOfRangeException($"cannot broadcast matrix {left.ShapeString()} to {right.ShapeString()}");
                }
                var result = Matrix<double>.Build.DenseOfMatrix(left);
                for (int i = 0; i < result.RowCount; i++)
                {
                    result.SetRow(i, left.Row(i).Map(x => func(x, right[i, 0])));
                }
                return result;
            }
            else if (left.IsRowVector())
            {
                if (left.ColumnCount != right.ColumnCount)
                {
                    throw new ArgumentOutOfRangeException($"cannot broadcast matrix {left.ShapeString()} to {right.ShapeString()}");
                }
                var result = Matrix<double>.Build.DenseOfMatrix(right);
                for (int i = 0; i < result.ColumnCount; i++)
                {
                    result.SetColumn(i, result.Column(i).Map(x => func(left[0, i], x)));
                }
                return result;
            }
            else if (right.IsRowVector())
            {
                if (left.ColumnCount != right.ColumnCount)
                {
                    throw new ArgumentOutOfRangeException($"cannot broadcast matrix {left.ShapeString()} to {right.ShapeString()}");
                }
                var result = Matrix<double>.Build.DenseOfMatrix(left);
                for (int i = 0; i < result.ColumnCount; i++)
                {
                    result.SetColumn(i, left.Column(i).Map(x => func(x, right[0, i])));
                }
                return result;
            }
            else return left.Map2(func, right);
        }
        public static (int, int) BroadcastShape(int xRow, int xCol, int yRow, int yCol)
        {
            if (xRow == 1 && xCol == 1)
            {
                return (yRow, yCol);
            }
            else if (yRow == 1 && yCol == 1)
            {
                return (xRow, xCol);
            }
            else if (xCol == 1)
            {
                if (xRow != yRow)
                {
                    throw new ArgumentOutOfRangeException($"cannot broadcast matrix {xRow}x{xCol} to {yRow}x{yCol}");
                }
                return (yRow, yCol);
            }
            else if (yCol == 1)
            {
                if (xRow != yRow)
                {
                    throw new ArgumentOutOfRangeException($"cannot broadcast matrix {xRow}x{xCol} to {yRow}x{yCol}");
                }
                return (xRow, xCol);
            }
            else if (xRow == 1)
            {
                if (xCol != yCol)
                {
                    throw new ArgumentOutOfRangeException($"cannot broadcast matrix {xRow}x{xCol} to {yRow}x{yCol}");
                }
                return (yRow, yCol);
            }
            else if (yRow == 1)
            {
                if (xCol !=yCol)
                {
                    throw new ArgumentOutOfRangeException($"cannot broadcast matrix {xRow}x{xCol} to {yRow}x{yCol}");
                }
                return (xRow, xCol);
            }
            else
            {
                if (xCol != yCol || xRow != yRow)
                {
                    throw new ArgumentOutOfRangeException($"cannot broadcast matrix {xRow}x{xCol} to {yRow}x{yCol}");
                }
                return (yRow, yCol);
            }
        }
        public static void Apply(this Matrix<double> matrix, Action<double> action)
        {
            for (int r=0; r < matrix.RowCount; r++)
            {
                for (int c=0; c < matrix.ColumnCount; c++)
                {
                    action(matrix[r, c]);
                }
            }
        }
        public static void Apply2(this Matrix<double> matrix, Matrix<double> other, Action<double, double> action)
        {
            for (int r=0; r < matrix.RowCount; r++)
            {
                for (int c=0; c < matrix.ColumnCount; c++)
                {
                    action(matrix[r, c], other[r, c]);
                }
            }
        }
        public static void Apply2<T>(this Matrix<double> matrix, List<T> other, Action<double, T> action)
        {
            for (int r=0; r < matrix.RowCount; r++)
            {
                for (int c=0; c < matrix.ColumnCount; c++)
                {
                    action(matrix[r, c], other[r*matrix.ColumnCount+c]);
                }
            }
        }
    }
}