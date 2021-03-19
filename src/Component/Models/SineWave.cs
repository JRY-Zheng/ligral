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
    class SineWave : Model
    {
        protected override string DocString
        {
            get
            {
                return "This model outputs ampl*sin(omega*time+phi).";
            }
        }
        private double ampl = 1;
        private double omega = 1;
        private double phi = 0;
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
                {"ampl", new Parameter(ParameterType.Signal , value=>
                {
                    ampl = (double)value;
                }, ()=>
                {
                    ampl = 1;
                })},
                {"omega", new Parameter(ParameterType.Signal , value=>
                {
                    omega = (double)value;
                }, ()=>
                {
                    omega = 1;
                })},
                {"phi", new Parameter(ParameterType.Signal , value=>
                {
                    phi = (double)value;
                }, ()=>
                {
                    phi = 0;
                })},
            };
        }
        protected override List<Matrix<double>> Calculate(List<Matrix<double>> values)
        {
            Results[0] = (ampl * Math.Sin(omega * Solver.Time + phi)).ToMatrix();
            return Results;
        }
    }
}