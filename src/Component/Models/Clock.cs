using System.Collections.Generic;
using Ligral.Component;
using Ligral.Simulation;

namespace Ligral.Component.Models
{
    class Clock : Model
    {
        protected override string DocString
        {
            get
            {
                return "This model outputs the simulation time.";
            }
        }
        protected override void SetUpPorts()
        {
            OutPortList.Add(new OutPort("time", this));
        }
        protected override List<Signal> DefaultCalculate(List<Signal> values)
        {
            Results[0].Pack(Solver.Time);
            return Results;
        }
    }
}