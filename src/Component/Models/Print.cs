using System.Collections.Generic;
using System;
using ParameterDictionary = System.Collections.Generic.Dictionary<string, Ligral.Component.Parameter>;
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
        private static int maxLength = 10;
        private static string format = "Time: {0,-8:0.0000} {1,10} = {2:0.00}";
        private static int length
        {
            set
            {
                if (value > maxLength)
                {
                    maxLength = value;
                    format = "Time: {0,-8:0.0000} {1,"+ maxLength +"} = {2:0.00}";
                }
            }
        }
        private string varName;
        private double start = 0;
        private double end = 0;
        private Signal inputSignal;
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
                }, ()=>{})},
                {"start", new Parameter(ParameterType.Signal , value=>
                {
                    start = Convert.ToInt32(value);
                }, ()=>
                {
                    start = 0;
                })},
                {"stop", new Parameter(ParameterType.Signal , value=>
                {
                    end = Convert.ToInt32(value);
                }, ()=>
                {
                    Settings settings = Settings.GetInstance();
                    end = settings.StopTime;
                })},
            };
        }
        protected override List<Signal> DefaultCalculate(List<Signal> values)
        {
            // Results.Clear();
            inputSignal = values[0];
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
            length = varName.Length;
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
        public override void Refresh()
        {
            if (Solver.Time > end || Solver.Time < start) return;
            System.Console.WriteLine(string.Format(format, Solver.Time, varName, inputSignal.ToStringInLine()));
        }
    }
}