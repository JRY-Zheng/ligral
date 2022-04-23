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
    public class TestAtan2
    {
        [Fact]
        public void Atan2_InputZeroAndOne_OutputZero()
        {
            var model = ModelManager.Create("Atan2", null);
            var modelTester = new ModelTester();
            var inputs = new List<Matrix<double>> {0.ToMatrix(), 1.ToMatrix()};
            var outputs = new List<Matrix<double>> {0.ToMatrix()};
            Assert.True(modelTester.Test(model, inputs, outputs));
        }
        [Fact]
        public void Atan2_InputMinusOneAndZero_OutputMinus90Deg()
        {
            var model = ModelManager.Create("Atan2", null);
            var modelTester = new ModelTester();
            var inputs = new List<Matrix<double>> {-1.ToMatrix(), 0.ToMatrix()};
            var outputs = new List<Matrix<double>> {-Math.PI/2.ToMatrix()};
            Assert.True(modelTester.Test(model, inputs, outputs));
        }
        [Fact]
        public void Atan2_InputOneAndZero_Output90Deg()
        {
            var model = ModelManager.Create("Atan2", null);
            var modelTester = new ModelTester();
            var inputs = new List<Matrix<double>> {1.ToMatrix(), 0.ToMatrix()};
            var outputs = new List<Matrix<double>> {Math.PI/2.ToMatrix()};
            Assert.True(modelTester.Test(model, inputs, outputs));
        }
        [Fact]
        public void Atan2_InputTwoScalar_OutputBetweenMinusOneAndOne()
        {
            var model = ModelManager.Create("Atan2", null);
            var modelTester = new ModelTester();
            var inputs = new List<Matrix<double>> {123.45.ToMatrix(), -0.118.ToMatrix()};
            var outputs = new List<Matrix<double>> {Math.Atan2(123.45, -0.118).ToMatrix()};
            Assert.True(modelTester.Test(model, inputs, outputs));
        }
        [Fact]
        public void Atan2_InputVectorAndMatrix_OutputMatrix()
        {
            var model = ModelManager.Create("Atan2", null);
            var modelTester = new ModelTester();
            var inputs = new List<Matrix<double>> 
            {
                Matrix<double>.Build.DenseOfArray(new double[1,3]{{1, 2, 3}}), 
                Matrix<double>.Build.DenseOfArray(new double[2,3]{{88.1, 2, 3.01}, {4.707, 50.6, 0.1}})
            };
            var outputs = new List<Matrix<double>> {inputs[0].Broadcast(inputs[1], Math.Atan2)};
            Assert.True(modelTester.Test(model, inputs, outputs));
        }
        [Fact]
        public void Atan2_InputMatrix_OutputMatrix()
        {
            var model = ModelManager.Create("Atan2", null);
            var modelTester = new ModelTester();
            var inputs = new List<Matrix<double>> 
            {
                Matrix<double>.Build.DenseOfArray(new double[2,3]{{1, -2, 3}, {-4.707, 5.707, 100}}), 
                Matrix<double>.Build.DenseOfArray(new double[2,3]{{-88.1, -2, 3.01}, {4.707, 50.6, 0}})
            };
            var outputs = new List<Matrix<double>> {inputs[0].Map2(Math.Atan2, inputs[1])};
            Assert.True(modelTester.Test(model, inputs, outputs));
        }
        [Fact]
        public void Atan2_InputMatrix_ShapeInconsistency_CauseError()
        {
            var model = ModelManager.Create("Atan2", null);
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