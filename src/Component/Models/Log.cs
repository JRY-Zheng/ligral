/* Copyright (C) 2019-2021 Junruoyu Zheng. Home page: https://junruoyu-zheng.gitee.io/ligral

   Distributed under MIT license.
   See file LICENSE for detail or copy at https://opensource.org/licenses/MIT
*/

using System.Linq;
using System.Collections.Generic;
using ParameterDictionary = System.Collections.Generic.Dictionary<string, Ligral.Component.Parameter>;
using System;
using MathNet.Numerics.LinearAlgebra;

namespace Ligral.Component.Models
{
    class Log : Model
    {
        protected override string DocString
        {
            get
            {
                return "This model calculates y=log(x)/log(base), where default base is e.";
            }
        }
        private double newBase = Math.E;
        protected override void SetUpPorts()
        {
            InPortList.Add(new InPort("x", this));
            OutPortList.Add(new OutPort("y", this));
        }
        protected override void SetUpParameters()
        {
            Parameters = new ParameterDictionary()
            {
                {"base", new Parameter(ParameterType.Signal , value=>
                {
                    newBase = value.ToScalar();
                }, ()=>{})}
            };
        }
        protected override List<Matrix<double>> Calculate(List<Matrix<double>> values)
        {
            Results[0] = values[0].Map(x => Math.Log(x, newBase));
            if (Results[0].Enumerate().Contains(double.NaN))
            {
                throw logger.Error(new ModelException(this, "The input of log cannot be negative"));
            }
            else if (Results[0].Enumerate().Contains(double.NegativeInfinity))
            {
                throw logger.Error(new ModelException(this, "The input of log cannot be zero"));
            }
            return Results;
        }
    }
}