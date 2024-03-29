/* Copyright (C) 2019-2022 Junruoyu Zheng. Home page: https://junruoyu-zheng.gitee.io/ligral

   Distributed under MIT license.
   See file LICENSE for detail or copy at https://opensource.org/licenses/MIT
*/

using System;
using Xunit;
using MathNet.Numerics.LinearAlgebra;
using Ligral.Component;

namespace Ligral.Tests.MatrixCalculation
{
    public class MatrixAddition
    {
        [Fact]
        public void MatrixAdd_ScalarAddMatrix_ReturnMatrix()
        {
            Matrix<double> scalar = Matrix<double>.Build.Dense(1, 1, 1);
            Matrix<double> matrix = Matrix<double>.Build.DenseOfArray(new double[3,2]{{3, 4}, {5, 6}, {7, 8}});
            Matrix<double> result = Matrix<double>.Build.DenseOfArray(new double[3,2]{{4, 5}, {6, 7}, {8, 9}});
            Assert.True(scalar.MatAdd(matrix).Equals(result), "Scalar plus matrix shall be matrix");
            Assert.True(matrix.MatAdd(scalar).Equals(result), "Matrix plus scalar shall be matrix");
        }
        [Fact]
        public void MatrixAdd_RowVectorAddMatrix_ReturnMatrix()
        {
            Matrix<double> rowVector = Matrix<double>.Build.DenseOfArray(new double[1, 2]{{2, 1}});
            Matrix<double> matrix = Matrix<double>.Build.DenseOfArray(new double[3,2]{{3, 4}, {5, 6}, {7, 8}});
            Matrix<double> result = Matrix<double>.Build.DenseOfArray(new double[3,2]{{5, 5}, {7, 7}, {9, 9}});
            Assert.True(matrix.MatAdd(rowVector).Equals(result), "Matrix plus row vector shall be matrix");
            Assert.True(rowVector.MatAdd(matrix).Equals(result), "Row vector plus matrix shall be matrix");
            Assert.Throws<ArgumentException>(()=>rowVector.MatAdd(matrix.Transpose()));
            Matrix<double> longRowVector = rowVector.Append(Matrix<double>.Build.Dense(1, 1, 1));
            Assert.Throws<ArgumentException>(()=>longRowVector.MatAdd(matrix));
        }
        [Fact]
        public void MatrixAdd_ColumnVectorAddMatrix_ReturnMatrix()
        {
            Matrix<double> columnVector = Matrix<double>.Build.DenseOfArray(new double[3,1]{{3}, {2}, {1}});
            Matrix<double> matrix = Matrix<double>.Build.DenseOfArray(new double[3,2]{{3, 4}, {5, 6}, {7, 8}});
            Matrix<double> result = Matrix<double>.Build.DenseOfArray(new double[3,2]{{6, 7}, {7, 8}, {8, 9}});
            Assert.True(matrix.MatAdd(columnVector).Equals(result), "Matrix plus column vector shall be matrix");
            Assert.True(columnVector.MatAdd(matrix).Equals(result), "Column vector plus matrix shall be matrix");
            Matrix<double> shortColumnVector = columnVector.SubMatrix(0, 2, 0, 1);
            Assert.Throws<ArgumentException>(()=>shortColumnVector.MatAdd(matrix));
            Matrix<double> longColumnVector = columnVector.Stack(Matrix<double>.Build.Dense(1, 1, 1));
            Assert.Throws<ArgumentException>(()=>longColumnVector.MatAdd(matrix));
        }
        [Fact]
        public void MatrixAdd_MatrixAddMatrix_ReturnMatrix()
        {
            Matrix<double> left = Matrix<double>.Build.DenseOfArray(new double[3,2]{{1, 0}, {-1, 1}, {0, -1}});
            Matrix<double> right = Matrix<double>.Build.DenseOfArray(new double[3,2]{{3, 4}, {5, 6}, {7, 8}});
            Matrix<double> result = Matrix<double>.Build.DenseOfArray(new double[3,2]{{4, 4}, {4, 7}, {7, 7}});
            Assert.True(right.MatAdd(left).Equals(result), "Matrix plus matrix shall be matrix");
            Assert.True(left.MatAdd(right).Equals(result), "Matrix plus matrix shall be matrix");
            Assert.Throws<ArgumentException>(() => right.MatAdd(left.Transpose()));
        }
    }
}
