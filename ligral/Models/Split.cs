using System.Collections.Generic;
using System.Linq;
using MathNet.Numerics.LinearAlgebra;
using Ligral.Block;

namespace Ligral.Models
{
    class Split : OutPortVariableModel
    {
        protected override string DocString
        {
            get
            {
                return "This model splits inputs item-wise to doubles";
            }
        }
        protected override void SetUpPorts()
        {
            InPortList.Add(new InPort("matrix", this));
            base.SetUpPorts();
        }
        protected override List<Signal> Calculate(List<Signal> values)
        {
            Signal inputSignal = values[0];
            if (!inputSignal.IsMatrix)
            {
                throw new ModelException(this, "Double cannot be splitted.");
            }
            else if (inputSignal.Count() != Results.Count)
            {
                throw new ModelException(this, "Item count inconsistency.");
            }
            else
            {
                inputSignal.ZipApply<Signal>(Results, (value, signal) => signal.Pack(value));
            }
            return Results;
        }
    }
}