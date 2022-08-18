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
    public class TestHSplit
    {
        [Fact]
        public void HSplit_InputScalar_OutputScalar()
        {
            var model = ModelManager.Create("HSplit", null);
            var modelTester = new ModelTester();
            var inputs = new List<Matrix<double>> {1.3.ToMatrix()};
            var outputs = new List<Matrix<double>> {1.3.ToMatrix()};
            Assert.True(modelTester.Test(model, inputs, outputs));
        }
        [Fact]
        public void HSplit_InputRowVector_OutputScalars()
        {
            var model = ModelManager.Create("HSplit", null);
            var modelTester = new ModelTester();
            var matrix = Matrix<double>.Build.DenseOfArray(new double[1,3]{{-4.1, 5.1, -6.1}});
            var inputs = new List<Matrix<double>> {matrix};
            var outputs = new List<Matrix<double>> {-4.1.ToMatrix(), 5.1.ToMatrix(), -6.1.ToMatrix()};
            Assert.True(modelTester.Test(model, inputs, outputs));
        }
        [Fact]
        public void HSplit_InputRowVector_OutputInconsistency_CauseError()
        {
            var model = ModelManager.Create("HSplit", null);
            var modelTester = new ModelTester();
            var matrix = Matrix<double>.Build.DenseOfArray(new double[1,3]{{-4.1, 5.1, -6.1}});
            var inputs = new List<Matrix<double>> {matrix};
            var outputs = new List<Matrix<double>> {-4.1.ToMatrix(), 5.1.ToMatrix()};
            Assert.Throws<ModelException>(()=>modelTester.Test(model, inputs, outputs));
        }
        [Fact]
        public void HSplit_InputColumnVector_OutputColumnVector()
        {
            var model = ModelManager.Create("HSplit", null);
            var modelTester = new ModelTester();
            var matrix = Matrix<double>.Build.DenseOfArray(new double[3,1]{{-4.1}, {5.1}, {-6.1}});
            var inputs = new List<Matrix<double>> {matrix};
            Assert.True(modelTester.Test(model, inputs, inputs));
        }
        [Fact]
        public void HSplit_InputColumnVector_OutputInconsistency_CauseError()
        {
            var model = ModelManager.Create("HSplit", null);
            var modelTester = new ModelTester();
            var matrix = Matrix<double>.Build.DenseOfArray(new double[3,1]{{-4.1}, {5.1}, {-6.1}});
            var inputs = new List<Matrix<double>> {matrix};
            var outputs = new List<Matrix<double>> {-4.1.ToMatrix(), 5.1.ToMatrix()};
            Assert.Throws<ModelException>(()=>modelTester.Test(model, inputs, outputs));
        }
        [Fact]
        public void HSplit_InputMatrix_OutputColumnVectors()
        {
            var model = ModelManager.Create("HSplit", null);
            var modelTester = new ModelTester();
            var matrix = Matrix<double>.Build.DenseOfArray(new double[2,3]{{1, 2, 3}, {-4.1, 5.1, -6.1}});
            var vector1 = Matrix<double>.Build.DenseOfArray(new double[2,1]{{1}, {-4.1}});
            var vector2 = Matrix<double>.Build.DenseOfArray(new double[2,1]{{2}, {5.1}});
            var vector3 = Matrix<double>.Build.DenseOfArray(new double[2,1]{{3}, {-6.1}});
            var inputs = new List<Matrix<double>> {matrix};
            var outputs = new List<Matrix<double>> {vector1, vector2, vector3};
            Assert.True(modelTester.Test(model, inputs, outputs));
        }
        [Fact]
        public void HSplit_InputMatrix_OutputInconsistency_CauseError()
        {
            var model = ModelManager.Create("HSplit", null);
            var modelTester = new ModelTester();
            var matrix = Matrix<double>.Build.DenseOfArray(new double[2,3]{{1, 2, 3}, {-4.1, 5.1, -6.1}});
            var vector1 = Matrix<double>.Build.DenseOfArray(new double[2,1]{{1}, {-4.1}});
            var vector2 = Matrix<double>.Build.DenseOfArray(new double[2,1]{{2}, {5.1}});
            var inputs = new List<Matrix<double>> {matrix};
            var outputs = new List<Matrix<double>> {vector1, vector2};
            Assert.Throws<ModelException>(()=>modelTester.Test(model, inputs, outputs));
        }
    }
}