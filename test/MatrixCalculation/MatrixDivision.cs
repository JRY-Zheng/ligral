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
    public class MatrixDivision
    {
        [Fact]
        public void MatrixDiv_ScalarDotDivMatrix_ReturnMatrix()
        {
            Matrix<double> scalar = Matrix<double>.Build.Dense(1, 1, 2);
            Matrix<double> zero = Matrix<double>.Build.Dense(1, 1, 0);
            Matrix<double> matrix = Matrix<double>.Build.DenseOfArray(new double[3,2]{{3, 4}, {5, 6}, {7, 8}});
            Matrix<double> result1 = Matrix<double>.Build.DenseOfArray(new double[3,2]{{2.0/3.0, 2.0/4.0}, {2.0/5.0, 2.0/6.0}, {2.0/7.0, 2.0/8.0}});
            Matrix<double> result2 = Matrix<double>.Build.DenseOfArray(new double[3,2]{{3.0/2.0, 4.0/2.0}, {5.0/2.0, 6.0/2.0}, {7.0/2.0, 8.0/2.0}});
            Assert.True(scalar.DotDiv(matrix).Equals(result1), "Scalar broadcast-over matrix shall be matrix");
            Assert.True(matrix.DotDiv(scalar).Equals(result2), "Matrix broadcast-over scalar shall be matrix");
            Assert.True(scalar.LeftDiv(matrix).Equals(result2), "Scalar over matrix shall be matrix");
            Assert.True(matrix.LeftDiv(scalar).Equals(result1), "Matrix over scalar shall be matrix");
            Assert.True(scalar.RightDiv(matrix).Equals(result1), "Scalar under matrix shall be matrix");
            Assert.True(matrix.RightDiv(scalar).Equals(result2), "Matrix under scalar shall be matrix");
            Assert.Throws<DivideByZeroException>(()=>matrix.DotDiv(zero));
            Assert.Throws<DivideByZeroException>(()=>matrix.RightDiv(zero));
            Assert.Throws<DivideByZeroException>(()=>zero.LeftDiv(matrix));
        }
        [Fact]
        public void MatrixDotDiv_RowVectorDotDivMatrix_ReturnMatrix()
        {
            Matrix<double> rowVector = Matrix<double>.Build.DenseOfArray(new double[1, 2]{{2, 1}});
            Matrix<double> matrix = Matrix<double>.Build.DenseOfArray(new double[3,2]{{3, 4}, {5, 6}, {7, 8}});
            Matrix<double> result1 = Matrix<double>.Build.DenseOfArray(new double[3,2]{{2.0/3.0, 1.0/4.0}, {2.0/5.0, 1.0/6.0}, {2.0/7.0, 1.0/8.0}});
            Matrix<double> result2 = Matrix<double>.Build.DenseOfArray(new double[3,2]{{3.0/2.0, 4.0/1.0}, {5.0/2.0, 6.0/1.0}, {7.0/2.0, 8.0/1.0}});
            Assert.True(matrix.DotDiv(rowVector).Equals(result2), "Matrix broadcast-over row vector shall be matrix");
            Assert.True(rowVector.DotDiv(matrix).Equals(result1), "Row vector broadcast-over matrix shall be matrix");
            Assert.Throws<ArgumentException>(()=>rowVector.DotDiv(matrix.Transpose()));
            Matrix<double> longRowVector = rowVector.Append(Matrix<double>.Build.Dense(1, 1, 1));
            Assert.Throws<ArgumentException>(()=>longRowVector.DotDiv(matrix));
            Matrix<double> zeroRowVector = Matrix<double>.Build.DenseOfArray(new double[1, 2]{{0, 1}});
            Assert.Throws<DivideByZeroException>(()=>matrix.DotDiv(zeroRowVector));
            Matrix<double> zeroMatrix = Matrix<double>.Build.DenseOfArray(new double[3,2]{{3, 4}, {0, 6}, {7, 8}});
            Assert.Throws<DivideByZeroException>(()=>rowVector.DotDiv(zeroMatrix));
        }
        [Fact]
        public void MatrixDotDiv_ColumnVectorDotDivMatrix_ReturnMatrix()
        {
            Matrix<double> columnVector = Matrix<double>.Build.DenseOfArray(new double[3,1]{{3}, {2}, {1}});
            Matrix<double> matrix = Matrix<double>.Build.DenseOfArray(new double[3,2]{{3, 4}, {5, 6}, {7, 8}});
            Matrix<double> result1 = Matrix<double>.Build.DenseOfArray(new double[3,2]{{3.0/3.0, 3.0/4.0}, {2.0/5.0, 2.0/6.0}, {1.0/7.0, 1.0/8.0}});
            Matrix<double> result2 = Matrix<double>.Build.DenseOfArray(new double[3,2]{{3.0/3.0, 4.0/3.0}, {5.0/2.0, 6.0/2.0}, {7.0/1.0, 8.0/1.0}});
            Assert.True(matrix.DotDiv(columnVector).Equals(result2), "Matrix broadcast-over column vector shall be matrix");
            Assert.True(columnVector.DotDiv(matrix).Equals(result1), "Column vector broadcast-over matrix shall be matrix");
            Matrix<double> shortColumnVector = columnVector.SubMatrix(0, 2, 0, 1);
            Assert.Throws<ArgumentException>(()=>shortColumnVector.DotDiv(matrix));
            Matrix<double> longColumnVector = columnVector.Stack(Matrix<double>.Build.Dense(1, 1, 1));
            Assert.Throws<ArgumentException>(()=>longColumnVector.DotDiv(matrix));
            Matrix<double> zeroColumnVector = Matrix<double>.Build.DenseOfArray(new double[3,1]{{0}, {2}, {1}});
            Assert.Throws<DivideByZeroException>(()=>matrix.DotDiv(zeroColumnVector));
            Matrix<double> zeroMatrix = Matrix<double>.Build.DenseOfArray(new double[3,2]{{3, 4}, {0, 6}, {7, 8}});
            Assert.Throws<DivideByZeroException>(()=>columnVector.DotDiv(zeroMatrix));
        }
        [Fact]
        public void MatrixDotDiv_MatrixDotDivMatrix_ReturnMatrix()
        {
            Matrix<double> left = Matrix<double>.Build.DenseOfArray(new double[3,2]{{1, 0}, {-1, 1}, {0, -1}});
            Matrix<double> right = Matrix<double>.Build.DenseOfArray(new double[3,2]{{3, 4}, {5, 6}, {7, 8}});
            Matrix<double> result = Matrix<double>.Build.DenseOfArray(new double[3,2]{{1.0/3.0, 0.0/4.0}, {-1.0/5.0, 1.0/6.0}, {0.0/7.0, -1.0/8.0}});
            Assert.Throws<DivideByZeroException>(()=>right.DotDiv(left));
            Assert.True(left.DotDiv(right).Equals(result), "Matrix broadcast-over matrix shall be matrix");
            Assert.Throws<ArgumentException>(() => right.DotDiv(left.Transpose()));
        }
        [Fact]
        public void MatrixDiv_MatrixDivMatrix_ReturnMatrix()
        {
            Matrix<double> square = Matrix<double>.Build.DenseOfArray(new double[2,2]{{1, 0}, {-1, 1}});
            Matrix<double> inverse = Matrix<double>.Build.DenseOfArray(new double[2,2]{{1, 0}, {1, 1}});
            Matrix<double> right = Matrix<double>.Build.DenseOfArray(new double[2,1]{{2}, {3}});
            Assert.True(square.LeftDiv(right).Equals(inverse.MatMul(right)), "Matrix over matrix shall be matrix");
            Assert.True(right.Transpose().RightDiv(square).Equals(right.Transpose().MatMul(inverse)), "Matrix under matrix shall be matrix");
            Matrix<double> singularSquare = Matrix<double>.Build.DenseOfArray(new double[2,2]{{1, -1}, {-1, 1}});
            Assert.Throws<DivideByZeroException>(()=>singularSquare.LeftDiv(right));
            Assert.Throws<DivideByZeroException>(()=>right.Transpose().RightDiv(singularSquare));
            Matrix<double> left = Matrix<double>.Build.DenseOfArray(new double[3,2]{{1, 0}, {-1, 1}, {0, -1}});
            Assert.Throws<ArgumentException>(() => left.LeftDiv(right));
            Assert.Throws<ArgumentException>(() => left.RightDiv(right));
        }
    }
}
