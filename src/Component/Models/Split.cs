/* Copyright 2019-2020 Junruoyu Zheng. All rights reserved.

   Distributed under MIT license.
   See file LICENSE for detail or copy at https://opensource.org/licenses/MIT
*/

using System.Collections.Generic;
using System.Linq;
using MathNet.Numerics.LinearAlgebra;
using Ligral.Component;

namespace Ligral.Component.Models
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
        protected override List<Signal> DefaultCalculate(List<Signal> values)
        {
            Signal inputSignal = values[0];
            if (!inputSignal.IsMatrix)
            {
                throw logger.Error(new ModelException(this, "Double cannot be splitted."));
            }
            else if (inputSignal.Count() != Results.Count)
            {
                throw logger.Error(new ModelException(this, "Item count inconsistency."));
            }
            else
            {
                inputSignal.ZipApply<Signal>(Results, (value, signal) => signal.Pack(value));
            }
            return Results;
        }
    }
}