/* Copyright (C) 2019-2022 Junruoyu Zheng. Home page: https://junruoyu-zheng.gitee.io/ligral

   Distributed under MIT license.
   See file LICENSE for detail or copy at https://opensource.org/licenses/MIT
*/

using System;
using System.Collections.Generic;
using Xunit;
using MathNet.Numerics.LinearAlgebra;
using Ligral.Component;

namespace Ligral.Tests.ModelTester
{
    public class TestAsin
    {
        [Fact]
        public void Asin_InputZero_OutputZero()
        {
            var model = ModelManager.Create("Asin", null);
            var modelTester = new ModelTester();
            var inputs = new List<Matrix<double>> {0.ToMatrix()};
            var outputs = new List<Matrix<double>> {0.ToMatrix()};
            Assert.True(modelTester.Test(model, inputs, outputs));
        }
        [Fact]
        public void Asin_InputOne_OutputHalfPi()
        {
            var model = ModelManager.Create("Asin", null);
            var modelTester = new ModelTester();
            var inputs = new List<Matrix<double>> {1.ToMatrix()};
            var outputs = new List<Matrix<double>> {Math.PI/2.ToMatrix()};
            Assert.True(modelTester.Test(model, inputs, outputs));
        }
        [Fact]
        public void Asin_InputNegativeScalar_OutputNegativeScalar()
        {
            var model = ModelManager.Create("Asin", null);
            var modelTester = new ModelTester();
            var inputs = new List<Matrix<double>> {-0.56.ToMatrix()};
            var outputs = new List<Matrix<double>> {-Math.Asin(0.56).ToMatrix()};
            Assert.True(modelTester.Test(model, inputs, outputs));
        }
        [Fact]
        public void Asin_InputLargeScalar_CauseError()
        {
            var model = ModelManager.Create("Asin", null);
            var modelTester = new ModelTester();
            var inputs = new List<Matrix<double>> {1.3.ToMatrix()};
            Assert.Throws<ModelException>(() => modelTester.TestInput(model, inputs));
        }
        [Fact]
        public void Asin_InputMatrix_OutputMatrix()
        {
            var model = ModelManager.Create("Asin", null);
            var modelTester = new ModelTester();
            var inputs = new List<Matrix<double>> {Matrix<double>.Build.DenseOfArray(new double[2,3]{{1, -1, 0}, {0.707, -0.707, 0.56}})};
            Assert.True(modelTester.Test(model, inputs, inputs.ConvertAll(matrix => matrix.PointwiseAsin())));
        }
        [Fact]
        public void Asin_InputMatrixWithLargeValue_CauseError()
        {
            var model = ModelManager.Create("Asin", null);
            var modelTester = new ModelTester();
            var inputs = new List<Matrix<double>> {Matrix<double>.Build.DenseOfArray(new double[2,3]{{1, -1, 0}, {0.707, -0.707, -1.3}})};
            Assert.Throws<ModelException>(() => modelTester.TestInput(model, inputs));
        }
    }
}