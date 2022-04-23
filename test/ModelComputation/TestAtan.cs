/* Copyright (C) 2019-2021 Junruoyu Zheng. Home page: https://junruoyu-zheng.gitee.io/ligral

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
    public class TestAtan
    {
        [Fact]
        public void Atan_InputZero_OutputZero()
        {
            var model = ModelManager.Create("Atan", null);
            var modelTester = new ModelTester();
            var inputs = new List<Matrix<double>> {0.ToMatrix()};
            var outputs = new List<Matrix<double>> {0.ToMatrix()};
            Assert.True(modelTester.Test(model, inputs, outputs));
        }
        [Fact]
        public void Atan_InputNegative_OutputNegative()
        {
            var model = ModelManager.Create("Atan", null);
            var modelTester = new ModelTester();
            var inputs = new List<Matrix<double>> {-10.ToMatrix()};
            var outputs = new List<Matrix<double>> {Math.Atan(-10).ToMatrix()};
            Assert.True(modelTester.Test(model, inputs, outputs));
        }
        [Fact]
        public void Atan_InputPositive_OutputPositive()
        {
            var model = ModelManager.Create("Atan", null);
            var modelTester = new ModelTester();
            var inputs = new List<Matrix<double>> {1.3.ToMatrix()};
            var outputs = new List<Matrix<double>> {Math.Atan(1.3).ToMatrix()};
            Assert.True(modelTester.Test(model, inputs, outputs));
        }
        [Fact]
        public void Atan_InputMatrix_OutputMatrix()
        {
            var model = ModelManager.Create("Atan", null);
            var modelTester = new ModelTester();
            var inputs = new List<Matrix<double>> {Matrix<double>.Build.DenseOfArray(new double[2,3]{{1, -2, 3}, {-4.707, 5.707, 100}})};
            Assert.True(modelTester.Test(model, inputs, inputs.ConvertAll(matrix => matrix.Map(item => Math.Atan(item)))));
        }
    }
}