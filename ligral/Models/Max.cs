using System.Collections.Generic;
using System.Linq;
using Ligral.Block;

namespace Ligral.Models
{
    class Max : Model
    {
        protected override string DocString
        {
            get
            {
                return "This model returns maximal value among inputs";
            }
        }
        protected override void SetUpPorts()
        {
            InPortList.Add(new InPort("first", this));
            InPortList.Add(new InPort("second", this));
            OutPortList.Add(new OutPort("max", this));
        }
        protected override List<Signal> Calculate(List<Signal> values)
        {
            // Results.Clear();
            Results[0].Clone(values.Max());
            return Results;
        }
    }
}