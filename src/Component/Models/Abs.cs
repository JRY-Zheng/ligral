/* Copyright (C) 2019-2022 Junruoyu Zheng. Home page: https://junruoyu-zheng.gitee.io/ligral

   Distributed under MIT license.
   See file LICENSE for detail or copy at https://opensource.org/licenses/MIT
*/

using System.Collections.Generic;
using MathNet.Numerics.LinearAlgebra;
using Ligral.Simulation;

namespace Ligral.Component.Models
{
    class Abs : Model
    {
        protected override string DocString
        {
            get
            {
                return "This model calculates the absolute value.";
            }
        }
        private EventHandle handle;
        protected override void SetUpPorts()
        {
            InPortList.Add(new InPort("x", this));
            OutPortList.Add(new OutPort("y", this));
        }
        public override void Confirm()
        {
            handle = Event.CreateEvent(ScopedName, InPortList[0].RowNo, InPortList[0].ColNo);
        }
        protected override List<Matrix<double>> Calculate(List<Matrix<double>> values)
        {
            handle.SetCurrentValue(values[0]);
            Results[0] = values[0].PointwiseAbs();
            return Results;
        }
    }
}