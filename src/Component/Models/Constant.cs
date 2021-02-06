/* Copyright (C) 2019-2021 Junruoyu Zheng. Home page: https://junruoyu-zheng.gitee.io/ligral

   Distributed under MIT license.
   See file LICENSE for detail or copy at https://opensource.org/licenses/MIT
*/

using System.Collections.Generic;
using ParameterDictionary = System.Collections.Generic.Dictionary<string, Ligral.Component.Parameter>;
using System;
using MathNet.Numerics.LinearAlgebra;
using Ligral.Component;

namespace Ligral.Component.Models
{
    class Constant : Model
    {
        protected override string DocString
        {
            get
            {
                return "This model outputs the given constant.";
            }
        }
        protected override void SetUpPorts()
        {
            OutPortList.Add(new OutPort("value", this));
        }
        public override void Check()
        {
            (int rowNo, int colNo) = Results[0].Shape();
            OutPortList[0].SetShape(rowNo, colNo);
        }
        protected override void SetUpParameters()
        {
            Parameters = new ParameterDictionary()
            {
                {"value", new Parameter(ParameterType.Signal , value=>
                {
                    Matrix<double> matrix = value as Matrix<double>;
                    if (matrix!=null)
                    {
                        Results[0].Pack(matrix);
                    }
                    else
                    {
                        Results[0].Pack(Convert.ToDouble(value));
                    }
                })}
            };
        }
        protected override List<Signal> Calculate(List<Signal> values)
        {
            return Results;
        }
    }
}