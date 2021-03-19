/* Copyright (C) 2019-2021 Junruoyu Zheng. Home page: https://junruoyu-zheng.gitee.io/ligral

   Distributed under MIT license.
   See file LICENSE for detail or copy at https://opensource.org/licenses/MIT
*/

using System.Collections.Generic;
using ParameterDictionary = System.Collections.Generic.Dictionary<string, Ligral.Component.Parameter>;
using System;
using MathNet.Numerics.LinearAlgebra;
using Ligral.Simulation;

namespace Ligral.Component.Models
{
    class Sweep : Model
    {
        protected override string DocString
        {
            get
            {
                return "This model generates frequency-sweep signal with frequency from f1 to f2.";
            }
        }
        private double f1 = 10;
        private double f2 = 1;
        protected override void SetUpPorts()
        {
            OutPortList.Add(new OutPort("source", this));
        }
        public override void Check()
        {
            OutPortList[0].SetShape(1, 1);
        }
        protected override void SetUpParameters()
        {
            Parameters = new ParameterDictionary()
            {
                {"f1", new Parameter(ParameterType.Signal , value=>
                {
                    f1 = (double)value;
                })},
                {"f2", new Parameter(ParameterType.Signal , value=>
                {
                    f2 = (double)value;
                })},
            };
        }
        protected override List<Matrix<double>> Calculate(List<Matrix<double>> values)
        {
            Settings settings = Settings.GetInstance();
            var k = (f1-f2)/settings.StopTime/f2;
            var a = settings.StopTime*f1*f2/(f1-f2);
            var x = Solver.Time*k + 1;
            var l = Math.Log(x)*a*2*Math.PI;
            Results[0] = Math.Sin(l).ToMatrix();
            return Results;
        }
    }
}