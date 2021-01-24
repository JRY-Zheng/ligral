/* Copyright (C) 2019-2021 Junruoyu Zheng. Home page: https://junruoyu-zheng.gitee.io/ligral

   Distributed under MIT license.
   See file LICENSE for detail or copy at https://opensource.org/licenses/MIT
*/

using System.Collections.Generic;
using System.Linq;
using MathNet.Numerics.LinearAlgebra;
using ParameterDictionary = System.Collections.Generic.Dictionary<string, Ligral.Component.Parameter>;
using Ligral.Simulation;


namespace Ligral.Component.Models
{
    class InputMarker : Model
    {
        protected override string DocString
        {
            get
            {
                return "This model marks the input signal as control input.";
            }
        }
        private string varName;
        private int rowNo;
        private int colNo;
        private List<ControlInput> controlInputs = new List<ControlInput>();
        protected override void SetUpParameters()
        {
            Parameters = new ParameterDictionary()
            {
                {"name", new Parameter(ParameterType.String , value=>
                {
                    varName = (string) value;
                }, ()=>{})}
            };
        }
        protected override List<Signal> DefaultCalculate(List<Signal> values)
        {
            // Results.Clear();
            Signal inputSignal = values[0];
            if (varName == null)
            {
                varName = GivenName ?? inputSignal.Name ?? Name;
                if (Scope != null)
                {
                    if (GivenName != null || inputSignal.Name == null)
                    {
                        varName = Scope + "." + varName;
                    }
                }
            }
            (rowNo, colNo) = inputSignal.Shape();
            if (inputSignal.IsMatrix)
            {
                for(int i = 0; i < rowNo; i++)
                {
                    for (int j = 0; j < colNo; j++)
                    {
                        controlInputs.Add(ControlInput.CreateInput($"{varName}({i}-{j})"));
                    }
                }
            }
            else
            {
                controlInputs.Add(ControlInput.CreateInput(varName));
            }
            Calculate = PostCalculate;
            return Calculate(values);
        }
        protected List<Signal> PostCalculate(List<Signal> values)
        {
            Signal inputSignal = values[0];
            Signal outputSignal = Results[0];
            inputSignal.ZipApply<ControlInput>(controlInputs, (value, controlInput) => controlInput.ClosedLoopInput = value);
            if (rowNo==0 && colNo==0)
            {
                outputSignal.Pack(controlInputs[0].Input);
            }
            else
            {
                IEnumerable<double> row = controlInputs.ConvertAll(controlInput => controlInput.Input);
                MatrixBuilder<double> m = Matrix<double>.Build;
                Matrix<double> mat = m.DenseOfRowMajor(1, colNo, row.Take(colNo));
                for (int i = 1; i < rowNo; i++)
                {
                    row = row.Skip(colNo);
                    Matrix<double> vec = m.DenseOfRowMajor(1, colNo, row.Take(colNo));
                    mat = mat.Stack(vec);
                }
                outputSignal.Pack(mat);
            }
            return Results;
        }
    }
}