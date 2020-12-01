using System.Collections.Generic;
using System.Linq;
using Ligral.Component;

namespace Ligral.Component.Models
{
    class Min : Model
    {
        protected override string DocString
        {
            get
            {
                return "This model returns minimal value among inputs";
            }
        }
        protected override void SetUpPorts()
        {
            InPortList.Add(new InPort("first", this));
            InPortList.Add(new InPort("second", this));
            OutPortList.Add(new OutPort("min", this));
        }
        protected override List<Signal> DefaultCalculate(List<Signal> values)
        {
            // Results.Clear();
            Results[0].Clone(values.Min());
            return Results;
        }
    }
}