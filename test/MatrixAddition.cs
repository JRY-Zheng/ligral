using System;
using Xunit;
using MathNet.Numerics.LinearAlgebra;
using Ligral.Component;

namespace Ligral.Tests
{
    public class MatrixAddition
    {
        [Fact]
        public void MatrixAdd_ScalarAddMatrix_ReturnMatrix()
        {
            Matrix<double> scalar = Matrix<double>.Build.Dense(1, 1, 1);
            Matrix<double> matrix = Matrix<double>.Build.DenseOfArray(new double[3,2]{{3, 4}, {5, 6}, {7, 8}});
            Matrix<double> result = Matrix<double>.Build.DenseOfArray(new double[3,2]{{4, 5}, {6, 7}, {8, 9}});
            Assert.True(scalar.MatAdd(matrix).Equals(result), "Scalar plus matrix is matrix");
            Assert.True(matrix.MatAdd(scalar).Equals(result), "Matrix plus scalar is matrix");
        }
        [Fact]
        public void MatrixAdd_RowVectorAddMatrix_ReturnMatrix()
        {
            Matrix<double> rowVector = Matrix<double>.Build.DenseOfArray(new double[1, 2]{{2, 1}});
            Matrix<double> matrix = Matrix<double>.Build.DenseOfArray(new double[3,2]{{3, 4}, {5, 6}, {7, 8}});
            Matrix<double> result = Matrix<double>.Build.DenseOfArray(new double[3,2]{{5, 5}, {7, 7}, {9, 9}});
            Assert.True(matrix.MatAdd(rowVector).Equals(result), "Row vector plus matrix is matrix");
            Assert.True(rowVector.MatAdd(matrix).Equals(result), "Matrix plus row vector is matrix");
            Matrix<double> shortRowVector = rowVector.SubMatrix(0, 1, 0, 1);
            Assert.Throws<ArgumentOutOfRangeException>(()=>shortRowVector.Add(matrix));
            Matrix<double> longRowVector = rowVector.Append(Matrix<double>.Build.Dense(1, 1, 1));
            Assert.Throws<ArgumentOutOfRangeException>(()=>longRowVector.Add(matrix));
        }
        [Fact]
        public void MatrixAdd_ColumnVectorAddMatrix_ReturnMatrix()
        {
            Matrix<double> columnVector = Matrix<double>.Build.DenseOfArray(new double[3,1]{{3}, {2}, {1}});
            Matrix<double> matrix = Matrix<double>.Build.DenseOfArray(new double[3,2]{{3, 4}, {5, 6}, {7, 8}});
            Matrix<double> result = Matrix<double>.Build.DenseOfArray(new double[3,2]{{6, 7}, {7, 8}, {8, 9}});
            Assert.True(matrix.MatAdd(columnVector).Equals(result), "Column vector plus matrix is matrix");
            Assert.True(columnVector.MatAdd(matrix).Equals(result), "Matrix plus column vector is matrix");
            Matrix<double> shortColumnVector = columnVector.SubMatrix(0, 2, 0, 1);
            Assert.Throws<ArgumentOutOfRangeException>(()=>shortColumnVector.Add(matrix));
            Matrix<double> longColumnVector = columnVector.Stack(Matrix<double>.Build.Dense(1, 1, 1));
            Assert.Throws<ArgumentOutOfRangeException>(()=>longColumnVector.Add(matrix));
        }
        [Fact]
        public void MatrixAdd_MatrixAddMatrix_ReturnMatrix()
        {
            Matrix<double> left = Matrix<double>.Build.DenseOfArray(new double[3,2]{{1, 0}, {-1, 1}, {0, -1}});
            Matrix<double> right = Matrix<double>.Build.DenseOfArray(new double[3,2]{{3, 4}, {5, 6}, {7, 8}});
            Matrix<double> result = Matrix<double>.Build.DenseOfArray(new double[3,2]{{4, 4}, {4, 7}, {7, 7}});
            Assert.True(right.MatAdd(left).Equals(result), "Column vector plus matrix is matrix");
            Assert.True(left.MatAdd(right).Equals(result), "Matrix plus column vector is matrix");
        }
    }
}
