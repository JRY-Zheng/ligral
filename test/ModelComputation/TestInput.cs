/* Copyright (C) 2019-2021 Junruoyu Zheng. Home page: https://junruoyu-zheng.gitee.io/ligral

   Distributed under MIT license.
   See file LICENSE for detail or copy at https://opensource.org/licenses/MIT
*/

using System.Linq;
using System.Collections.Generic;
using Xunit;
using MathNet.Numerics.LinearAlgebra;
using Ligral.Component;

namespace Ligral.Tests.ModelTester
{
    public class TestInput
    {
        [Fact]
        public void Input_InputScalar_OutputScalar()
        {
            var node = ModelManager.Create("<Input>");
            var modelTester = new ModelTester();
            var inputs = new List<Matrix<double>> {1.ToMatrix()};
            Assert.True(modelTester.Test(node, inputs, inputs));
        }
        [Fact]
        public void Input_InputMatrix_OutputMatrix()
        {
            var node = ModelManager.Create("<Input>");
            var modelTester = new ModelTester();
            var inputs = new List<Matrix<double>> {Matrix<double>.Build.DenseOfArray(new double[2,3]{{1, 2, 3}, {2, 3, 4}})};
            Assert.True(modelTester.Test(node, inputs, inputs));
        }
        [Fact]
        public void Input_InputNothing_SetDefaultSource_OutputMatrix()
        {
            var node = ModelManager.Create("<Input>");
            var mat = Matrix<double>.Build.DenseOfArray(new double[2,3]{{1, 2, 3}, {2, 3, 4}});
            var inputType = typeof(ModelManager).Assembly.GetType("Ligral.Component.Models.Input");
            var setDefaultSourceMethod = inputType.GetMethod("SetDefaultSource");
            setDefaultSourceMethod.Invoke(node, new object[] {mat});
            var modelTester = new ModelTester();
            var outputs = new List<Matrix<double>> {mat};
            Assert.True(modelTester.TestOutput(node, outputs, beforeChecking:()=>
            {
                ((IFixable)node).FixConnection();
                ModelManager.ModelPool.Last().Check();
            }, beforeRunning:() => 
            {
                ModelManager.ModelPool.Last().Propagate();
            }));
        }
    }
}