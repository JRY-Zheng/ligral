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
    public class TestDiv
    {
        [Fact]
        public void Div_InputTwoScalars_OutputScalar()
        {
            var model = ModelManager.Create("Div", null);
            var modelTester = new ModelTester();
            var inputs = new List<Matrix<double>> {1.2.ToMatrix(), -1.2.ToMatrix()};
            var outputs = new List<Matrix<double>> {-1.ToMatrix()};
            Assert.True(modelTester.Test(model, inputs, outputs));
        }
        [Fact]
        public void Div_InputScalarDivZero_CauseError()
        {
            var model = ModelManager.Create("Div", null);
            var modelTester = new ModelTester();
            var inputs = new List<Matrix<double>> {1.2.ToMatrix(), 0.ToMatrix()};
            Assert.Throws<ModelException>(() => modelTester.TestInput(model, inputs));
        }
        [Fact]
        public void Div_InputScalarDivMatrix_OutputMatrix()
        {
            var model = ModelManager.Create("Div", null);
            var modelTester = new ModelTester();
            var left = 1.2.ToMatrix();
            var right = Matrix<double>.Build.DenseOfArray(new double[2,3]{{1, 2, 3}, {2, 3, 4}});
            var inputs = new List<Matrix<double>> {left, right};
            var outputs = new List<Matrix<double>> {left.RightDiv(right)};
            Assert.True(modelTester.Test(model, inputs, outputs));
        }
        [Fact]
        public void Div_InputMatrixDivScalar_OutputMatrix()
        {
            var model = ModelManager.Create("Div", null);
            var modelTester = new ModelTester();
            var left = Matrix<double>.Build.DenseOfArray(new double[2,3]{{1, 2, 3}, {2, 3, 4}});
            var right = 1.2.ToMatrix();
            var inputs = new List<Matrix<double>> {left, right};
            var outputs = new List<Matrix<double>> {left.RightDiv(right)};
            Assert.True(modelTester.Test(model, inputs, outputs));
        }
        [Fact]
        public void Div_InputMatrixDivZero_CauseError()
        {
            var model = ModelManager.Create("Div", null);
            var modelTester = new ModelTester();
            var left = Matrix<double>.Build.DenseOfArray(new double[2,3]{{1, 2, 3}, {2, 3, 4}});
            var inputs = new List<Matrix<double>> {left, 0.ToMatrix()};
            Assert.Throws<ModelException>(() => modelTester.TestInput(model, inputs));
        }
        [Fact]
        public void Div_InputMatrixDivMatrix_OutputMatrix()
        {
            var model = ModelManager.Create("Div", null);
            var modelTester = new ModelTester();
            var left = Matrix<double>.Build.DenseOfArray(new double[1,3]{{1, 2, 3}});
            var right = Matrix<double>.Build.DenseOfArray(new double[3,3]{{1, 2, 3}, {2, 3, 4}, {-1.1, -2, -4}});
            var inputs = new List<Matrix<double>> {left, right};
            var outputs = new List<Matrix<double>> {left.RightDiv(right)};
            Assert.True(modelTester.Test(model, inputs, outputs));
        }
        [Fact]
        public void Div_InputMatrixDivNonSquareMatrix_CauseError()
        {
            var model = ModelManager.Create("Div", null);
            var modelTester = new ModelTester();
            var left = Matrix<double>.Build.DenseOfArray(new double[1,3]{{1, 2, 3}});
            var right = Matrix<double>.Build.DenseOfArray(new double[2,3]{{1, 2, 3}, {2, 3, 4}});
            var inputs = new List<Matrix<double>> {left, right};
            Assert.Throws<ModelException>(() => modelTester.TestInput(model, inputs));
        }
        [Fact]
        public void Div_InputMatrixDivSingularMatrix_CauseError()
        {
            var model = ModelManager.Create("Div", null);
            var modelTester = new ModelTester();
            var left = Matrix<double>.Build.DenseOfArray(new double[1,3]{{1, 2, 3}});
            var right = Matrix<double>.Build.DenseOfArray(new double[3,3]{{1, 2, 3}, {2, 3, 4}, {-1, -2, -3}});
            var inputs = new List<Matrix<double>> {left, right};
            Assert.Throws<ModelException>(() => modelTester.TestInput(model, inputs));
        }
        [Fact]
        public void Div_InputShapeInconsistency_CauseError()
        {
            var model = ModelManager.Create("Div", null);
            var modelTester = new ModelTester();
            var left = Matrix<double>.Build.DenseOfArray(new double[3,2]{{1, 2}, {2, 3}, {3, 4}});
            var right = Matrix<double>.Build.DenseOfArray(new double[3,3]{{1, 2, 3}, {2, 3, 4}, {3, 4, 5}});
            var inputs = new List<Matrix<double>> {left, right};
            Assert.Throws<ModelException>(() => modelTester.TestInput(model, inputs));
        }
        [Fact]
        public void Div_InputLessThanTwo_CauseError()
        {
            var model = ModelManager.Create("Div", null);
            var modelTester = new ModelTester();
            var left = Matrix<double>.Build.DenseOfArray(new double[3,2]{{1, 2}, {2, 3}, {3, 4}});
            var inputs = new List<Matrix<double>> {left};
            Assert.Throws<ModelException>(() => modelTester.TestInput(model, inputs));
        }
        [Fact]
        public void Div_InputMoreThanTwo_CauseError()
        {
            var model = ModelManager.Create("Div", null);
            var modelTester = new ModelTester();
            var left = Matrix<double>.Build.DenseOfArray(new double[3,2]{{1, 2}, {2, 3}, {3, 4}});
            var inputs = new List<Matrix<double>> {left, left, left};
            Assert.Throws<ModelException>(() => modelTester.TestInput(model, inputs));
        }
    }
}