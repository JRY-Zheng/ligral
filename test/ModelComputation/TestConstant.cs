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
    public class TestConstant
    {
        [Fact]
        public void Constant_ScalarValue_OutputScalar()
        {
            var model = ModelManager.Create("Constant", null);
            var dict = new Dictionary<string, object> {{"value", 1.2}};
            model.Configure(dict);
            var modelTester = new ModelTester();
            var outputs = new List<Matrix<double>> {1.2.ToMatrix()};
            Assert.True(modelTester.TestOutput(model, outputs));
        }
        [Fact]
        public void Constant_MatrixValue_OutputMatrix()
        {
            var model = ModelManager.Create("Constant", null);
            var matrix = Matrix<double>.Build.DenseOfArray(new double[2, 3]{{1, 3.33, -5}, {0, 100.2, 0.002}});
            var dict = new Dictionary<string, object> {{"value", matrix}};
            model.Configure(dict);
            var modelTester = new ModelTester();
            var outputs = new List<Matrix<double>> {matrix};
            Assert.True(modelTester.TestOutput(model, outputs));
        }
        [Fact]
        public void Constant_NoValue_CauseError()
        {
            var model = ModelManager.Create("Constant", null);
            var dict = new Dictionary<string, object> {};
            // configure is a must procedure even if there is nothing to config
            Assert.Throws<ModelException>(()=>model.Configure(dict));
        }
    }
}