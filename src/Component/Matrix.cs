/* Copyright (C) 2019-2021 Junruoyu Zheng. Home page: https://junruoyu-zheng.gitee.io/ligral

   Distributed under MIT license.
   See file LICENSE for detail or copy at https://opensource.org/licenses/MIT
*/

using System;
using MathNet.Numerics.LinearAlgebra;

namespace Ligral.Component 
{
    public static class Matrix
    {
        public static bool IsScalar(this Matrix<double> matrix)
        {
            return matrix.ColumnCount==1 && matrix.RowCount==1;
        }
        public static bool IsVector(this Matrix<double> matrix)
        {
            return matrix.ColumnCount==1 || matrix.RowCount==1;
        }
        public static bool IsColumnVector(this Matrix<double> matrix)
        {
            return matrix.ColumnCount==1;
        }
        public static bool IsRowVector(this Matrix<double> matrix)
        {
            return matrix.RowCount==1;
        }
        public static bool IsEmpty(this Matrix<double> matrix)
        {
            return matrix.ColumnCount==0 || matrix.RowCount==0;
        }
        public static string ShapeString(this Matrix<double> matrix)
        {
            return $"{matrix.RowCount}x{matrix.ColumnCount}";
        }
        public static Matrix<double> MatAdd(this Matrix<double> left, Matrix<double> right)
        {
            if (left.IsScalar()) return left[0,0]+right;
            if (right.IsScalar()) return left+right[0,0];
            else return left+right;
        }
        public static Matrix<double> DotAdd(this Matrix<double> left, Matrix<double> right)
        {
            if (left.IsColumnVector())
            {
                if (left.RowCount != right.RowCount)
                {
                    throw new ArgumentOutOfRangeException($"cannot broadcast matrix {left.ShapeString()} to {right.ShapeString()}");
                }
                var result = Matrix<double>.Build.DenseOfMatrix(right);
                for (int i = 0; i < result.RowCount; i++)
                {
                    result.SetRow(i, left[i, 0]+result.Row(i));
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
                    result.SetRow(i, left.Row(i)+right[i, 0]);
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
                    result.SetColumn(i, left[0, i]+result.Column(i));
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
                    result.SetRow(i, left.Column(i)+right[0, i]);
                }
                return result;
            }
            else return left+right;
        }
    }
}