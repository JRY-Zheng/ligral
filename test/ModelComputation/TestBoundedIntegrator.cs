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
    public class TestBoundedIntegrator
    {
        [Fact]
        public void BoundedIntegrator_InputScalar_OutputZero()
        {
            State.StatePool.Clear();
            State.StateHandles.Clear();
            var model = ModelManager.Create("BoundedIntegrator");
            var dict = new Dictionary<string, object> {{"upper", 10.0}, {"lower", -10.0}, {"name", "myName"}};
            model.Configure(dict);
            var modelTester = new ModelTester();
            var inputs = new List<Matrix<double>> {1.3.ToMatrix()};
            var outputs = new List<Matrix<double>> {0.ToMatrix()};
            Assert.True(modelTester.Test(model, inputs, outputs));
            Assert.True(State.StatePool[0].Derivative==1.3);
            Assert.True(State.StatePool[0].Name == "myName");
        }
        [Fact]
        public void BoundedIntegrator_InputZero_InitializeOne_OutputOne()
        {
            State.StatePool.Clear();
            State.StateHandles.Clear();
            var model = ModelManager.Create("BoundedIntegrator");
            var dict = new Dictionary<string, object> {{"initial", 1.0}, {"upper", 10.0}, {"lower", -10.0}};
            model.Configure(dict);
            var modelTester = new ModelTester();
            var inputs = new List<Matrix<double>> {0.ToMatrix()};
            var outputs = new List<Matrix<double>> {1.ToMatrix()};
            Assert.True(modelTester.Test(model, inputs, outputs));
        }
        [Fact]
        public void BoundedIntegrator_SetState_OutputStateValue()
        {
            State.StatePool.Clear();
            State.StateHandles.Clear();
            var model = ModelManager.Create("BoundedIntegrator");
            var dict = new Dictionary<string, object> {{"initial", 1.0}, {"upper", 10.0}, {"lower", -10.0}};
            model.Configure(dict);
            var modelTester = new ModelTester();
            var inputs = new List<Matrix<double>> {0.ToMatrix()};
            var outputs = new List<Matrix<double>> {5.3.ToMatrix()};
            Assert.True(modelTester.Test(model, inputs, outputs, beforeRunning: ()=>State.StatePool[0].StateVariable = 5.3));
        }
        [Fact]
        public void BoundedIntegrator_StateOutOfUpperBound_PositiveDerivativeDemand_DerivativeZero()
        {
            State.StatePool.Clear();
            State.StateHandles.Clear();
            var model = ModelManager.Create("BoundedIntegrator");
            var dict = new Dictionary<string, object> {{"initial", 1.0}, {"upper", 10.0}, {"lower", -10.0}};
            model.Configure(dict);
            var modelTester = new ModelTester();
            var inputs = new List<Matrix<double>> {1.2.ToMatrix()};
            var outputs = new List<Matrix<double>> {10.ToMatrix()};
            Assert.True(modelTester.Test(model, inputs, outputs, beforeRunning: ()=>State.StatePool[0].StateVariable = 10.0));
            Assert.True(State.StatePool[0].Derivative==0);
        }
        [Fact]
        public void BoundedIntegrator_StateOutOfUpperBound_NegativeDerivativeDemand_DerivativeDemanded()
        {
            State.StatePool.Clear();
            State.StateHandles.Clear();
            var model = ModelManager.Create("BoundedIntegrator");
            var dict = new Dictionary<string, object> {{"initial", 1.0}, {"upper", 10.0}, {"lower", -10.0}};
            model.Configure(dict);
            var modelTester = new ModelTester();
            var inputs = new List<Matrix<double>> {-11.2.ToMatrix()};
            var outputs = new List<Matrix<double>> {10.ToMatrix()};
            Assert.True(modelTester.Test(model, inputs, outputs, beforeRunning: ()=>State.StatePool[0].StateVariable = 10.0));
            Assert.True(State.StatePool[0].Derivative==-11.2);
        }
        [Fact]
        public void BoundedIntegrator_StateOutOfLowerBound_PositiveDerivativeDemand_DerivativeDemanded()
        {
            State.StatePool.Clear();
            State.StateHandles.Clear();
            var model = ModelManager.Create("BoundedIntegrator");
            var dict = new Dictionary<string, object> {{"initial", 1.0}, {"upper", 10.0}, {"lower", -10.0}};
            model.Configure(dict);
            var modelTester = new ModelTester();
            var inputs = new List<Matrix<double>> {1.2.ToMatrix()};
            var outputs = new List<Matrix<double>> {-10.ToMatrix()};
            Assert.True(modelTester.Test(model, inputs, outputs, beforeRunning: ()=>State.StatePool[0].StateVariable = -10.0));
            Assert.True(State.StatePool[0].Derivative==1.2);
        }
        [Fact]
        public void BoundedIntegrator_StateOutOfLowerBound_NegativeDerivativeDemand_DerivativeDemanded()
        {
            State.StatePool.Clear();
            State.StateHandles.Clear();
            var model = ModelManager.Create("BoundedIntegrator");
            var dict = new Dictionary<string, object> {{"initial", 1.0}, {"upper", 10.0}, {"lower", -10.0}};
            model.Configure(dict);
            var modelTester = new ModelTester();
            var inputs = new List<Matrix<double>> {-11.2.ToMatrix()};
            var outputs = new List<Matrix<double>> {-10.ToMatrix()};
            Assert.True(modelTester.Test(model, inputs, outputs, beforeRunning: ()=>State.StatePool[0].StateVariable = -10.0));
            Assert.True(State.StatePool[0].Derivative==0);
        }
        [Fact]
        public void BoundedIntegrator_InputMatrix_OutputMatrix()
        {
            State.StatePool.Clear();
            State.StateHandles.Clear();
            var model = ModelManager.Create("BoundedIntegrator");
            var dict = new Dictionary<string, object> {{"upper", 10.0}, {"lower", -10.0}};
            model.Configure(dict);
            var modelTester = new ModelTester();
            var inputs = new List<Matrix<double>> {Matrix<double>.Build.DenseOfArray(new double[2,3] {{1, 2.1, -23.2}, {0, -0.02, 10}})};
            var outputs = new List<Matrix<double>> {Matrix<double>.Build.Dense(2, 3, 0)};
            Assert.True(modelTester.Test(model, inputs, outputs));
            Assert.True(State.StatePool.Zip(inputs[0].ToList()).All(pair => pair.First.Derivative==pair.Second));
        }
        [Fact]
        public void BoundedIntegrator_InputMatrix_InitializeMatrix_OutputMatrix()
        {
            State.StatePool.Clear();
            State.StateHandles.Clear();
            var model = ModelManager.Create("BoundedIntegrator");
            var initial = Matrix<double>.Build.DenseOfArray(new double[2,3] {{1, 20.1, -23.2}, {0, -15.02, 10}});
            var dict = new Dictionary<string, object> {{"initial", initial}, {"upper", 10.0.ToMatrix()}, {"lower", -10.0.ToMatrix()}};
            model.Configure(dict);
            var modelTester = new ModelTester();
            var inputs = new List<Matrix<double>> {Matrix<double>.Build.DenseOfArray(new double[2,3] {{-1, 2.1, 23.2}, {0.9, -1.02, -10}})};
            var outputs = new List<Matrix<double>> {initial};
            var derivative = Matrix<double>.Build.DenseOfArray(new double[2,3] {{-1, 0, 23.2}, {0.9, 0, -10}});
            Assert.True(modelTester.Test(model, inputs, outputs));
            Assert.True(State.StatePool.Zip(derivative.ToList()).All(pair => pair.First.Derivative==pair.Second));
        }
        [Fact]
        public void BoundedIntegrator_InputMatrix_InitializeShapeInconsistency_CauseError()
        {
            State.StatePool.Clear();
            State.StateHandles.Clear();
            var model = ModelManager.Create("BoundedIntegrator");
            var initial = Matrix<double>.Build.DenseOfArray(new double[3, 2] {{1, 20.1}, {-23.2, 0}, {-15.02, 10}});
            var dict = new Dictionary<string, object> {{"initial", initial}, {"upper", 10.0.ToMatrix()}, {"lower", -10.0.ToMatrix()}};
            model.Configure(dict);
            var modelTester = new ModelTester();
            var inputs = new List<Matrix<double>> {Matrix<double>.Build.DenseOfArray(new double[2,3] {{-1, 2.1, 23.2}, {0.9, -1.02, -10}})};
            Assert.Throws<ModelException>(() => modelTester.TestInput(model, inputs));
        }
        [Fact]
        public void BoundedIntegrator_MissUpper_CauseError()
        {
            State.StatePool.Clear();
            State.StateHandles.Clear();
            var model = ModelManager.Create("BoundedIntegrator");
            var dict = new Dictionary<string, object> {{"initial", 1}, {"lower", -10}};
            Assert.Throws<ModelException>(() => model.Configure(dict));
        }
        [Fact]
        public void BoundedIntegrator_MissLower_CauseError()
        {
            State.StatePool.Clear();
            State.StateHandles.Clear();
            var model = ModelManager.Create("BoundedIntegrator");
            var dict = new Dictionary<string, object> {{"initial", 1}, {"upper", 10}};
            Assert.Throws<ModelException>(() => model.Configure(dict));
        }
        [Fact]
        public void BoundedIntegrator_ParameterTypeWrong_CauseError()
        {
            State.StatePool.Clear();
            State.StateHandles.Clear();
            var model = ModelManager.Create("BoundedIntegrator");
            var dict = new Dictionary<string, object> {{"initial", 1}, {"upper", "10"}, {"lower", -10}};
            Assert.Throws<ModelException>(() => model.Configure(dict));
        }
        [Fact]
        public void BoundedIntegrator_UnknownParameter_CauseError()
        {
            State.StatePool.Clear();
            State.StateHandles.Clear();
            var model = ModelManager.Create("BoundedIntegrator");
            var dict = new Dictionary<string, object> {{"initial", 1}, {"upper", 10}, {"lower", -10}, {"unknown", 0}};
            Assert.Throws<ModelException>(() => model.Configure(dict));
        }
        [Fact]
        public void BoundedIntegrator_InputMatrix_ExplicitShape_OutputMatrix()
        {
            State.StatePool.Clear();
            State.StateHandles.Clear();
            var model = ModelManager.Create("BoundedIntegrator");
            var dict = new Dictionary<string, object> {{"upper", 10.0}, {"lower", -10.0}, {"row", 2}, {"col", 3}};
            model.Configure(dict);
            var modelTester = new ModelTester();
            var inputs = new List<Matrix<double>> {Matrix<double>.Build.DenseOfArray(new double[2,3] {{1, 2.1, -23.2}, {0, -0.02, 10}})};
            var outputs = new List<Matrix<double>> {Matrix<double>.Build.Dense(2, 3, 0)};
            Assert.True(modelTester.Test(model, inputs, outputs));
            Assert.True(State.StatePool.Zip(inputs[0].ToList()).All(pair => pair.First.Derivative==pair.Second));
        }
        [Fact]
        public void BoundedIntegrator_InputMatrix_RowInconsistency_CauseError()
        {
            State.StatePool.Clear();
            State.StateHandles.Clear();
            var model = ModelManager.Create("BoundedIntegrator");
            var dict = new Dictionary<string, object> {{"upper", 10.0}, {"lower", -10.0}, {"row", 3}, {"col", 3}};
            model.Configure(dict);
            var modelTester = new ModelTester();
            var inputs = new List<Matrix<double>> {Matrix<double>.Build.DenseOfArray(new double[2,3] {{1, 2.1, -23.2}, {0, -0.02, 10}})};
            Assert.Throws<ModelException>(()=>modelTester.TestInput(model, inputs));
        }
        [Fact]
        public void BoundedIntegrator_InputMatrix_ColumnInconsistency_CauseError()
        {
            State.StatePool.Clear();
            State.StateHandles.Clear();
            var model = ModelManager.Create("BoundedIntegrator");
            var dict = new Dictionary<string, object> {{"upper", 10.0}, {"lower", -10.0}, {"row", 2}, {"col", 2}};
            model.Configure(dict);
            var modelTester = new ModelTester();
            var inputs = new List<Matrix<double>> {Matrix<double>.Build.DenseOfArray(new double[2,3] {{1, 2.1, -23.2}, {0, -0.02, 10}})};
            Assert.Throws<ModelException>(()=>modelTester.TestInput(model, inputs));
        }
        [Fact]
        public void BoundedIntegrator_DecimalShape_CauseError()
        {
            State.StatePool.Clear();
            State.StateHandles.Clear();
            var model = ModelManager.Create("BoundedIntegrator");
            var dict = new Dictionary<string, object> {{"upper", 10.0}, {"lower", -10.0}, {"row", 2.3}, {"col", 3}};
            Assert.Throws<ModelException>(()=>model.Configure(dict));
        }
        [Fact]
        public void BoundedIntegrator_ConflictBound_CauseError()
        {
            State.StatePool.Clear();
            State.StateHandles.Clear();
            var model = ModelManager.Create("BoundedIntegrator");
            var dict = new Dictionary<string, object> {{"upper", -10.0}, {"lower", 10.0}};
            Assert.Throws<ModelException>(()=>model.Configure(dict));
        }
        [Fact]
        public void BoundedIntegrator_Loop_InputScalar_ImplicitShape_OutputScalar()
        {
            State.StatePool.Clear();
            State.StateHandles.Clear();
            var model = ModelManager.Create("BoundedIntegrator");
            var add = ModelManager.Create("Add");
            var node = ModelManager.Create("Node");
            var dict = new Dictionary<string, object> {{"upper", 10.0}, {"lower", -10.0}};
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
        public void BoundedIntegrator_Loop_InputMatrix_ImplicitShape_CauseError()
        {
            State.StatePool.Clear();
            State.StateHandles.Clear();
            var model = ModelManager.Create("BoundedIntegrator");
            var add = ModelManager.Create("Add");
            var node = ModelManager.Create("Node");
            var dict = new Dictionary<string, object> {{"upper", 10.0}, {"lower", -10.0}};
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
        public void BoundedIntegrator_Loop_InputMatrix_ExplicitShape_OutputMatrix()
        {
            State.StatePool.Clear();
            State.StateHandles.Clear();
            var model = ModelManager.Create("BoundedIntegrator");
            var add = ModelManager.Create("Add");
            var node = ModelManager.Create("Node");
            var dict = new Dictionary<string, object> {{"upper", 10.0}, {"lower", -10.0}, {"row", 2}, {"col", 3}};
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