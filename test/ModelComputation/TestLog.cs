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
    public class TestLog
    {
        [Fact]
        public void Log_InputOne_OutputZero()
        {
            var model = ModelManager.Create("Log", null);
            var modelTester = new ModelTester();
            var inputs = new List<Matrix<double>> {1.ToMatrix()};
            var outputs = new List<Matrix<double>> {0.ToMatrix()};
            Assert.True(modelTester.Test(model, inputs, outputs));
        }
        [Fact]
        public void Log_InputLargeScalar_OutputPositiveScalar()
        {
            var model = ModelManager.Create("Log", null);
            var modelTester = new ModelTester();
            var inputs = new List<Matrix<double>> {100.ToMatrix()};
            var outputs = new List<Matrix<double>> {Math.Log(100).ToMatrix()};
            Assert.True(modelTester.Test(model, inputs, outputs));
        }
        [Fact]
        public void Log_InputSmallPositiveScalar_OutputNegativeScalar()
        {
            var model = ModelManager.Create("Log", null);
            var modelTester = new ModelTester();
            var inputs = new List<Matrix<double>> {0.01.ToMatrix()};
            var outputs = new List<Matrix<double>> {Math.Log(0.01).ToMatrix()};
            Assert.True(modelTester.Test(model, inputs, outputs));
        }
        [Fact]
        public void Log_InputNegativeScalar_CauseError()
        {
            var model = ModelManager.Create("Log", null);
            var modelTester = new ModelTester();
            var inputs = new List<Matrix<double>> {-1.3.ToMatrix()};
            Assert.Throws<ModelException>(() => modelTester.TestInput(model, inputs));
        }
        [Fact]
        public void Log_InputZero_CauseError()
        {
            var model = ModelManager.Create("Log", null);
            var modelTester = new ModelTester();
            var inputs = new List<Matrix<double>> {0.ToMatrix()};
            Assert.Throws<ModelException>(() => modelTester.TestInput(model, inputs));
        }
        [Fact]
        public void Log_InputMatrix_OutputMatrix()
        {
            var model = ModelManager.Create("Log", null);
            var modelTester = new ModelTester();
            var inputs = new List<Matrix<double>> {Matrix<double>.Build.DenseOfArray(new double[2,3]{{1, 0.1, 0.001}, {0.707, 10.707, 100.56}})};
            Assert.True(modelTester.Test(model, inputs, inputs.ConvertAll(matrix => matrix.PointwiseLog())));
        }
        [Fact]
        public void Log_InputMatrixWithNonPositiveValue_CauseError()
        {
            var model = ModelManager.Create("Log", null);
            var modelTester = new ModelTester();
            var inputs = new List<Matrix<double>> {Matrix<double>.Build.DenseOfArray(new double[2,3]{{1, -1, 0}, {0.707, -0.707, -1.3}})};
            Assert.Throws<ModelException>(() => modelTester.TestInput(model, inputs));
        }
    }
}