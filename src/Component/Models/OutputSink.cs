/* Copyright (C) 2019-2021 Junruoyu Zheng. Home page: https://junruoyu-zheng.gitee.io/ligral

   Distributed under MIT license.
   See file LICENSE for detail or copy at https://opensource.org/licenses/MIT
*/

using System.Collections.Generic;
using ParameterDictionary = System.Collections.Generic.Dictionary<string, Ligral.Component.Parameter>;
using Ligral.Simulation;

namespace Ligral.Component.Models
{
    class OutputSink : Model
    {
        protected override string DocString
        {
            get
            {
                return "This model marks thee input signal as control output.";
            }
        }
        private string varName;
        private int rowNo;
        private int colNo;
        private List<Observation> observations = new List<Observation>();
        protected override void SetUpPorts()
        {
            InPortList.Add(new InPort("input", this));
        }
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
                        observations.Add(Observation.CreateObservation($"{varName}({i}-{j})"));
                    }
                }
            }
            else
            {
                observations.Add(Observation.CreateObservation(varName));
            }
            Calculate = PostCalculate;
            return Calculate(values);
        }
        protected List<Signal> PostCalculate(List<Signal> values)
        {
            Signal inputSignal = values[0];
            inputSignal.ZipApply<Observation>(observations, (value, observation) => observation.Cache(value));
            return Results;
        }
    }
}