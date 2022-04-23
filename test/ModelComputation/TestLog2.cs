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
    public class TestLog2
    {
        [Fact]
        public void Log2_InputOneAndPositiveScalar_OutputZero()
        {
            var model = ModelManager.Create("Log2", null);
            var modelTester = new ModelTester();
            var inputs = new List<Matrix<double>> {1.ToMatrix(), 1.8.ToMatrix()};
            var outputs = new List<Matrix<double>> {0.ToMatrix()};
            Assert.True(modelTester.Test(model, inputs, outputs));
        }
        [Fact]
        public void Log2_InputPositiveAndNegative_CauseError()
        {
            var model = ModelManager.Create("Log2", null);
            var modelTester = new ModelTester();
            var inputs = new List<Matrix<double>> {1.3.ToMatrix(), -0.3.ToMatrix()};
            Assert.Throws<ModelException>(() => modelTester.TestInput(model, inputs));
        }
        [Fact]
        public void Log2_InputNegativeAndPositive_CauseError()
        {
            var model = ModelManager.Create("Log2", null);
            var modelTester = new ModelTester();
            var inputs = new List<Matrix<double>> {-1.3.ToMatrix(), 1.8.ToMatrix()};
            Assert.Throws<ModelException>(() => modelTester.TestInput(model, inputs));
        }
        [Fact]
        public void Log2_InputPositiveAndZero_CauseError()
        {
            var model = ModelManager.Create("Log2", null);
            var modelTester = new ModelTester();
            var inputs = new List<Matrix<double>> {1.3.ToMatrix(), 0.ToMatrix()};
            Assert.Throws<ModelException>(() => modelTester.TestInput(model, inputs));
        }
        [Fact]
        public void Log2_InputPositiveAndOne_CauseError()
        {
            var model = ModelManager.Create("Log2", null);
            var modelTester = new ModelTester();
            var inputs = new List<Matrix<double>> {1.3.ToMatrix(), 1.ToMatrix()};
            Assert.Throws<ModelException>(() => modelTester.TestInput(model, inputs));
        }
        [Fact]
        public void Log2_InputVectorAndMatrix_OutputMatrix()
        {
            var model = ModelManager.Create("Log2", null);
            var modelTester = new ModelTester();
            var inputs = new List<Matrix<double>> 
            {
                Matrix<double>.Build.DenseOfArray(new double[1,3]{{1, 2, 3}}), 
                Matrix<double>.Build.DenseOfArray(new double[2,3]{{88.1, 2, 3.01}, {4.707, 50.6, 0.1}})
            };
            var outputs = new List<Matrix<double>> {inputs[0].Broadcast(inputs[1], Math.Log)};
            Assert.True(modelTester.Test(model, inputs, outputs));
        }
        [Fact]
        public void Log2_InputMatrix_OutputMatrix()
        {
            var model = ModelManager.Create("Log2", null);
            var modelTester = new ModelTester();
            var inputs = new List<Matrix<double>> 
            {
                Matrix<double>.Build.DenseOfArray(new double[2,3]{{1, 2, 3}, {4.707, 5.707, 100}}), 
                Matrix<double>.Build.DenseOfArray(new double[2,3]{{88.1, 2, 3.01}, {4.707, 50.6, 0.1}})
            };
            var outputs = new List<Matrix<double>> {inputs[0].Map2(Math.Log, inputs[1])};
            Assert.True(modelTester.Test(model, inputs, outputs));
        }
        [Fact]
        public void Log2_InputMatrix_ShapeInconsistency_CauseError()
        {
            var model = ModelManager.Create("Log2", null);
            var modelTester = new ModelTester();
            var inputs = new List<Matrix<double>> 
            {
                Matrix<double>.Build.DenseOfArray(new double[2,2]{{1, 2}, {1, 3}}), 
                Matrix<double>.Build.DenseOfArray(new double[2,3]{{88.1, 2, 3.01}, {4.707, 50.6, 0.1}})
            };
            Assert.Throws<ModelException>(() => modelTester.TestInput(model, inputs));
        }
    }
}