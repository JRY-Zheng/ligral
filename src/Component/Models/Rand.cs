/* Copyright (C) 2019-2021 Junruoyu Zheng. Home page: https://junruoyu-zheng.gitee.io/ligral

   Distributed under MIT license.
   See file LICENSE for detail or copy at https://opensource.org/licenses/MIT
*/

using System.Collections.Generic;
using ParameterDictionary = System.Collections.Generic.Dictionary<string, Ligral.Component.Parameter>;
using System;
using MathNet.Numerics.LinearAlgebra;

namespace Ligral.Component.Models
{
    class Rand : Model
    {
        protected override string DocString
        {
            get
            {
                return "This model generates random value in given range.";
            }
        }
        private Random random;
        private int seed;
        private double upper;
        private double lower;
        private int rowNo;
        private int colNo;
        protected override void SetUpPorts()
        {
            OutPortList.Add(new OutPort("output", this));
        }
        protected override void SetUpParameters()
        {
            Parameters = new ParameterDictionary()
            {
                {"seed", new Parameter(ParameterType.Signal , value=>
                {
                    seed = Convert.ToInt32(value);
                    random = new Random(seed);
                }, ()=>
                {
                    seed = System.DateTime.Now.Millisecond;
                    random = new Random(seed);
                })},
                {"upper", new Parameter(ParameterType.Signal , value=>
                {
                    upper = (double)value;
                }, ()=>
                {
                    upper = 1;
                })},
                {"lower", new Parameter(ParameterType.Signal , value=>
                {
                    lower = (double)value;
                }, ()=>
                {
                    lower = 0;
                })},
                {"col", new Parameter(ParameterType.Signal , value=>
                {
                    colNo = (int)value;
                }, ()=>
                {
                    colNo = 0;
                })},
                {"row", new Parameter(ParameterType.Signal , value=>
                {
                    rowNo = (int)value;
                }, ()=>
                {
                    rowNo = 0;
                })},
            };
        }
        public override void Check()
        {
            OutPortList[0].SetShape(rowNo, colNo);
        }
        protected override List<Signal> DefaultCalculate(List<Signal> values)
        {
            Signal outputSignal = Results[0];
            if (rowNo == 0 && colNo == 0)
            {
                outputSignal.Pack(random.NextDouble() * (upper - lower) + lower);
            }
            else if (rowNo > 0 && colNo > 0)
            {
                var build = Matrix<double>.Build;
                var matrix = build.Dense(rowNo, colNo);
                Signal signal = new Signal(matrix);
                outputSignal.Pack(signal.Apply(_=>random.NextDouble() * (upper - lower) + lower));
            }
            return Results;
        }
    }
}