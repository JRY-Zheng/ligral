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
        private int rowNo = 1;
        private int colNo = 1;
        private Matrix<double> cache;
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
                    upper = value.ToScalar();
                }, ()=>
                {
                    upper = 1;
                })},
                {"lower", new Parameter(ParameterType.Signal , value=>
                {
                    lower = value.ToScalar();
                }, ()=>
                {
                    lower = 0;
                })},
                {"col", new Parameter(ParameterType.Signal , value=>
                {
                    colNo = value.ToInt();
                }, ()=>{})},
                {"row", new Parameter(ParameterType.Signal , value=>
                {
                    rowNo = value.ToInt();
                }, ()=>{})},
            };
        }
        public override void Check()
        {
            OutPortList[0].SetShape(rowNo, colNo);
            Results[0] = Matrix<double>.Build.Dense(rowNo, colNo);
            Refresh();
        }
        public override void Refresh()
        {
            cache = Results[0].Map(_=>random.NextDouble() * (upper - lower) + lower);
        }
        protected override List<Matrix<double>> Calculate(List<Matrix<double>> values)
        {
            Results[0] = cache;
            return Results;
        }
    }
}