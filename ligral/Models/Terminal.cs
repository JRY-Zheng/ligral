using System.Collections.Generic;
using System;

namespace Ligral.Models
{
    class Terminal : Model
    {
        protected override string DocString
        {
            get
            {
                return "This model does not do anything but hide input.";
            }
        }
        protected override void SetUpPorts()
        {
            InPortList.Add(new InPort("input", this));
        }
        protected override List<Signal> Calculate(List<Signal> values)
        {
            return Results;
        }
    }
}