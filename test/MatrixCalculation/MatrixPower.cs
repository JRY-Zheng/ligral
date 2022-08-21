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
    public class MatrixPower
    {
        [Fact]
        public void MatrixPow_ScalarDotPowMatrix_ReturnMatrix()
        {
            Matrix<double> scalar = Matrix<double>.Build.Dense(1, 1, 2);
            Matrix<double> matrix = Matrix<double>.Build.DenseOfArray(new double[3,2]{{3, 4}, {5, 6}, {7, 8}});
            Matrix<double> result1 = Matrix<double>.Build.DenseOfArray(new double[3,2]{{8, 16}, {32, 64}, {128, 256}});
            Matrix<double> result2 = Matrix<double>.Build.DenseOfArray(new double[3,2]{{9, 16}, {25, 36}, {49, 64}});
            Assert.True(scalar.DotPow(matrix).Equals(result1), "Scalar broadcast-power matrix shall be matrix");
            Assert.True(matrix.DotPow(scalar).Equals(result2), "Matrix broadcast-times scalar shall be matrix");
            Matrix<double> result4 = Matrix<double>.Build.DenseOfArray(new double[3,2]{{1.0/3.0, 1.0/4.0}, {1.0/5.0, 1.0/6.0}, {1.0/7.0, 1.0/8.0}});
            Matrix<double> negative = Matrix<double>.Build.Dense(1, 1, -1);
            Assert.True(matrix.DotPow(negative).Equals(result4), "Matrix broadcast-power scalar shall be matrix");
        }
        [Fact]
        public void MatrixPow_ScalarPowMatrix_ReturnMatrix()
        {
            Matrix<double> scalar = Matrix<double>.Build.Dense(1, 1, 2);
            Matrix<double> matrix = Matrix<double>.Build.DenseOfArray(new double[3,2]{{3, 4}, {5, 6}, {7, 8}});
            Matrix<double> square = Matrix<double>.Build.DenseOfArray(new double[2,2]{{3, 4}, {5, 6}});
            Matrix<double> result3 = Matrix<double>.Build.DenseOfArray(new double[2,2]{{29, 36}, {45, 56}});
            Assert.True(square.MatPow(scalar).Equals(result3), "Matrix to the power of scalar shall be matrix");
            Assert.Throws<ArgumentException>(()=>matrix.MatPow(scalar));
            Assert.Throws<ArgumentException>(()=>square.MatPow(square));
            Matrix<double> rational = Matrix<double>.Build.Dense(1, 1, 2.1);
            Assert.Throws<ArgumentException>(()=>square.MatPow(rational));
            Matrix<double> zero = Matrix<double>.Build.Dense(1, 1, 0);
            Assert.True(square.MatPow(zero).Equals(Matrix<double>.Build.DenseIdentity(2, 2)), "Matrix to the power of 0 shall be identity");
            Matrix<double> negative = Matrix<double>.Build.Dense(1, 1, -1);
            Matrix<double> inverse = Matrix<double>.Build.DenseOfArray(new double[2,2]{{-3, 2}, {2.5, -1.5}});
            Assert.True(square.MatPow(negative).EqualTo(inverse), "Matrix to the power of negative scalar shall be power of inverse");
            Matrix<double> singularSquare = Matrix<double>.Build.DenseOfArray(new double[2,2]{{1, -1}, {-1, 1}});
            Matrix<double> result5 = Matrix<double>.Build.DenseOfArray(new double[2,2]{{2, -2}, {-2, 2}});
            Assert.True(singularSquare.MatPow(scalar).EqualTo(result5), "Matrix to the power of scalar shall be matrix");
            Assert.Throws<DivideByZeroException>(()=>singularSquare.MatPow(negative));
        }
        [Fact]
        public void MatrixDotPow_RowVectorDotPowMatrix_ReturnMatrix()
        {
            Matrix<double> rowVector = Matrix<double>.Build.DenseOfArray(new double[1, 2]{{2, 1}});
            Matrix<double> matrix = Matrix<double>.Build.DenseOfArray(new double[3,2]{{3, 4}, {5, 6}, {7, 8}});
            Matrix<double> result1 = Matrix<double>.Build.DenseOfArray(new double[3,2]{{9, 4}, {25, 6}, {49, 8}});
            Matrix<double> result2 = Matrix<double>.Build.DenseOfArray(new double[3,2]{{8, 1}, {32, 1}, {128, 1}});
            Assert.True(matrix.DotPow(rowVector).Equals(result1), "Matrix broadcast-power row vector shall be matrix");
            Assert.True(rowVector.DotPow(matrix).Equals(result2), "Row vector broadcast-power matrix shall be matrix");
            Assert.Throws<ArgumentException>(()=>rowVector.DotPow(matrix.Transpose()));
            Matrix<double> longRowVector = rowVector.Append(Matrix<double>.Build.Dense(1, 1, 1));
            Assert.Throws<ArgumentException>(()=>longRowVector.DotPow(matrix));
        }
        [Fact]
        public void MatrixDotPow_ColumnVectorDotPowMatrix_ReturnMatrix()
        {
            Matrix<double> columnVector = Matrix<double>.Build.DenseOfArray(new double[3,1]{{3}, {2}, {1}});
            Matrix<double> matrix = Matrix<double>.Build.DenseOfArray(new double[3,2]{{3, 4}, {5, 6}, {7, 8}});
            Matrix<double> result1 = Matrix<double>.Build.DenseOfArray(new double[3,2]{{27, 64}, {25, 36}, {7, 8}});
            Matrix<double> result2 = Matrix<double>.Build.DenseOfArray(new double[3,2]{{27, 81}, {32, 64}, {1, 1}});
            Assert.True(matrix.DotPow(columnVector).Equals(result1), "Matrix broadcast-power column vector shall be matrix");
            Assert.True(columnVector.DotPow(matrix).Equals(result2), "Column vector broadcast-power matrix shall be matrix");
            Matrix<double> shortColumnVector = columnVector.SubMatrix(0, 2, 0, 1);
            Assert.Throws<ArgumentException>(()=>shortColumnVector.DotPow(matrix));
            Matrix<double> longColumnVector = columnVector.Stack(Matrix<double>.Build.Dense(1, 1, 1));
            Assert.Throws<ArgumentException>(()=>longColumnVector.DotPow(matrix));
        }
        [Fact]
        public void MatrixDotPow_MatrixDotPowMatrix_ReturnMatrix()
        {
            Matrix<double> left = Matrix<double>.Build.DenseOfArray(new double[3,2]{{1, 0}, {-1, 1}, {0, -1}});
            Matrix<double> right = Matrix<double>.Build.DenseOfArray(new double[3,2]{{3, 4}, {5, 6}, {7, 8}});
            Matrix<double> result1 = Matrix<double>.Build.DenseOfArray(new double[3,2]{{3, 1}, {1.0/5.0, 6}, {1, 1.0/8.0}});
            Matrix<double> result2 = Matrix<double>.Build.DenseOfArray(new double[3,2]{{1, 0}, {-1, 1}, {0, 1}});
            Assert.True(right.DotPow(left).Equals(result1), "Matrix broadcast-times matrix shall be matrix");
            Assert.True(left.DotPow(right).Equals(result2), "Matrix broadcast-times matrix shall be matrix");
            Assert.Throws<ArgumentException>(() => right.DotPow(left.Transpose()));
        }
    }
}
