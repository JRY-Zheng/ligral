/* Copyright (C) 2019-2021 Junruoyu Zheng. Home page: https://junruoyu-zheng.gitee.io/ligral

   Distributed under MIT license.
   See file LICENSE for detail or copy at https://opensource.org/licenses/MIT
*/

using System.Reflection;
using System.Collections.Generic;
using Xunit;
using MathNet.Numerics.LinearAlgebra;
using Ligral.Component;
using Ligral.Simulation;
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
                dataInfo.SetValue(storage, new List<List<double>>{new List<double>{0, 1}, new List<double>{1, 2}});
                model.GetType().GetField("table", BindingFlags.NonPublic|BindingFlags.Instance).SetValue(model, storage);
            };
            var dict = new Dictionary<string, object> {{"file", "fakefile"}};
            model.Configure(dict);
            var modelTester = new ModelTester();
            var inputs = new List<Matrix<double>> {0.5.ToMatrix()};
            var outputs = new List<Matrix<double>> {1.5.ToMatrix()};
            Assert.True(modelTester.Test(model, inputs, outputs));
        }
    }
}