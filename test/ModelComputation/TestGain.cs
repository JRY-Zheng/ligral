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
    public class TestGain
    {
        [Fact]
        public void Gain_InputScalar_TimesScalar_OutputScalar()
        {
            var model = ModelManager.Create("Gain", null);
            var modelTester = new ModelTester();
            var dict = new Dictionary<string, object> {{"value", 2}};
            model.Configure(dict);
            var inputs = new List<Matrix<double>> {1.3.ToMatrix()};
            var outputs = new List<Matrix<double>> {2.6.ToMatrix()};
            Assert.True(modelTester.Test(model, inputs, outputs));
        }
        [Fact]
        public void Gain_InputScalar_TimesMatrix_OutputMatrix()
        {
            var model = ModelManager.Create("Gain", null);
            var modelTester = new ModelTester();
            var gain = Matrix<double>.Build.DenseOfArray(new double[2,3]{{1, 2, 3}, {-4.1, 5.1, -6.1}});
            var dict = new Dictionary<string, object> {{"value", gain}};
            model.Configure(dict);
            var inputs = new List<Matrix<double>> {1.3.ToMatrix()};
            var outputs = new List<Matrix<double>> {inputs[0].MatMul(gain)};
            Assert.True(modelTester.Test(model, inputs, outputs));
        }
        [Fact]
        public void Gain_InputMatrix_TimesScalar_OutputMatrix()
        {
            var model = ModelManager.Create("Gain", null);
            var modelTester = new ModelTester();
            var mat = Matrix<double>.Build.DenseOfArray(new double[2,3]{{1, 2, 3}, {-4.1, 5.1, -6.1}});
            var dict = new Dictionary<string, object> {{"value", 2}};
            model.Configure(dict);
            var inputs = new List<Matrix<double>> {mat};
            var outputs = new List<Matrix<double>> {mat.MatMul(2.ToMatrix())};
            Assert.True(modelTester.Test(model, inputs, outputs));
        }
        [Fact]
        public void Gain_InputMatrix_TimesMatrix_OutputMatrix()
        {
            var model = ModelManager.Create("Gain", null);
            var modelTester = new ModelTester();
            var gain = Matrix<double>.Build.DenseOfArray(new double[2,3]{{1, 2, 3}, {-4.1, 5.1, -6.1}});
            var mat = Matrix<double>.Build.DenseOfArray(new double[3,1]{{3.33}, {-4.44}, {0}});
            var dict = new Dictionary<string, object> {{"value", gain}};
            model.Configure(dict);
            var inputs = new List<Matrix<double>> {mat};
            var outputs = new List<Matrix<double>> {gain.MatMul(mat)};
            Assert.True(modelTester.Test(model, inputs, outputs));
        }
        [Fact]
        public void Gain_InputMatrix_RightTimesMatrix_OutputMatrix()
        {
            var model = ModelManager.Create("Gain", null);
            var modelTester = new ModelTester();
            var gain = Matrix<double>.Build.DenseOfArray(new double[2,3]{{1, 2, 3}, {-4.1, 5.1, -6.1}});
            var mat = Matrix<double>.Build.DenseOfArray(new double[1,2]{{3.33, -4.44}});
            var dict = new Dictionary<string, object> {{"value", gain}, {"prod", "right"}};
            model.Configure(dict);
            var inputs = new List<Matrix<double>> {mat};
            var outputs = new List<Matrix<double>> {mat.MatMul(gain)};
            Assert.True(modelTester.Test(model, inputs, outputs));
        }
        [Fact]
        public void Gain_InputMatrix_UnknownProduction_CauseError()
        {
            var model = ModelManager.Create("Gain", null);
            var modelTester = new ModelTester();
            var dict = new Dictionary<string, object> {{"value", 1}, {"prod", "top"}};
            Assert.Throws<ModelException>(() => model.Configure(dict));
        }
        [Fact]
        public void Gain_InputMatrix_ShapeInconsistency_CauseError()
        {
            var model = ModelManager.Create("Gain", null);
            var modelTester = new ModelTester();
            var gain = Matrix<double>.Build.DenseOfArray(new double[2,3]{{1, 2, 3}, {-4.1, 5.1, -6.1}});
            var mat = Matrix<double>.Build.DenseOfArray(new double[1,2]{{3.33, -4.44}});
            var dict = new Dictionary<string, object> {{"value", gain}};
            model.Configure(dict);
            var inputs = new List<Matrix<double>> {mat};
            Assert.Throws<ModelException>(() => modelTester.TestInput(model, inputs));
        }
    }
}