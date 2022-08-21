/* Copyright (C) 2019-2022 Junruoyu Zheng. Home page: https://junruoyu-zheng.gitee.io/ligral

   Distributed under MIT license.
   See file LICENSE for detail or copy at https://opensource.org/licenses/MIT
*/

using System.Collections.Generic;
using Xunit;
using MathNet.Numerics.LinearAlgebra;
using Ligral.Component;

namespace Ligral.Tests.ModelTester
{
    public class TestInverse
    {
        [Fact]
        public void Inverse_InputScalar_OutputScalar()
        {
            var model = ModelManager.Create("Inverse", null);
            var modelTester = new ModelTester();
            var inputs = new List<Matrix<double>> {3.1.ToMatrix()};
            var outputs = new List<Matrix<double>> {1/3.1.ToMatrix()};
            Assert.True(modelTester.Test(model, inputs, outputs));
        }
        [Fact]
        public void Inverse_InputSquareMatrix_OutputMatrix()
        {
            var model = ModelManager.Create("Inverse", null);
            var modelTester = new ModelTester();
            var mat = Matrix<double>.Build.DenseOfArray(new double[3,3]{{1, 2, 3}, {2, 3, 4}, {-1, -2, -5}});
            var inputs = new List<Matrix<double>> {mat};
            var outputs = new List<Matrix<double>> {mat.Inverse()};
            Assert.True(modelTester.Test(model, inputs, outputs));
        }
        [Fact]
        public void Inverse_InputNonSquareMatrix_OutputMatrix()
        {
            var model = ModelManager.Create("Inverse", null);
            var modelTester = new ModelTester();
            var mat = Matrix<double>.Build.DenseOfArray(new double[2,3]{{1, 2, 3}, {2, 3, 4}});
            var inputs = new List<Matrix<double>> {mat};
            Assert.Throws<ModelException>(() => modelTester.TestInput(model, inputs));
        }
        [Fact]
        public void Inverse_InputSingularMatrix_OutputMatrix()
        {
            var model = ModelManager.Create("Inverse", null);
            var modelTester = new ModelTester();
            var mat = Matrix<double>.Build.DenseOfArray(new double[3,3]{{1, 2, 3}, {2, 3, 4}, {-1, -2, -3}});
            var inputs = new List<Matrix<double>> {mat};
            Assert.Throws<ModelException>(() => modelTester.TestInput(model, inputs));
        }
    }
}