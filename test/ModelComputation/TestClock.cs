/* Copyright (C) 2019-2021 Junruoyu Zheng. Home page: https://junruoyu-zheng.gitee.io/ligral

   Distributed under MIT license.
   See file LICENSE for detail or copy at https://opensource.org/licenses/MIT
*/

using System.Collections.Generic;
using Xunit;
using MathNet.Numerics.LinearAlgebra;
using Ligral.Component;
using Ligral.Simulation;

namespace Ligral.Tests.ModelTester
{
    public class TestClock
    {
        [Fact]
        public void Clock_OutputTime()
        {
            var model = ModelManager.Create("Clock", null);
            var modelTester = new ModelTester();
            Solver.Time = 1.3;
            var outputs = new List<Matrix<double>> {1.3.ToMatrix()};
            Assert.True(modelTester.TestOutput(model, outputs));
        }
    }
}