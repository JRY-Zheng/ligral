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
    public class TestExp
    {
        [Fact]
        public void Exp_InputScalar_OutputScalar()
        {
            var model = ModelManager.Create("Exp", null);
            var modelTester = new ModelTester();
            var inputs = new List<Matrix<double>> {1.3.ToMatrix()};
            var outputs = new List<Matrix<double>> {Math.Exp(1.3).ToMatrix()};
            Assert.True(modelTester.Test(model, inputs, outputs));
        }
        [Fact]
        public void Exp_InputNegativeScalar_OutputScalar()
        {
            var model = ModelManager.Create("Exp", null);
            var modelTester = new ModelTester();
            var inputs = new List<Matrix<double>> {-1.3.ToMatrix()};
            var outputs = new List<Matrix<double>> {Math.Exp(-1.3).ToMatrix()};
            Assert.True(modelTester.Test(model, inputs, outputs));
        }
        [Fact]
        public void Exp_InputZero_OutputOne()
        {
            var model = ModelManager.Create("Exp", null);
            var modelTester = new ModelTester();
            var inputs = new List<Matrix<double>> {0.ToMatrix()};
            var outputs = new List<Matrix<double>> {1.ToMatrix()};
            Assert.True(modelTester.Test(model, inputs, outputs));
        }
        [Fact]
        public void Exp_InputMatrix_OutputMatrix()
        {
            var model = ModelManager.Create("Exp", null);
            var modelTester = new ModelTester();
            var inputs = new List<Matrix<double>> {Matrix<double>.Build.DenseOfArray(new double[2,3]{{1, -2, 3}, {2, -3, -4}})};
            Assert.True(modelTester.Test(model, inputs, inputs.ConvertAll(matrix => matrix.PointwiseExp())));
        }
    }
}