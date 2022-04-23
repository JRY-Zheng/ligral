/* Copyright (C) 2019-2021 Junruoyu Zheng. Home page: https://junruoyu-zheng.gitee.io/ligral

   Distributed under MIT license.
   See file LICENSE for detail or copy at https://opensource.org/licenses/MIT
*/

using System.Linq;
using System.Collections.Generic;
using Xunit;
using MathNet.Numerics.LinearAlgebra;
using Ligral.Component;
using Ligral.Simulation;

namespace Ligral.Tests.ModelTester
{
    public class TestEqualToZero
    {
        [Fact]
        public void EqualToZero_InputScalar()
        {
            Function.FunctionPool.Clear();
            Function.FunctionHandles.Clear();
            var model = ModelManager.Create("EqualToZero", null);
            var dict = new Dictionary<string, object> {{"name", "myName"}};
            model.Configure(dict);
            var modelTester = new ModelTester();
            var inputs = new List<Matrix<double>> {1.3.ToMatrix()};
            modelTester.TestInput(model, inputs);
            Assert.True(Function.FunctionPool[0].Value==1.3);
            Assert.True(Function.FunctionPool[0].Name == "myName");
        }
        [Fact]
        public void EqualToZero_InputMatrix()
        {
            Function.FunctionPool.Clear();
            Function.FunctionHandles.Clear();
            var model = ModelManager.Create("EqualToZero", null);
            var dict = new Dictionary<string, object> ();
            model.Configure(dict);
            var modelTester = new ModelTester();
            var inputs = new List<Matrix<double>> {Matrix<double>.Build.DenseOfArray(new double[2,3] {{1, 2.1, -23.2}, {0, -0.02, 10}})};
            modelTester.TestInput(model, inputs);
            Assert.True(Function.FunctionPool.Zip(inputs[0].ToList()).All(pair => pair.First.Value==pair.Second));
        }
        [Fact]
        public void EqualToZero_ParameterTypeWrong_CauseError()
        {
            Function.FunctionPool.Clear();
            Function.FunctionHandles.Clear();
            var model = ModelManager.Create("EqualToZero", null);
            var dict = new Dictionary<string, object> {{"col", "1"}};
            Assert.Throws<ModelException>(() => model.Configure(dict));
        }
        [Fact]
        public void EqualToZero_UnknownParameter_CauseError()
        {
            Function.FunctionPool.Clear();
            Function.FunctionHandles.Clear();
            var model = ModelManager.Create("EqualToZero", null);
            var dict = new Dictionary<string, object> {{"unknown", 0}};
            Assert.Throws<ModelException>(() => model.Configure(dict));
        }
        [Fact]
        public void EqualToZero_InputMatrix_ExplicitShape()
        {
            Function.FunctionPool.Clear();
            Function.FunctionHandles.Clear();
            var model = ModelManager.Create("EqualToZero", null);
            var dict = new Dictionary<string, object> {{"row", 2}, {"col", Matrix<double>.Build.Dense(1, 1, 3)}};
            model.Configure(dict);
            var modelTester = new ModelTester();
            var inputs = new List<Matrix<double>> {Matrix<double>.Build.DenseOfArray(new double[2,3] {{1, 2.1, -23.2}, {0, -0.02, 10}})};
            modelTester.TestInput(model, inputs);
            Assert.True(Function.FunctionPool.Zip(inputs[0].ToList()).All(pair => pair.First.Value==pair.Second));
        }
        [Fact]
        public void EqualToZero_InputMatrix_RowInconsistency_CauseError()
        {
            Function.FunctionPool.Clear();
            Function.FunctionHandles.Clear();
            var model = ModelManager.Create("EqualToZero", null);
            var dict = new Dictionary<string, object> {{"row", 3}, {"col", 3}};
            model.Configure(dict);
            var modelTester = new ModelTester();
            var inputs = new List<Matrix<double>> {Matrix<double>.Build.DenseOfArray(new double[2,3] {{1, 2.1, -23.2}, {0, -0.02, 10}})};
            Assert.Throws<ModelException>(()=>modelTester.TestInput(model, inputs));
        }
        [Fact]
        public void EqualToZero_InputMatrix_ColumnInconsistency_CauseError()
        {
            Function.FunctionPool.Clear();
            Function.FunctionHandles.Clear();
            var model = ModelManager.Create("EqualToZero", null);
            var dict = new Dictionary<string, object> {{"row", 2}, {"col", 2}};
            model.Configure(dict);
            var modelTester = new ModelTester();
            var inputs = new List<Matrix<double>> {Matrix<double>.Build.DenseOfArray(new double[2,3] {{1, 2.1, -23.2}, {0, -0.02, 10}})};
            Assert.Throws<ModelException>(()=>modelTester.TestInput(model, inputs));
        }
        [Fact]
        public void EqualToZero_DecimalShape_CauseError()
        {
            Function.FunctionPool.Clear();
            Function.FunctionHandles.Clear();
            var model = ModelManager.Create("EqualToZero", null);
            var dict = new Dictionary<string, object> {{"row", 2.3}, {"col", 3}};
            Assert.Throws<ModelException>(()=>model.Configure(dict));
        }
        [Fact]
        public void EqualToZero_NegativeShape_CauseError()
        {
            Function.FunctionPool.Clear();
            Function.FunctionHandles.Clear();
            var model = ModelManager.Create("EqualToZero", null);
            var dict = new Dictionary<string, object> {{"row", -10.0}, {"col", 10.0}};
            Assert.Throws<ModelException>(()=>model.Configure(dict));
        }
        [Fact]
        public void EqualToZero_ZeroShape_CauseError()
        {
            Function.FunctionPool.Clear();
            Function.FunctionHandles.Clear();
            var model = ModelManager.Create("EqualToZero", null);
            var dict = new Dictionary<string, object> {{"row", 0.0}, {"col", 10.0}};
            Assert.Throws<ModelException>(()=>model.Configure(dict));
        }
        [Fact]
        public void EqualToZero_MatrixShape_CauseError()
        {
            Function.FunctionPool.Clear();
            Function.FunctionHandles.Clear();
            var model = ModelManager.Create("EqualToZero", null);
            var dict = new Dictionary<string, object> {{"row", Matrix<double>.Build.Dense(3, 2, 1)}, {"col", 10.0}};
            Assert.Throws<ModelException>(()=>model.Configure(dict));
        }
    }
}