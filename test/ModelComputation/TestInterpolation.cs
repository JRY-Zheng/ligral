/* Copyright (C) 2019-2021 Junruoyu Zheng. Home page: https://junruoyu-zheng.gitee.io/ligral

   Distributed under MIT license.
   See file LICENSE for detail or copy at https://opensource.org/licenses/MIT
*/

using System.Reflection;
using System.Collections.Generic;
using Xunit;
using MathNet.Numerics.LinearAlgebra;
using Ligral.Component;
using Ligral.Tools;

namespace Ligral.Tests.ModelTester
{
    public class TestInterpolation
    {
        [Fact]
        public void Interpolation_InputScalar_OutputScalar()
        {
            var model = ModelManager.Create("Interpolation");
            var parametersInfo = model.GetType().GetField("Parameters", BindingFlags.NonPublic|BindingFlags.Instance);
            var parameters = (Dictionary<string, Parameter>) parametersInfo.GetValue(model);
            parameters["file"].OnSet = val => 
            {
                var storage = new Storage();
                var columnInfo = storage.GetType().GetProperty("Columns");
                columnInfo.SetValue(storage, new List<string>{"x", "y"});
                var dataInfo = storage.GetType().GetProperty("Data");
                var row0 = new List<double>{0, 1};
                var row1 = new List<double>{1, 2};
                var row2 = new List<double>{2, 3};
                dataInfo.SetValue(storage, new List<List<double>>{row0, row1, row2});
                model.GetType().GetField("table", BindingFlags.NonPublic|BindingFlags.Instance).SetValue(model, storage);
            };
            var dict = new Dictionary<string, object> {{"file", "fakefile"}};
            model.Configure(dict);
            var modelTester = new ModelTester();
            var inputs = new List<Matrix<double>> {0.5.ToMatrix()};
            var outputs = new List<Matrix<double>> {1.5.ToMatrix()};
            Assert.True(modelTester.Test(model, inputs, outputs));
        }
        [Fact]
        public void Interpolation_InputScalar_OutOfRightLimit_OutputScalar()
        {
            var model = ModelManager.Create("Interpolation");
            var parametersInfo = model.GetType().GetField("Parameters", BindingFlags.NonPublic|BindingFlags.Instance);
            var parameters = (Dictionary<string, Parameter>) parametersInfo.GetValue(model);
            parameters["file"].OnSet = val => 
            {
                var storage = new Storage();
                var columnInfo = storage.GetType().GetProperty("Columns");
                columnInfo.SetValue(storage, new List<string>{"x", "y"});
                var dataInfo = storage.GetType().GetProperty("Data");
                var row0 = new List<double>{0, 1};
                var row1 = new List<double>{1, 2};
                var row2 = new List<double>{2, 3};
                dataInfo.SetValue(storage, new List<List<double>>{row0, row1, row2});
                model.GetType().GetField("table", BindingFlags.NonPublic|BindingFlags.Instance).SetValue(model, storage);
            };
            var dict = new Dictionary<string, object> {{"file", "fakefile"}};
            model.Configure(dict);
            var modelTester = new ModelTester();
            var inputs = new List<Matrix<double>> {10.ToMatrix()};
            var outputs = new List<Matrix<double>> {3.ToMatrix()};
            Assert.True(modelTester.Test(model, inputs, outputs));
        }
        [Fact]
        public void Interpolation_InputScalar_OutOfLeftLimit_OutputScalar()
        {
            var model = ModelManager.Create("Interpolation");
            var parametersInfo = model.GetType().GetField("Parameters", BindingFlags.NonPublic|BindingFlags.Instance);
            var parameters = (Dictionary<string, Parameter>) parametersInfo.GetValue(model);
            parameters["file"].OnSet = val => 
            {
                var storage = new Storage();
                var columnInfo = storage.GetType().GetProperty("Columns");
                columnInfo.SetValue(storage, new List<string>{"x", "y"});
                var dataInfo = storage.GetType().GetProperty("Data");
                var row0 = new List<double>{-1, -1};
                var row1 = new List<double>{1, 2};
                var row2 = new List<double>{2, 3};
                dataInfo.SetValue(storage, new List<List<double>>{row0, row1, row2});
                model.GetType().GetField("table", BindingFlags.NonPublic|BindingFlags.Instance).SetValue(model, storage);
            };
            var dict = new Dictionary<string, object> {{"file", "fakefile"}};
            model.Configure(dict);
            var modelTester = new ModelTester();
            var inputs = new List<Matrix<double>> {-10.ToMatrix()};
            var outputs = new List<Matrix<double>> {-1.ToMatrix()};
            Assert.True(modelTester.Test(model, inputs, outputs));
        }
        [Fact]
        public void Interpolation_InputScalar_OutputMatrix()
        {
            var model = ModelManager.Create("Interpolation");
            var parametersInfo = model.GetType().GetField("Parameters", BindingFlags.NonPublic|BindingFlags.Instance);
            var parameters = (Dictionary<string, Parameter>) parametersInfo.GetValue(model);
            parameters["file"].OnSet = val => 
            {
                var storage = new Storage();
                var columnInfo = storage.GetType().GetProperty("Columns");
                columnInfo.SetValue(storage, new List<string>{"x", "y"});
                var dataInfo = storage.GetType().GetProperty("Data");
                var row0 = new List<double>{0, 1, 2, 3, -3, -2, -1};
                var row1 = new List<double>{1, 2, 3, 4, -4, -3, -2};
                var row2 = new List<double>{2, 3, 4, 5, -5, -4, -3};
                dataInfo.SetValue(storage, new List<List<double>>{row0, row1, row2});
                model.GetType().GetField("table", BindingFlags.NonPublic|BindingFlags.Instance).SetValue(model, storage);
            };
            var dict = new Dictionary<string, object> {{"file", "fakefile"}, {"row", 2}, {"col", 3}};
            model.Configure(dict);
            var modelTester = new ModelTester();
            var inputs = new List<Matrix<double>> {0.5.ToMatrix()};
            var outputs = new List<Matrix<double>> {Matrix<double>.Build.DenseOfArray(new double[2,3]{{1.5, 2.5, 3.5}, {-3.5, -2.5, -1.5}})};
            Assert.True(modelTester.Test(model, inputs, outputs));
        }
        [Fact]
        public void Interpolation_InputScalar_OutputVector()
        {
            var model = ModelManager.Create("Interpolation");
            var parametersInfo = model.GetType().GetField("Parameters", BindingFlags.NonPublic|BindingFlags.Instance);
            var parameters = (Dictionary<string, Parameter>) parametersInfo.GetValue(model);
            parameters["file"].OnSet = val => 
            {
                var storage = new Storage();
                var columnInfo = storage.GetType().GetProperty("Columns");
                columnInfo.SetValue(storage, new List<string>{"x", "y"});
                var dataInfo = storage.GetType().GetProperty("Data");
                var row0 = new List<double>{0, 1, 2, 3, -3, -2, -1};
                var row1 = new List<double>{1, 2, 3, 4, -4, -3, -2};
                var row2 = new List<double>{2, 3, 4, 5, -5, -4, -3};
                dataInfo.SetValue(storage, new List<List<double>>{row0, row1, row2});
                model.GetType().GetField("table", BindingFlags.NonPublic|BindingFlags.Instance).SetValue(model, storage);
            };
            var dict = new Dictionary<string, object> {{"file", "fakefile"}, {"row", 6}, {"col", 1}};
            model.Configure(dict);
            var modelTester = new ModelTester();
            var inputs = new List<Matrix<double>> {0.5.ToMatrix()};
            var outputs = new List<Matrix<double>> {Matrix<double>.Build.DenseOfArray(new double[6,1]{{1.5}, {2.5}, {3.5}, {-3.5}, {-2.5}, {-1.5}})};
            Assert.True(modelTester.Test(model, inputs, outputs));
        }
        [Fact]
        public void Interpolation_InputScalar_ShapeInconsistency_OutputMatrix()
        {
            var model = ModelManager.Create("Interpolation");
            var parametersInfo = model.GetType().GetField("Parameters", BindingFlags.NonPublic|BindingFlags.Instance);
            var parameters = (Dictionary<string, Parameter>) parametersInfo.GetValue(model);
            parameters["file"].OnSet = val => 
            {
                var storage = new Storage();
                var columnInfo = storage.GetType().GetProperty("Columns");
                columnInfo.SetValue(storage, new List<string>{"x", "y"});
                var dataInfo = storage.GetType().GetProperty("Data");
                var row0 = new List<double>{0, 1, 2, 3, -3, -2, -1};
                var row1 = new List<double>{1, 2, 3, 4, -4, -3, -2};
                var row2 = new List<double>{2, 3, 4, 5, -5, -4, -3};
                dataInfo.SetValue(storage, new List<List<double>>{row0, row1, row2});
                model.GetType().GetField("table", BindingFlags.NonPublic|BindingFlags.Instance).SetValue(model, storage);
            };
            var dict = new Dictionary<string, object> {{"file", "fakefile"}, {"row", 1}, {"col", 3}};
            model.Configure(dict);
            var modelTester = new ModelTester();
            var inputs = new List<Matrix<double>> {0.5.ToMatrix()};
            Assert.Throws<ModelException>(() => modelTester.TestInput(model, inputs));
        }
        [Fact]
        public void Interpolation_InputScalar_NegativeShape_OutputMatrix()
        {
            var model = ModelManager.Create("Interpolation");
            var parametersInfo = model.GetType().GetField("Parameters", BindingFlags.NonPublic|BindingFlags.Instance);
            var parameters = (Dictionary<string, Parameter>) parametersInfo.GetValue(model);
            parameters["file"].OnSet = val => {};
            var dict = new Dictionary<string, object> {{"file", "fakefile"}, {"row", -1}, {"col", 3}};
            Assert.Throws<ModelException>(() => model.Configure(dict));
        }
        [Fact]
        public void Interpolation_InputScalar_DecimalShape_OutputMatrix()
        {
            var model = ModelManager.Create("Interpolation");
            var parametersInfo = model.GetType().GetField("Parameters", BindingFlags.NonPublic|BindingFlags.Instance);
            var parameters = (Dictionary<string, Parameter>) parametersInfo.GetValue(model);
            parameters["file"].OnSet = val => {};
            var dict = new Dictionary<string, object> {{"file", "fakefile"}, {"row", 2.3}, {"col", 3}};
            Assert.Throws<ModelException>(() => model.Configure(dict));
        }
    }
}