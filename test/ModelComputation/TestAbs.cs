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
    public class TestAbs
    {
        [Fact]
        public void Abs_InputScalar_OutputScalar()
        {
            var model = ModelManager.Create("Abs", null);
            var modelTester = new ModelTester();
            var inputs = new List<Matrix<double>> {1.3.ToMatrix()};
            Assert.True(modelTester.Test(model, inputs, inputs));
        }
        [Fact]
        public void Abs_InputNegativeScalar_OutputNegativeScalar()
        {
            var model = ModelManager.Create("Abs", null);
            var modelTester = new ModelTester();
            var inputs = new List<Matrix<double>> {-1.3.ToMatrix()};
            Assert.True(modelTester.Test(model, inputs, inputs.ConvertAll(scalar => -scalar)));
        }
        [Fact]
        public void Abs_InputZero_OutputZero()
        {
            var model = ModelManager.Create("Abs", null);
            var modelTester = new ModelTester();
            var inputs = new List<Matrix<double>> {0.ToMatrix()};
            Assert.True(modelTester.Test(model, inputs, inputs));
        }
        [Fact]
        public void Abs_InputMatrix_OutputMatrix()
        {
            var model = ModelManager.Create("Abs", null);
            var modelTester = new ModelTester();
            var inputs = new List<Matrix<double>> {Matrix<double>.Build.DenseOfArray(new double[2,3]{{1, -2, 3}, {2, -3, -4}})};
            Assert.True(modelTester.Test(model, inputs, inputs.ConvertAll(matrix => matrix.PointwiseAbs())));
        }
    }
}