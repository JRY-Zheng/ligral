using System.Collections.Generic;
using ParameterDictionary = System.Collections.Generic.Dictionary<string, Ligral.Component.Parameter>;
using Ligral.Component;
using Ligral.Simulation;

namespace Ligral.Component.Models
{
    class Print : Model
    {
        protected override string DocString
        {
            get
            {
                return "This model prints the data to the console.";
            }
        }
        private string varName;
        private List<Observation> observations = new List<Observation>();
        protected override void SetUpPorts()
        {
            InPortList.Add(new InPort("input", this));
        }
        protected override void SetUpParameters()
        {
            Parameters = new ParameterDictionary()
            {
                {"name", new Parameter(value=>
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
                if (ScopeName != null)
                {
                    if (GivenName != null || inputSignal.Name == null)
                    {
                        varName = ScopeName + "." + varName;
                    }
                }
            }
            Observation.Stepped += () =>
                System.Console.WriteLine(string.Format("Time: {0,-6:0.00} {1,10} = {2:0.00}", Solver.Time, varName, inputSignal.ToStringInLine()));
            (int rowNo, int colNo) = inputSignal.Shape();
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