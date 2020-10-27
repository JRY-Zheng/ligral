using System.Collections.Generic;
using Ligral.Block;

namespace Ligral.Models
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
        protected override List<Signal> Calculate(List<Signal> values)
        {
            Results[0].Pack(time);
            return Results;
        }
    }
}