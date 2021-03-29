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
    public class TestEqual
    {
        [Fact]
        public void Equal_InputScalar_OutputZero()
        {
            Solution.SolutionPool.Clear();
            Solution.SolutionHandles.Clear();
            Function.FunctionPool.Clear();
            Function.FunctionHandles.Clear();
            var model = ModelManager.Create("Equal");
            var dict = new Dictionary<string, object> {{"name", "myName"}};
            model.Configure(dict);
            var modelTester = new ModelTester();
            var inputs = new List<Matrix<double>> {1.3.ToMatrix()};
            var outputs = new List<Matrix<double>> {0.ToMatrix()};
            Assert.True(modelTester.Test(model, inputs, outputs));
            Assert.True(Function.FunctionPool[0].Value==1.3);
            Assert.True(Solution.SolutionPool[0].Name == "myName");
            Assert.True(Function.FunctionPool[0].Name == "myName");
        }
        [Fact]
        public void Equal_InputZero_InitializeOne_OutputOne()
        {
            Solution.SolutionPool.Clear();
            Solution.SolutionHandles.Clear();
            Function.FunctionPool.Clear();
            Function.FunctionHandles.Clear();
            var model = ModelManager.Create("Equal");
            var dict = new Dictionary<string, object> {{"initial", 1.0}};
            model.Configure(dict);
            var modelTester = new ModelTester();
            var inputs = new List<Matrix<double>> {0.ToMatrix()};
            var outputs = new List<Matrix<double>> {0.ToMatrix()};
            // the initial value is set to the solution initial value
            // but the equal model return the guessed value
            // initial value is parsed by problem
            Assert.True(modelTester.Test(model, inputs, outputs));
            Assert.True(Solution.SolutionPool[0].InitialValue == 1);
        }
        [Fact]
        public void Equal_SetGuessedValue_OutputGuessedValue()
        {
            Solution.SolutionPool.Clear();
            Solution.SolutionHandles.Clear();
            Function.FunctionPool.Clear();
            Function.FunctionHandles.Clear();
            var model = ModelManager.Create("Equal");
            var dict = new Dictionary<string, object> {{"initial", 1.0}};
            model.Configure(dict);
            var modelTester = new ModelTester();
            var inputs = new List<Matrix<double>> {0.ToMatrix()};
            var outputs = new List<Matrix<double>> {5.3.ToMatrix()};
            Assert.True(modelTester.Test(model, inputs, outputs, ()=>Solution.SolutionPool[0].GuessedValue = 5.3));
        }
        [Fact]
        public void Equal_InputMatrix_OutputMatrix()
        {
            Solution.SolutionPool.Clear();
            Solution.SolutionHandles.Clear();
            Function.FunctionPool.Clear();
            Function.FunctionHandles.Clear();
            var model = ModelManager.Create("Equal");
            var dict = new Dictionary<string, object> ();
            model.Configure(dict);
            var modelTester = new ModelTester();
            var inputs = new List<Matrix<double>> {Matrix<double>.Build.DenseOfArray(new double[2,3] {{1, 2.1, -23.2}, {0, -0.02, 10}})};
            var outputs = new List<Matrix<double>> {Matrix<double>.Build.Dense(2, 3, 0)};
            Assert.True(modelTester.Test(model, inputs, outputs));
            Assert.True(State.StatePool.Zip(inputs[0].ToList()).All(pair => pair.First.Derivative==pair.Second));
        }
        [Fact]
        public void Equal_InputMatrix_InitializeMatrix_OutputMatrix()
        {
            Solution.SolutionPool.Clear();
            Solution.SolutionHandles.Clear();
            Function.FunctionPool.Clear();
            Function.FunctionHandles.Clear();
            var model = ModelManager.Create("Equal");
            var initial = Matrix<double>.Build.DenseOfArray(new double[2,3] {{1, 20.1, -23.2}, {0, -15.02, 10}});
            var dict = new Dictionary<string, object> {{"initial", initial}};
            model.Configure(dict);
            var modelTester = new ModelTester();
            var inputs = new List<Matrix<double>> {Matrix<double>.Build.DenseOfArray(new double[2,3] {{-1, 2.1, 23.2}, {0.9, -1.02, -10}})};
            var outputs = new List<Matrix<double>> {initial};
            Assert.True(modelTester.Test(model, inputs, outputs, ()=>Solution.SolutionPool.Zip(initial.ToList()).ToList().ForEach(pair => pair.First.GuessedValue=pair.Second)));
            // the solution is the root when the function is equal to zero
            // what we want is input[0] = initial(guessed value), so that function = input[0] - initial
            Assert.True(Function.FunctionPool.Zip((inputs[0]-initial).ToList()).All(pair => pair.First.Value==pair.Second));
        }
        [Fact]
        public void Equal_InputMatrix_InitializeShapeInconsistency_CauseError()
        {
            Solution.SolutionPool.Clear();
            Solution.SolutionHandles.Clear();
            Function.FunctionPool.Clear();
            Function.FunctionHandles.Clear();
            var model = ModelManager.Create("Equal");
            var initial = Matrix<double>.Build.DenseOfArray(new double[3, 2] {{1, 20.1}, {-23.2, 0}, {-15.02, 10}});
            var dict = new Dictionary<string, object> {{"initial", initial}};
            model.Configure(dict);
            var modelTester = new ModelTester();
            var inputs = new List<Matrix<double>> {Matrix<double>.Build.DenseOfArray(new double[2,3] {{-1, 2.1, 23.2}, {0.9, -1.02, -10}})};
            Assert.Throws<ModelException>(() => modelTester.TestInput(model, inputs));
        }
        [Fact]
        public void Equal_ParameterTypeWrong_CauseError()
        {
            Solution.SolutionPool.Clear();
            Solution.SolutionHandles.Clear();
            Function.FunctionPool.Clear();
            Function.FunctionHandles.Clear();
            var model = ModelManager.Create("Equal");
            var dict = new Dictionary<string, object> {{"initial", "1"}};
            Assert.Throws<ModelException>(() => model.Configure(dict));
        }
        [Fact]
        public void Equal_UnknownParameter_CauseError()
        {
            Solution.SolutionPool.Clear();
            Solution.SolutionHandles.Clear();
            Function.FunctionPool.Clear();
            Function.FunctionHandles.Clear();
            var model = ModelManager.Create("Equal");
            var dict = new Dictionary<string, object> {{"initial", 1}, {"unknown", 0}};
            Assert.Throws<ModelException>(() => model.Configure(dict));
        }
        [Fact]
        public void Equal_InputMatrix_ExplicitShape_OutputMatrix()
        {
            Solution.SolutionPool.Clear();
            Solution.SolutionHandles.Clear();
            Function.FunctionPool.Clear();
            Function.FunctionHandles.Clear();
            var model = ModelManager.Create("Equal");
            var dict = new Dictionary<string, object> {{"row", 2}, {"col", 3}};
            model.Configure(dict);
            var modelTester = new ModelTester();
            var inputs = new List<Matrix<double>> {Matrix<double>.Build.DenseOfArray(new double[2,3] {{1, 2.1, -23.2}, {0, -0.02, 10}})};
            var outputs = new List<Matrix<double>> {Matrix<double>.Build.Dense(2, 3, 0)};
            Assert.True(modelTester.Test(model, inputs, outputs));
            Assert.True(Function.FunctionPool.Zip(inputs[0].ToList()).All(pair => pair.First.Value==pair.Second));
        }
        [Fact]
        public void Equal_InputMatrix_RowInconsistency_CauseError()
        {
            Solution.SolutionPool.Clear();
            Solution.SolutionHandles.Clear();
            Function.FunctionPool.Clear();
            Function.FunctionHandles.Clear();
            var model = ModelManager.Create("Equal");
            var dict = new Dictionary<string, object> {{"row", 3}, {"col", 3}};
            model.Configure(dict);
            var modelTester = new ModelTester();
            var inputs = new List<Matrix<double>> {Matrix<double>.Build.DenseOfArray(new double[2,3] {{1, 2.1, -23.2}, {0, -0.02, 10}})};
            Assert.Throws<ModelException>(()=>modelTester.TestInput(model, inputs));
        }
        [Fact]
        public void Equal_InputMatrix_ColumnInconsistency_CauseError()
        {
            Solution.SolutionPool.Clear();
            Solution.SolutionHandles.Clear();
            Function.FunctionPool.Clear();
            Function.FunctionHandles.Clear();
            var model = ModelManager.Create("Equal");
            var dict = new Dictionary<string, object> {{"row", 2}, {"col", 2}};
            model.Configure(dict);
            var modelTester = new ModelTester();
            var inputs = new List<Matrix<double>> {Matrix<double>.Build.DenseOfArray(new double[2,3] {{1, 2.1, -23.2}, {0, -0.02, 10}})};
            Assert.Throws<ModelException>(()=>modelTester.TestInput(model, inputs));
        }
        [Fact]
        public void Equal_DecimalShape_CauseError()
        {
            Solution.SolutionPool.Clear();
            Solution.SolutionHandles.Clear();
            Function.FunctionPool.Clear();
            Function.FunctionHandles.Clear();
            var model = ModelManager.Create("Equal");
            var dict = new Dictionary<string, object> {{"row", 2.3}, {"col", 3}};
            Assert.Throws<ModelException>(()=>model.Configure(dict));
        }
        [Fact]
        public void Equal_NegativeShape_CauseError()
        {
            Solution.SolutionPool.Clear();
            Solution.SolutionHandles.Clear();
            Function.FunctionPool.Clear();
            Function.FunctionHandles.Clear();
            var model = ModelManager.Create("Equal");
            var dict = new Dictionary<string, object> {{"row", -10.0}, {"col", 10.0}};
            Assert.Throws<ModelException>(()=>model.Configure(dict));
        }
        [Fact]
        public void Equal_ZeroShape_CauseError()
        {
            Solution.SolutionPool.Clear();
            Solution.SolutionHandles.Clear();
            Function.FunctionPool.Clear();
            Function.FunctionHandles.Clear();
            var model = ModelManager.Create("Equal");
            var dict = new Dictionary<string, object> {{"row", 0.0}, {"col", 10.0}};
            Assert.Throws<ModelException>(()=>model.Configure(dict));
        }
        [Fact]
        public void Equal_Loop_InputScalar_ImplicitShape_OutputScalar()
        {
            Solution.SolutionPool.Clear();
            Solution.SolutionHandles.Clear();
            Function.FunctionPool.Clear();
            Function.FunctionHandles.Clear();
            var model = ModelManager.Create("Equal");
            var add = ModelManager.Create("Add");
            var node = ModelManager.Create("Node");
            var dict = new Dictionary<string, object> ();
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
        public void Equal_Loop_InputMatrix_ImplicitShape_CauseError()
        {
            Solution.SolutionPool.Clear();
            Solution.SolutionHandles.Clear();
            Function.FunctionPool.Clear();
            Function.FunctionHandles.Clear();
            var model = ModelManager.Create("Equal");
            var add = ModelManager.Create("Add");
            var node = ModelManager.Create("Node");
            var dict = new Dictionary<string, object> ();
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
        public void Equal_Loop_InputMatrix_ExplicitShape_CauseError()
        {
            Solution.SolutionPool.Clear();
            Solution.SolutionHandles.Clear();
            Function.FunctionPool.Clear();
            Function.FunctionHandles.Clear();
            var model = ModelManager.Create("Equal");
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