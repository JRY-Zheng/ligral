/* Copyright (C) 2019-2021 Junruoyu Zheng. Home page: https://junruoyu-zheng.gitee.io/ligral

   Distributed under MIT license.
   See file LICENSE for detail or copy at https://opensource.org/licenses/MIT
*/

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
        public static Matrix<double> ToColumnVector(this List<double> list)
        {
            MatrixBuilder<double> m = Matrix<double>.Build;
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
    }
}