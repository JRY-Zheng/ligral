/* Copyright (C) 2019-2022 Junruoyu Zheng. Home page: https://junruoyu-zheng.gitee.io/ligral

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
    public class TestDeadzone
    {
        [Fact]
        public void Deadzone_InputScalarBetweenDeadzone_OutputZero()
        {
            State.StatePool.Clear();
            var model = ModelManager.Create("Deadzone", null);
            var dict = new Dictionary<string, object> {{"right", 12.0}, {"left", -10.0}};
            model.Configure(dict);
            var modelTester = new ModelTester();
            var inputs = new List<Matrix<double>> {1.3.ToMatrix()};
            var outputs = new List<Matrix<double>> {0.ToMatrix()};
            Assert.True(modelTester.Test(model, inputs, outputs));
        }
        [Fact]
        public void Deadzone_InputScalarOutOfRightBound_OutputZero()
        {
            State.StatePool.Clear();
            var model = ModelManager.Create("Deadzone", null);
            var dict = new Dictionary<string, object> {{"right", 12.0}, {"left", -10.0}};
            model.Configure(dict);
            var modelTester = new ModelTester();
            var inputs = new List<Matrix<double>> {21.3.ToMatrix()};
            var outputs = new List<Matrix<double>> {9.3.ToMatrix()};
            Assert.True(modelTester.Test(model, inputs, outputs));
        }
        [Fact]
        public void Deadzone_InputScalarOutOfLeftBound_OutputZero()
        {
            State.StatePool.Clear();
            var model = ModelManager.Create("Deadzone", null);
            var dict = new Dictionary<string, object> {{"right", 12.0}, {"left", -10.0}};
            model.Configure(dict);
            var modelTester = new ModelTester();
            var inputs = new List<Matrix<double>> {-21.3.ToMatrix()};
            var outputs = new List<Matrix<double>> {-11.3.ToMatrix()};
            Assert.True(modelTester.Test(model, inputs, outputs));
        }
        [Fact]
        public void Deadzone_InputMatrix_OutputMatrix()
        {
            State.StatePool.Clear();
            var model = ModelManager.Create("Deadzone", null);
            var dict = new Dictionary<string, object> {{"right", 12.0}, {"left", -10.0}};
            model.Configure(dict);
            var modelTester = new ModelTester();
            var inputs = new List<Matrix<double>> {Matrix<double>.Build.DenseOfArray(new double[2,3] {{1, 2.1, -23.2}, {0, 12, 14.4}})};
            var outputs = new List<Matrix<double>> {Matrix<double>.Build.DenseOfArray(new double[2,3] {{0, 0, -13.2}, {0, 0, 2.4}})};
            Assert.True(modelTester.Test(model, inputs, outputs));
        }
        [Fact]
        public void Deadzone_MissRight_CauseError()
        {
            State.StatePool.Clear();
            var model = ModelManager.Create("Deadzone", null);
            var dict = new Dictionary<string, object> {{"initial", 1}, {"left", -10}};
            Assert.Throws<ModelException>(() => model.Configure(dict));
        }
        [Fact]
        public void Deadzone_MissLeft_CauseError()
        {
            State.StatePool.Clear();
            var model = ModelManager.Create("Deadzone", null);
            var dict = new Dictionary<string, object> {{"right", 12}};
            Assert.Throws<ModelException>(() => model.Configure(dict));
        }
        [Fact]
        public void Deadzone_ParameterTypeWrong_CauseError()
        {
            State.StatePool.Clear();
            var model = ModelManager.Create("Deadzone", null);
            var dict = new Dictionary<string, object> {{"right", "10"}, {"left", -10}};
            Assert.Throws<ModelException>(() => model.Configure(dict));
        }
        [Fact]
        public void Deadzone_UnknownParameter_CauseError()
        {
            State.StatePool.Clear();
            var model = ModelManager.Create("Deadzone", null);
            var dict = new Dictionary<string, object> {{"right", 12}, {"left", -10}, {"unknown", 0}};
            Assert.Throws<ModelException>(() => model.Configure(dict));
        }
        [Fact]
        public void Deadzone_ConflictBound_CauseError()
        {
            State.StatePool.Clear();
            var model = ModelManager.Create("Deadzone", null);
            var dict = new Dictionary<string, object> {{"right", -10.0}, {"left", 10.0}};
            Assert.Throws<ModelException>(()=>model.Configure(dict));
        }
    }
}