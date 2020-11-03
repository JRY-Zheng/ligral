using System.Collections.Generic;
using System;
using Ligral.Component;

namespace Ligral.Component.Models
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
        protected override List<Signal> DefaultCalculate(List<Signal> values)
        {
            return Results;
        }
    }
}