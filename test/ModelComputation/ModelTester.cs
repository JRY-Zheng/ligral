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
    class InputTester : Model
    {
        protected override void SetUpPorts()
        {
            
        }
        public override void Check()
        {
            for (int i=0; i<Results.Count; i++)
            {
                OutPortList[i].SetShape(Results[i].RowCount, Results[i].ColumnCount);
            }
        }
        public void SetInput(List<Matrix<double>> inputs)
        {
            foreach (var input in inputs)
            {
                var outPort = new OutPort($"out{OutPortList.Count}", this);
                OutPortList.Add(outPort);
                Results = inputs;
            }
        }
        protected override List<Matrix<double>> Calculate(List<Matrix<double>> values)
        {
            return Results;
        }
    }
    class OutputTester : Model
    {
        protected override void SetUpPorts()
        {
            
        }
        public override void Check()
        {
            
        }
        public void SetOutput(int number)
        {
            for (int i=0; i<number; i++)
            {
                var inPort = new InPort($"in{i}", this);
                InPortList.Add(inPort);
            }
        }
        public List<Matrix<double>> GetOutput()
        {
            return InPortList.ConvertAll(input => input.GetValue());
        }
        protected override List<Matrix<double>> Calculate(List<Matrix<double>> values)
        {
            return Results;
        }
    }
    class ModelTester
    {
        public bool Test(Model model, List<Matrix<double>> inputs, List<Matrix<double>> outputs)
        {
            InputTester inputTester = new InputTester();
            OutputTester outputTester = new OutputTester();
            inputTester.SetInput(inputs);
            outputTester.SetOutput(outputs.Count);
            ((ILinkable) inputTester).Connect(model);
            ((ILinkable) model).Connect(outputTester);
            List<Model> models = new List<Model> {inputTester, model, outputTester};
            models.ForEach(m => m.Prepare());
            models.ForEach(m => m.Check());
            models.ForEach(m => m.Propagate());
            var results = outputTester.GetOutput();
            for (int i=0; i<outputs.Count; i++)
            {
                if (!results[i].EqualTo(outputs[i]))
                {
                    return false;
                }
            }
            return true;
        }
    }
}