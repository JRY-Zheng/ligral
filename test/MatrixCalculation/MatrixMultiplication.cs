using System;
using Xunit;
using MathNet.Numerics.LinearAlgebra;
using Ligral.Component;

namespace Ligral.Tests
{
    public class MatrixMultiplication
    {
        [Fact]
        public void MatrixMul_ScalarDotMulMatrix_ReturnMatrix()
        {
            Matrix<double> scalar = Matrix<double>.Build.Dense(1, 1, 2);
            Matrix<double> matrix = Matrix<double>.Build.DenseOfArray(new double[3,2]{{3, 4}, {5, 6}, {7, 8}});
            Matrix<double> result = Matrix<double>.Build.DenseOfArray(new double[3,2]{{6, 8}, {10, 12}, {14, 16}});
            Assert.True(scalar.DotMul(matrix).Equals(result), "Scalar broadcast-times matrix is matrix");
            Assert.True(matrix.DotMul(scalar).Equals(result), "Matrix broadcast-times scalar is matrix");
            Assert.True(scalar.MatMul(matrix).Equals(result), "Scalar times matrix is matrix");
            Assert.True(matrix.MatMul(scalar).Equals(result), "Matrix times scalar is matrix");
        }
        [Fact]
        public void MatrixDotMul_RowVectorDotMulMatrix_ReturnMatrix()
        {
            Matrix<double> rowVector = Matrix<double>.Build.DenseOfArray(new double[1, 2]{{2, 1}});
            Matrix<double> matrix = Matrix<double>.Build.DenseOfArray(new double[3,2]{{3, 4}, {5, 6}, {7, 8}});
            Matrix<double> result = Matrix<double>.Build.DenseOfArray(new double[3,2]{{6, 4}, {10, 6}, {14, 8}});
            Assert.True(matrix.DotMul(rowVector).Equals(result), "Row vector broadcast-times matrix is matrix");
            Assert.True(rowVector.DotMul(matrix).Equals(result), "Matrix broadcast-times row vector is matrix");
            Assert.Throws<ArgumentException>(()=>rowVector.DotMul(matrix.Transpose()));
            Matrix<double> longRowVector = rowVector.Append(Matrix<double>.Build.Dense(1, 1, 1));
            Assert.Throws<ArgumentException>(()=>longRowVector.DotMul(matrix));
        }
        [Fact]
        public void MatrixDotMul_ColumnVectorDotMulMatrix_ReturnMatrix()
        {
            Matrix<double> columnVector = Matrix<double>.Build.DenseOfArray(new double[3,1]{{3}, {2}, {1}});
            Matrix<double> matrix = Matrix<double>.Build.DenseOfArray(new double[3,2]{{3, 4}, {5, 6}, {7, 8}});
            Matrix<double> result = Matrix<double>.Build.DenseOfArray(new double[3,2]{{9, 12}, {10, 12}, {7, 8}});
            Assert.True(matrix.DotMul(columnVector).Equals(result), "Column vector broadcast-times matrix is matrix");
            Assert.True(columnVector.DotMul(matrix).Equals(result), "Matrix broadcast-times column vector is matrix");
            Matrix<double> shortColumnVector = columnVector.SubMatrix(0, 2, 0, 1);
            Assert.Throws<ArgumentException>(()=>shortColumnVector.DotMul(matrix));
            Matrix<double> longColumnVector = columnVector.Stack(Matrix<double>.Build.Dense(1, 1, 1));
            Assert.Throws<ArgumentException>(()=>longColumnVector.DotMul(matrix));
        }
        [Fact]
        public void MatrixDotMul_MatrixDotMulMatrix_ReturnMatrix()
        {
            Matrix<double> left = Matrix<double>.Build.DenseOfArray(new double[3,2]{{1, 0}, {-1, 1}, {0, -1}});
            Matrix<double> right = Matrix<double>.Build.DenseOfArray(new double[3,2]{{3, 4}, {5, 6}, {7, 8}});
            Matrix<double> result = Matrix<double>.Build.DenseOfArray(new double[3,2]{{3, 0}, {-5, 6}, {0, -8}});
            Assert.True(right.DotMul(left).Equals(result), "Matrix broadcast-times matrix is matrix");
            Assert.True(left.DotMul(right).Equals(result), "Matrix broadcast-times matrix is matrix");
            Assert.Throws<ArgumentException>(() => right.DotMul(left.Transpose()));
        }
        [Fact]
        public void MatrixMul_MatrixMulMatrix_ReturnMatrix()
        {
            Matrix<double> left = Matrix<double>.Build.DenseOfArray(new double[3,2]{{1, 0}, {-1, 1}, {0, -1}});
            Matrix<double> right = Matrix<double>.Build.DenseOfArray(new double[2,1]{{2}, {3}});
            Matrix<double> result = Matrix<double>.Build.DenseOfArray(new double[3,1]{{2}, {1}, {-3}});
            Assert.True(left.MatMul(right).Equals(result), "Matrix times matrix is matrix");
            Assert.Throws<ArgumentException>(() => right.MatMul(left));
            Assert.Throws<ArgumentException>(() => left.Transpose().MatMul(right));
        }
    }
}
