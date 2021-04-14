/* Copyright (C) 2019-2021 Junruoyu Zheng. Home page: https://junruoyu-zheng.gitee.io/ligral

   Distributed under MIT license.
   See file LICENSE for detail or copy at https://opensource.org/licenses/MIT
*/

using System.Reflection;
using System.Linq;
using System.Collections.Generic;
using Xunit;
using MathNet.Numerics.LinearAlgebra;
using Ligral.Component;
using Ligral.Simulation;

namespace Ligral.Tests.ModelTester
{
    public class TestMemory
    {
        [Fact]
        public void Memory_InputScalar_OutputZero()
        {
            State.StatePool.Clear();
            State.StateHandles.Clear();
            var model = ModelManager.Create("Memory");
            var modelTester = new ModelTester();
            var inputs = new List<Matrix<double>> {1.3.ToMatrix()};
            var outputs = new List<Matrix<double>> {0.ToMatrix()};
            Assert.True(modelTester.Test(model, inputs, outputs));
            var stackInfo = model.GetType().GetField("stack", BindingFlags.NonPublic | BindingFlags.Instance);
            Assert.True(((Matrix<double>) stackInfo.GetValue(model)).EqualTo(1.3.ToMatrix()));
        }
        [Fact]
        public void Memory_InputZero_InitializeOne_OutputOne()
        {
            State.StatePool.Clear();
            State.StateHandles.Clear();
            var model = ModelManager.Create("Memory");
            var dict = new Dictionary<string, object> {{"initial", 1.0}};
            model.Configure(dict);
            var modelTester = new ModelTester();
            var inputs = new List<Matrix<double>> {-1.ToMatrix()};
            var outputs = new List<Matrix<double>> {1.ToMatrix()};
            var stackInfo = model.GetType().GetField("stack", BindingFlags.NonPublic | BindingFlags.Instance);
            Assert.True(modelTester.Test(model, inputs, outputs, beforeRunning: () => 
            {
                Assert.True(((Matrix<double>) stackInfo.GetValue(model)).EqualTo(1.ToMatrix()));
            }));
            Assert.True(((Matrix<double>) stackInfo.GetValue(model)).EqualTo(-1.ToMatrix()));
        }
        [Fact]
        public void Memory_InputMatrix_OutputMatrix()
        {
            State.StatePool.Clear();
            State.StateHandles.Clear();
            var model = ModelManager.Create("Memory");
            var modelTester = new ModelTester();
            var inputs = new List<Matrix<double>> {Matrix<double>.Build.DenseOfArray(new double[2,3] {{1, 2.1, -23.2}, {0, -0.02, 10}})};
            var outputs = new List<Matrix<double>> {Matrix<double>.Build.Dense(2, 3, 0)};
            Assert.True(modelTester.Test(model, inputs, outputs));
            var stackInfo = model.GetType().GetField("stack", BindingFlags.NonPublic | BindingFlags.Instance);
            Assert.True(((Matrix<double>) stackInfo.GetValue(model)).EqualTo(inputs[0]));
        }
        [Fact]
        public void Memory_InputMatrix_InitializeMatrix_OutputMatrix()
        {
            State.StatePool.Clear();
            State.StateHandles.Clear();
            var model = ModelManager.Create("Memory");
            var initial = Matrix<double>.Build.DenseOfArray(new double[2,3] {{1, 20.1, -23.2}, {0, -15.02, 10}});
            var dict = new Dictionary<string, object> {{"initial", initial}};
            model.Configure(dict);
            var modelTester = new ModelTester();
            var inputs = new List<Matrix<double>> {Matrix<double>.Build.DenseOfArray(new double[2,3] {{-1, 2.1, 23.2}, {0.9, -1.02, -10}})};
            var outputs = new List<Matrix<double>> {initial};
            var stackInfo = model.GetType().GetField("stack", BindingFlags.NonPublic | BindingFlags.Instance);
            Assert.True(modelTester.Test(model, inputs, outputs, beforeRunning: () => 
            {
                Assert.True(((Matrix<double>) stackInfo.GetValue(model)).EqualTo(initial));
            }));
            Assert.True(((Matrix<double>) stackInfo.GetValue(model)).EqualTo(inputs[0]));
        }
        [Fact]
        public void Memory_InputMatrix_InitializeShapeInconsistency_CauseError()
        {
            State.StatePool.Clear();
            State.StateHandles.Clear();
            var model = ModelManager.Create("Memory");
            var initial = Matrix<double>.Build.DenseOfArray(new double[3, 2] {{1, 20.1}, {-23.2, 0}, {-15.02, 10}});
            var dict = new Dictionary<string, object> {{"initial", initial}};
            model.Configure(dict);
            var modelTester = new ModelTester();
            var inputs = new List<Matrix<double>> {Matrix<double>.Build.DenseOfArray(new double[2,3] {{-1, 2.1, 23.2}, {0.9, -1.02, -10}})};
            Assert.Throws<ModelException>(() => modelTester.TestInput(model, inputs));
        }
        [Fact]
        public void Memory_ParameterTypeWrong_CauseError()
        {
            State.StatePool.Clear();
            State.StateHandles.Clear();
            var model = ModelManager.Create("Memory");
            var dict = new Dictionary<string, object> {{"initial", "1"}};
            Assert.Throws<ModelException>(() => model.Configure(dict));
        }
        [Fact]
        public void Memory_UnknownParameter_CauseError()
        {
            State.StatePool.Clear();
            State.StateHandles.Clear();
            var model = ModelManager.Create("Memory");
            var dict = new Dictionary<string, object> {{"initial", 1}, {"unknown", 0}};
            Assert.Throws<ModelException>(() => model.Configure(dict));
        }
        [Fact]
        public void Memory_InputMatrix_ExplicitShape_OutputMatrix()
        {
            State.StatePool.Clear();
            State.StateHandles.Clear();
            var model = ModelManager.Create("Memory");
            var dict = new Dictionary<string, object> {{"row", 2}, {"col", 3}};
            model.Configure(dict);
            var modelTester = new ModelTester();
            var inputs = new List<Matrix<double>> {Matrix<double>.Build.DenseOfArray(new double[2,3] {{1, 2.1, -23.2}, {0, -0.02, 10}})};
            var outputs = new List<Matrix<double>> {Matrix<double>.Build.Dense(2, 3, 0)};
            Assert.True(modelTester.Test(model, inputs, outputs));
            var stackInfo = model.GetType().GetField("stack", BindingFlags.NonPublic | BindingFlags.Instance);
            Assert.True(((Matrix<double>) stackInfo.GetValue(model)).EqualTo(inputs[0]));
        }
        [Fact]
        public void Memory_InputMatrix_RowInconsistency_CauseError()
        {
            State.StatePool.Clear();
            State.StateHandles.Clear();
            var model = ModelManager.Create("Memory");
            var dict = new Dictionary<string, object> {{"row", 3}, {"col", 3}};
            model.Configure(dict);
            var modelTester = new ModelTester();
            var inputs = new List<Matrix<double>> {Matrix<double>.Build.DenseOfArray(new double[2,3] {{1, 2.1, -23.2}, {0, -0.02, 10}})};
            Assert.Throws<ModelException>(()=>modelTester.TestInput(model, inputs));
        }
        [Fact]
        public void Memory_InputMatrix_ColumnInconsistency_CauseError()
        {
            State.StatePool.Clear();
            State.StateHandles.Clear();
            var model = ModelManager.Create("Memory");
            var dict = new Dictionary<string, object> {{"row", 2}, {"col", 2}};
            model.Configure(dict);
            var modelTester = new ModelTester();
            var inputs = new List<Matrix<double>> {Matrix<double>.Build.DenseOfArray(new double[2,3] {{1, 2.1, -23.2}, {0, -0.02, 10}})};
            Assert.Throws<ModelException>(()=>modelTester.TestInput(model, inputs));
        }
        [Fact]
        public void Memory_DecimalShape_CauseError()
        {
            State.StatePool.Clear();
            State.StateHandles.Clear();
            var model = ModelManager.Create("Memory");
            var dict = new Dictionary<string, object> {{"row", 2.3}, {"col", 3}};
            Assert.Throws<ModelException>(()=>model.Configure(dict));
        }
        [Fact]
        public void Memory_Loop_InputScalar_ImplicitShape_OutputScalar()
        {
            State.StatePool.Clear();
            State.StateHandles.Clear();
            var model = ModelManager.Create("Memory");
            var add = ModelManager.Create("Add");
            var node = ModelManager.Create("Node");
            var dict = new Dictionary<string, object> {};
            model.Configure(dict);
            node.Connect(0, add.Expose(0));
            model.Connect(0, add.Expose(1));
            add.Connect(0, model.Expose(0));
            var group = new Group();
            group.AddInputModel(node);
            group.AddOutputModel(model);
            var models = new List<Model> {node, model, add};
            var modelTester = new ModelTester();
            var inputs = new List<Matrix<double>> {1.3.ToMatrix()};
            var outputs = new List<Matrix<double>> {0.ToMatrix()};
            Assert.True(modelTester.Test(group, models, inputs, outputs));
        }
        [Fact]
        public void Memory_Loop_InputMatrix_ImplicitShape_CauseError()
        {
            State.StatePool.Clear();
            State.StateHandles.Clear();
            var model = ModelManager.Create("Memory");
            var add = ModelManager.Create("Add");
            var node = ModelManager.Create("Node");
            var dict = new Dictionary<string, object> {};
            model.Configure(dict);
            node.Connect(0, add.Expose(0));
            model.Connect(0, add.Expose(1));
            add.Connect(0, model.Expose(0));
            var group = new Group();
            group.AddInputModel(node);
            group.AddOutputModel(model);
            var models = new List<Model> {node, model, add};
            var modelTester = new ModelTester();
            var inputs = new List<Matrix<double>> {Matrix<double>.Build.DenseOfArray(new double[2,3] {{1, 2.1, -23.2}, {0, -0.02, 10}})};
            Assert.Throws<ModelException>(()=>modelTester.TestInput(group, models, inputs));
        }
        [Fact]
        public void Memory_Loop_InputMatrix_ExplicitShape_OutputMatrix()
        {
            State.StatePool.Clear();
            State.StateHandles.Clear();
            var model = ModelManager.Create("Memory");
            var add = ModelManager.Create("Add");
            var node = ModelManager.Create("Node");
            var dict = new Dictionary<string, object> {{"row", 2}, {"col", 3}};
            model.Configure(dict);
            node.Connect(0, add.Expose(0));
            model.Connect(0, add.Expose(1));
            add.Connect(0, model.Expose(0));
            var group = new Group();
            group.AddInputModel(node);
            group.AddOutputModel(model);
            var models = new List<Model> {node, model, add};
            var modelTester = new ModelTester();
            var inputs = new List<Matrix<double>> {Matrix<double>.Build.DenseOfArray(new double[2,3] {{1, 2.1, -23.2}, {0, -0.02, 10}})};
            var outputs = new List<Matrix<double>> {Matrix<double>.Build.Dense(2, 3, 0)};
            Assert.True(modelTester.Test(group, models, inputs, outputs));
        }
    }
}