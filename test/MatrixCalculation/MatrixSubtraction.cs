using System;
using Xunit;
using MathNet.Numerics.LinearAlgebra;
using Ligral.Component;

namespace Ligral.Tests
{
    public class MatrixSubtraction
    {
        [Fact]
        public void MatrixSub_ScalarSubMatrix_ReturnMatrix()
        {
            Matrix<double> scalar = Matrix<double>.Build.Dense(1, 1, 1);
            Matrix<double> matrix = Matrix<double>.Build.DenseOfArray(new double[3,2]{{3, 4}, {5, 6}, {7, 8}});
            Matrix<double> result = Matrix<double>.Build.DenseOfArray(new double[3,2]{{2, 3}, {4, 5}, {6, 7}});
            Assert.True(scalar.MatSub(matrix).Equals(-result), "Scalar minus matrix is matrix");
            Assert.True(matrix.MatSub(scalar).Equals(result), "Matrix minus scalar is matrix");
        }
        [Fact]
        public void MatrixSub_RowVectorSubMatrix_ReturnMatrix()
        {
            Matrix<double> rowVector = Matrix<double>.Build.DenseOfArray(new double[1, 2]{{2, 1}});
            Matrix<double> matrix = Matrix<double>.Build.DenseOfArray(new double[3,2]{{3, 4}, {5, 6}, {7, 8}});
            Matrix<double> result = Matrix<double>.Build.DenseOfArray(new double[3,2]{{1, 3}, {3, 5}, {5, 7}});
            Assert.True(matrix.MatSub(rowVector).Equals(result), "Row vector minus matrix is matrix");
            Assert.True(rowVector.MatSub(matrix).Equals(-result), "Matrix minus row vector is matrix");
            Assert.Throws<ArgumentException>(()=>rowVector.MatSub(matrix.Transpose()));
            Matrix<double> longRowVector = rowVector.Append(Matrix<double>.Build.Dense(1, 1, 1));
            Assert.Throws<ArgumentException>(()=>longRowVector.MatSub(matrix));
        }
        [Fact]
        public void MatrixSub_ColumnVectorSubMatrix_ReturnMatrix()
        {
            Matrix<double> columnVector = Matrix<double>.Build.DenseOfArray(new double[3,1]{{3}, {2}, {1}});
            Matrix<double> matrix = Matrix<double>.Build.DenseOfArray(new double[3,2]{{3, 4}, {5, 6}, {7, 8}});
            Matrix<double> result = Matrix<double>.Build.DenseOfArray(new double[3,2]{{0, 1}, {3, 4}, {6, 7}});
            Assert.True(matrix.MatSub(columnVector).Equals(result), "Column vector minus matrix is matrix");
            Assert.True(columnVector.MatSub(matrix).Equals(-result), "Matrix minus column vector is matrix");
            Matrix<double> shortColumnVector = columnVector.SubMatrix(0, 2, 0, 1);
            Assert.Throws<ArgumentException>(()=>shortColumnVector.MatSub(matrix));
            Matrix<double> longColumnVector = columnVector.Stack(Matrix<double>.Build.Dense(1, 1, 1));
            Assert.Throws<ArgumentException>(()=>longColumnVector.MatSub(matrix));
        }
        [Fact]
        public void MatrixSub_MatrixSubMatrix_ReturnMatrix()
        {
            Matrix<double> left = Matrix<double>.Build.DenseOfArray(new double[3,2]{{1, 0}, {-1, 1}, {0, -1}});
            Matrix<double> right = Matrix<double>.Build.DenseOfArray(new double[3,2]{{3, 4}, {5, 6}, {7, 8}});
            Matrix<double> result = Matrix<double>.Build.DenseOfArray(new double[3,2]{{-2, -4}, {-6, -5}, {-7, -9}});
            Assert.True(right.MatSub(left).Equals(-result), "Matrix minus matrix is matrix");
            Assert.True(left.MatSub(right).Equals(result), "Matrix minus matrix is matrix");
            Assert.Throws<ArgumentException>(() => right.MatSub(left.Transpose()));
        }
    }
}
