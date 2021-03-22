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
    public class TestAdd
    {
        [Fact]
        public void Add_InputTwoScalars_OutputScalar()
        {
            var model = ModelManager.Create("Add");
            var modelTester = new ModelTester();
            var inputs = new List<Matrix<double>> {1.2.ToMatrix(), -1.2.ToMatrix()};
            var outputs = new List<Matrix<double>> {0.ToMatrix()};
            Assert.True(modelTester.Test(model, inputs, outputs));
        }
        [Fact]
        public void Add_InputScalarAddMatrix_OutputMatrix()
        {
            var model = ModelManager.Create("Add");
            var modelTester = new ModelTester();
            var left = 1.2.ToMatrix();
            var right = Matrix<double>.Build.DenseOfArray(new double[2,3]{{1, 2, 3}, {2, 3, 4}});
            var inputs = new List<Matrix<double>> {left, right};
            var outputs = new List<Matrix<double>> {left.MatAdd(right)};
            Assert.True(modelTester.Test(model, inputs, outputs));
        }
        [Fact]
        public void Add_InputVectorAddMatrix_OutputMatrix()
        {
            var model = ModelManager.Create("Add");
            var modelTester = new ModelTester();
            var left = Matrix<double>.Build.DenseOfArray(new double[1,3]{{1, 2, 3}});
            var right = Matrix<double>.Build.DenseOfArray(new double[2,3]{{1, 2, 3}, {2, 3, 4}});
            var inputs = new List<Matrix<double>> {left, right};
            var outputs = new List<Matrix<double>> {left.MatAdd(right)};
            Assert.True(modelTester.Test(model, inputs, outputs));
        }
        [Fact]
        public void Add_InputMatrixAddMatrix_OutputMatrix()
        {
            var model = ModelManager.Create("Add");
            var modelTester = new ModelTester();
            var left = Matrix<double>.Build.DenseOfArray(new double[2,3]{{1, 2, 3}, {2, 3, 4}});
            var right = Matrix<double>.Build.DenseOfArray(new double[2,3]{{1, 2, 3}, {2, 3, 4}});
            var inputs = new List<Matrix<double>> {left, right};
            var outputs = new List<Matrix<double>> {left.MatAdd(right)};
            Assert.True(modelTester.Test(model, inputs, outputs));
        }
        [Fact]
        public void Add_InputShapeInconsistency_CauseError()
        {
            var model = ModelManager.Create("Add");
            var modelTester = new ModelTester();
            var left = Matrix<double>.Build.DenseOfArray(new double[3,2]{{1, 2}, {2, 3}, {3, 4}});
            var right = Matrix<double>.Build.DenseOfArray(new double[2,3]{{1, 2, 3}, {2, 3, 4}});
            var inputs = new List<Matrix<double>> {left, right};
            Assert.Throws<ModelException>(() => modelTester.TestInput(model, inputs));
        }
        [Fact]
        public void Add_InputLessThanTwo_CauseError()
        {
            var model = ModelManager.Create("Add");
            var modelTester = new ModelTester();
            var left = Matrix<double>.Build.DenseOfArray(new double[3,2]{{1, 2}, {2, 3}, {3, 4}});
            var inputs = new List<Matrix<double>> {left};
            Assert.Throws<ModelException>(() => modelTester.TestInput(model, inputs));
        }
        [Fact]
        public void Add_InputMoreThanTwo_CauseError()
        {
            var model = ModelManager.Create("Add");
            var modelTester = new ModelTester();
            var left = Matrix<double>.Build.DenseOfArray(new double[3,2]{{1, 2}, {2, 3}, {3, 4}});
            var inputs = new List<Matrix<double>> {left, left, left};
            Assert.Throws<ModelException>(() => modelTester.TestInput(model, inputs));
        }
    }
}