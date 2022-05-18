/* Copyright (C) 2019-2022 Junruoyu Zheng. Home page: https://junruoyu-zheng.gitee.io/ligral

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
                    seed = value.ToInt();
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
            Results[0] = Matrix<double>.Build.Dense(rowNo, colNo);
        }
        protected override List<Matrix<double>> Calculate(List<Matrix<double>> values)
        {
            Results[0] = Results[0].Map(_=>random.NextDouble() * (upper - lower) + lower);
            return Results;
        }
    }
}