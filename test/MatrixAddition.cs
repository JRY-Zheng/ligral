using System;
using Xunit;
using MathNet.Numerics.LinearAlgebra;
using Ligral.Component;

namespace Ligral.Tests
{
    public class UnitTest1
    {
        [Fact]
        public void MatrixAdd_ScalarAddMatrix_ReturnMatrix()
        {
            Matrix<double> scalar = Matrix<double>.Build.Dense(1, 1, 1);
            Matrix<double> matrix = Matrix<double>.Build.DenseOfArray(new double[3,2]{{3, 4}, {5, 6}, {7, 8}});
            Matrix<double> result = Matrix<double>.Build.DenseOfArray(new double[3,2]{{4, 5}, {6, 7}, {8, 9}});
            Assert.True(scalar.MatAdd(matrix).Equals(result), "Scalar add matrix is matrix");
        }
        [Fact]
        public void MatrixAdd_MatrixAddScalar_ReturnMatrix()
        {
            Matrix<double> scalar = Matrix<double>.Build.Dense(1, 1, -1);
            Matrix<double> matrix = Matrix<double>.Build.DenseOfArray(new double[3,2]{{3, 4}, {5, 6}, {7, 8}});
            Matrix<double> result = Matrix<double>.Build.DenseOfArray(new double[3,2]{{2, 3}, {4, 5}, {6, 7}});
            Assert.True(matrix.MatAdd(scalar).Equals(result), "Matrix add scalar is matrix");
        }
    }
}
