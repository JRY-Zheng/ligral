using System.Collections.Generic;
using ParameterDictionary = System.Collections.Generic.Dictionary<string, Ligral.Block.Parameter>;
using Ligral.Block;
using Ligral.Simulation;

namespace Ligral.Models
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
                }, ()=>
                {
                    varName = Name;
                })}
            };
        }
        protected override List<Signal> DefaultCalculate(List<Signal> values)
        {
            // Results.Clear();
            Signal inputSignal = values[0];
            System.Console.WriteLine(string.Format("Time: {0,-6:0.00} {1,10} = {2:0.00}", Wanderer.Time, varName, inputSignal.ToStringInLine()));
            return Results;
        }
    }
}