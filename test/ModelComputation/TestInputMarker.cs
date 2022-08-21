/* Copyright (C) 2019-2022 Junruoyu Zheng. Home page: https://junruoyu-zheng.gitee.io/ligral

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
    public class TestInputMarker
    {
        [Fact]
        public void InputMarker_InputScalar_OutputScalar()
        {
            ControlInput.InputPool.Clear();
            ControlInput.InputHandles.Clear();
            ControlInput.IsOpenLoop = false;
            var model = ModelManager.Create("InputMarker", null);
            var dict = new Dictionary<string, object> {{"name", "myName"}};
            model.Configure(dict);
            var modelTester = new ModelTester();
            var inputs = new List<Matrix<double>> {1.3.ToMatrix()};
            var outputs = new List<Matrix<double>> {1.3.ToMatrix()};
            Assert.True(modelTester.Test(model, inputs, outputs));
            Assert.True(ControlInput.InputPool[0].Name == "myName");
        }
        [Fact]
        public void InputMarker_InputScalar_OpenLoop_OutputScalar()
        {
            ControlInput.InputPool.Clear();
            ControlInput.InputHandles.Clear();
            ControlInput.IsOpenLoop = true;
            var model = ModelManager.Create("InputMarker", null);
            var dict = new Dictionary<string, object> {{"name", "myName"}};
            model.Configure(dict);
            var modelTester = new ModelTester();
            var inputs = new List<Matrix<double>> {1.3.ToMatrix()};
            var outputs = new List<Matrix<double>> {-3.1.ToMatrix()};
            Assert.True(modelTester.Test(model, inputs, outputs, beforeRunning:()=>ControlInput.InputPool[0].OpenLoopInput = -3.1));
            Assert.True(ControlInput.InputPool[0].Name == "myName");
        }
        [Fact]
        public void InputMarker_InputMatrix_OutputMatrix()
        {
            ControlInput.InputPool.Clear();
            ControlInput.InputHandles.Clear();
            ControlInput.IsOpenLoop = false;
            var model = ModelManager.Create("InputMarker", null);
            var modelTester = new ModelTester();
            var inputs = new List<Matrix<double>> {Matrix<double>.Build.DenseOfArray(new double[2,3] {{1, 2.1, -23.2}, {0, -0.02, 10}})};
            Assert.True(modelTester.Test(model, inputs, inputs));
            Assert.True(ControlInput.InputPool.Zip(inputs[0].ToList()).All(pair => pair.First.Input==pair.Second));
        }
        [Fact]
        public void InputMarker_InputMatrix_OpenLoop_OutputMatrix()
        {
            ControlInput.InputPool.Clear();
            ControlInput.InputHandles.Clear();
            ControlInput.IsOpenLoop = true;
            var model = ModelManager.Create("InputMarker", null);
            var modelTester = new ModelTester();
            var inputs = new List<Matrix<double>> {Matrix<double>.Build.DenseOfArray(new double[2,3] {{1, 2.1, -23.2}, {0, -0.02, 10}})};
            var outputs = new List<Matrix<double>> {Matrix<double>.Build.Dense(2, 3, 0)};
            Assert.True(modelTester.Test(model, inputs, outputs));
            ControlInput.IsOpenLoop = false;
            Assert.True(ControlInput.InputPool.Zip(inputs[0].ToList()).All(pair => pair.First.Input==pair.Second));
        }
        [Fact]
        public void InputMarker_ParameterTypeWrong_CauseError()
        {
            ControlInput.InputPool.Clear();
            ControlInput.InputHandles.Clear();
            ControlInput.IsOpenLoop = false;
            var model = ModelManager.Create("InputMarker", null);
            var dict = new Dictionary<string, object> {{"col", "1"}, {"row", "10"}};
            Assert.Throws<ModelException>(() => model.Configure(dict));
        }
        [Fact]
        public void InputMarker_UnknownParameter_CauseError()
        {
            ControlInput.InputPool.Clear();
            ControlInput.InputHandles.Clear();
            ControlInput.IsOpenLoop = false;
            var model = ModelManager.Create("InputMarker", null);
            var dict = new Dictionary<string, object> {{"unknown", 0}};
            Assert.Throws<ModelException>(() => model.Configure(dict));
        }
        [Fact]
        public void InputMarker_InputMatrix_ExplicitShape_OutputMatrix()
        {
            ControlInput.InputPool.Clear();
            ControlInput.InputHandles.Clear();
            ControlInput.IsOpenLoop = false;
            var model = ModelManager.Create("InputMarker", null);
            var dict = new Dictionary<string, object> {{"row", 2}, {"col", 3}};
            model.Configure(dict);
            var modelTester = new ModelTester();
            var inputs = new List<Matrix<double>> {Matrix<double>.Build.DenseOfArray(new double[2,3] {{1, 2.1, -23.2}, {0, -0.02, 10}})};
            Assert.True(modelTester.Test(model, inputs, inputs));
            Assert.True(ControlInput.InputPool.Zip(inputs[0].ToList()).All(pair => pair.First.Input==pair.Second));
        }
        [Fact]
        public void InputMarker_InputMatrix_RowInconsistency_CauseError()
        {
            ControlInput.InputPool.Clear();
            ControlInput.InputHandles.Clear();
            ControlInput.IsOpenLoop = false;
            var model = ModelManager.Create("InputMarker", null);
            var dict = new Dictionary<string, object> {{"row", 3}, {"col", 3}};
            model.Configure(dict);
            var modelTester = new ModelTester();
            var inputs = new List<Matrix<double>> {Matrix<double>.Build.DenseOfArray(new double[2,3] {{1, 2.1, -23.2}, {0, -0.02, 10}})};
            Assert.Throws<ModelException>(()=>modelTester.TestInput(model, inputs));
        }
        [Fact]
        public void InputMarker_InputMatrix_ColumnInconsistency_CauseError()
        {
            ControlInput.InputPool.Clear();
            ControlInput.InputHandles.Clear();
            ControlInput.IsOpenLoop = false;
            var model = ModelManager.Create("InputMarker", null);
            var dict = new Dictionary<string, object> {{"row", 2}, {"col", 2}};
            model.Configure(dict);
            var modelTester = new ModelTester();
            var inputs = new List<Matrix<double>> {Matrix<double>.Build.DenseOfArray(new double[2,3] {{1, 2.1, -23.2}, {0, -0.02, 10}})};
            Assert.Throws<ModelException>(()=>modelTester.TestInput(model, inputs));
        }
        [Fact]
        public void InputMarker_DecimalShape_CauseError()
        {
            ControlInput.InputPool.Clear();
            ControlInput.InputHandles.Clear();
            ControlInput.IsOpenLoop = false;
            var model = ModelManager.Create("InputMarker", null);
            var dict = new Dictionary<string, object> {{"row", 2.3}, {"col", 3}};
            Assert.Throws<ModelException>(()=>model.Configure(dict));
        }
        [Fact]
        public void InputMarker_ZeroShape_CauseError()
        {
            ControlInput.InputPool.Clear();
            ControlInput.InputHandles.Clear();
            ControlInput.IsOpenLoop = false;
            var model = ModelManager.Create("InputMarker", null);
            var dict = new Dictionary<string, object> {{"row", 2}, {"col", 0}};
            Assert.Throws<ModelException>(()=>model.Configure(dict));
        }
        [Fact]
        public void InputMarker_NegativeShape_CauseError()
        {
            ControlInput.InputPool.Clear();
            ControlInput.InputHandles.Clear();
            ControlInput.IsOpenLoop = false;
            var model = ModelManager.Create("InputMarker", null);
            var dict = new Dictionary<string, object> {{"row", -2}, {"col", 1}};
            Assert.Throws<ModelException>(()=>model.Configure(dict));
        }
        [Fact]
        public void InputMarker_InputNothing_OutputZero()
        {
            ControlInput.InputPool.Clear();
            ControlInput.InputHandles.Clear();
            ControlInput.IsOpenLoop = false;
            var node = ModelManager.Create("InputMarker", null);
            var modelTester = new ModelTester();
            var outputs = new List<Matrix<double>> {0.ToMatrix()};
            Assert.True(modelTester.TestOutput(node, outputs, beforeChecking:()=>
            {
                ((IFixable)node).FixConnection();
                ModelManager.ModelPool.Last().Check();
            }, beforeRunning:() => 
            {
                ModelManager.ModelPool.Last().Propagate();
            }));
        }
        [Fact]
        public void InputMarker_InputNothing_ExplicitShape_OutputZero()
        {
            ControlInput.InputPool.Clear();
            ControlInput.InputHandles.Clear();
            ControlInput.IsOpenLoop = false;
            var node = ModelManager.Create("InputMarker", null);
            var modelTester = new ModelTester();
            var dict = new Dictionary<string, object> {{"row", 2}, {"col", 3}};
            node.Configure(dict);
            var outputs = new List<Matrix<double>> {Matrix<double>.Build.Dense(2, 3, 0)};
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