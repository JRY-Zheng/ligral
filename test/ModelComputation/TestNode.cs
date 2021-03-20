/* Copyright (C) 2019-2021 Junruoyu Zheng. Home page: https://junruoyu-zheng.gitee.io/ligral

   Distributed under MIT license.
   See file LICENSE for detail or copy at https://opensource.org/licenses/MIT
*/

using System.Collections.Generic;
using Xunit;
using MathNet.Numerics.LinearAlgebra;
using Ligral.Component;

namespace Ligral.Tests.ModelTester
{
    public class TestNode
    {
        [Fact]
        public void Node_InputEqualToOutput()
        {
            var node = ModelManager.Create("Node");
            var modelTester = new ModelTester();
            var inputs = new List<Matrix<double>> {1.ToMatrix()};
            Assert.True(modelTester.Test(node, inputs, inputs));
        }
        [Fact]
        public void Node_InputEqualToOutput2()
        {
            var node = ModelManager.Create("Node");
            var modelTester = new ModelTester();
            var inputs = new List<Matrix<double>> {Matrix<double>.Build.DenseOfArray(new double[2,3]{{1, 2, 3}, {2, 3, 4}})};
            Assert.True(modelTester.Test(node, inputs, inputs));
        }
    }
}