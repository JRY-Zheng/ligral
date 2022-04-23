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
    public class TestAcosh
    {
        [Fact]
        public void Acosh_InputOne_OutputZero()
        {
            var model = ModelManager.Create("Acosh", null);
            var modelTester = new ModelTester();
            var inputs = new List<Matrix<double>> {1.ToMatrix()};
            var outputs = new List<Matrix<double>> {0.ToMatrix()};
            Assert.True(modelTester.Test(model, inputs, outputs));
        }
        [Fact]
        public void Acosh_InputSmallValue_CauseError()
        {
            var model = ModelManager.Create("Acosh", null);
            var modelTester = new ModelTester();
            var inputs = new List<Matrix<double>> {0.99.ToMatrix()};
            Assert.Throws<ModelException>(() => modelTester.TestInput(model, inputs));
        }
        [Fact]
        public void Acosh_InputLargeScalar_OutputsPositive()
        {
            var model = ModelManager.Create("Acosh", null);
            var modelTester = new ModelTester();
            var inputs = new List<Matrix<double>> {1.3.ToMatrix()};
            var outputs = new List<Matrix<double>> {Math.Acosh(1.3).ToMatrix()};
            Assert.True(modelTester.Test(model, inputs, outputs));
        }
        [Fact]
        public void Acosh_InputMatrix_OutputMatrix()
        {
            var model = ModelManager.Create("Acosh", null);
            var modelTester = new ModelTester();
            var inputs = new List<Matrix<double>> {Matrix<double>.Build.DenseOfArray(new double[2,3]{{1, 2, 3}, {4.707, 5.707, 100}})};
            Assert.True(modelTester.Test(model, inputs, inputs.ConvertAll(matrix => matrix.Map(item => Math.Acosh(item)))));
        }
        [Fact]
        public void Acosh_InputMatrixWithLargeValue_CauseError()
        {
            var model = ModelManager.Create("Acosh", null);
            var modelTester = new ModelTester();
            var inputs = new List<Matrix<double>> {Matrix<double>.Build.DenseOfArray(new double[2,3]{{1, -1, 0}, {0.707, -0.707, -1.3}})};
            Assert.Throws<ModelException>(() => modelTester.TestInput(model, inputs));
        }
    }
}