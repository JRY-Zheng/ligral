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
    public class Testa
    {
        [Fact]
        public void HStack_InputScalar_OutputScalar()
        {
            var model = ModelManager.Create("HStack", null);
            var modelTester = new ModelTester();
            var inputs = new List<Matrix<double>> {1.3.ToMatrix()};
            var outputs = new List<Matrix<double>> {1.3.ToMatrix()};
            Assert.True(modelTester.Test(model, inputs, outputs));
        }
        [Fact]
        public void HStack_InputScalars_OutputRowVector()
        {
            var model = ModelManager.Create("HStack", null);
            var modelTester = new ModelTester();
            var matrix = Matrix<double>.Build.DenseOfArray(new double[1,3]{{-4.1, 5.1, -6.1}});
            var inputs = new List<Matrix<double>> {-4.1.ToMatrix(), 5.1.ToMatrix(), -6.1.ToMatrix()};
            var outputs = new List<Matrix<double>> {matrix};
            Assert.True(modelTester.Test(model, inputs, outputs));
        }
        [Fact]
        public void HStack_InputColumnVectors_OutputMatrix()
        {
            var model = ModelManager.Create("HStack", null);
            var modelTester = new ModelTester();
            var matrix = Matrix<double>.Build.DenseOfArray(new double[2,3]{{1, 2, 3}, {-4.1, 5.1, -6.1}});
            var vector1 = Matrix<double>.Build.DenseOfArray(new double[2,1]{{1}, {-4.1}});
            var vector2 = Matrix<double>.Build.DenseOfArray(new double[2,1]{{2}, {5.1}});
            var vector3 = Matrix<double>.Build.DenseOfArray(new double[2,1]{{3}, {-6.1}});
            var inputs = new List<Matrix<double>> {vector1, vector2, vector3};
            var outputs = new List<Matrix<double>> {matrix};
            Assert.True(modelTester.Test(model, inputs, outputs));
        }
        [Fact]
        public void HStack_InputColumnVectors_ColumnNumberInconsistency_CauseError()
        {
            var model = ModelManager.Create("HStack", null);
            var modelTester = new ModelTester();
            var vector1 = Matrix<double>.Build.DenseOfArray(new double[2,1]{{1}, {-4.1}});
            var vector2 = Matrix<double>.Build.DenseOfArray(new double[3,1]{{2}, {5.1}, {3}});
            var vector3 = Matrix<double>.Build.DenseOfArray(new double[2,1]{{3}, {-6.1}});
            var inputs = new List<Matrix<double>> {vector1, vector2, vector3};
            Assert.Throws<ModelException>(() => modelTester.TestInput(model, inputs));
        }
        [Fact]
        public void HStack_InputMatrices_OutputMatrix()
        {
            var model = ModelManager.Create("HStack", null);
            var modelTester = new ModelTester();
            var matrix = Matrix<double>.Build.DenseOfArray(new double[2,3]{{1, 2, 3}, {-4.1, 5.1, -6.1}});
            var matrix1 = Matrix<double>.Build.DenseOfArray(new double[2,1]{{1}, {-4.1}});
            var matrix2 = Matrix<double>.Build.DenseOfArray(new double[2,2]{{2, 3}, {5.1, -6.1}});
            var inputs = new List<Matrix<double>> {matrix1, matrix2};
            var outputs = new List<Matrix<double>> {matrix};
            Assert.True(modelTester.Test(model, inputs, outputs));
        }
    }
}