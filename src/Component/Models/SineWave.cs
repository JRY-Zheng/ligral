/* Copyright (C) 2019-2022 Junruoyu Zheng. Home page: https://junruoyu-zheng.gitee.io/ligral

   Distributed under MIT license.
   See file LICENSE for detail or copy at https://opensource.org/licenses/MIT
*/

using System.Collections.Generic;
using ParameterDictionary = System.Collections.Generic.Dictionary<string, Ligral.Component.Parameter>;
using System;
using MathNet.Numerics.LinearAlgebra;
using Ligral.Simulation;
using Ligral.Syntax.CodeASTs;

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
                    ampl = value.ToScalar();
                }, ()=>
                {
                    ampl = 1;
                })},
                {"omega", new Parameter(ParameterType.Signal , value=>
                {
                    omega = value.ToScalar();
                }, ()=>
                {
                    omega = 1;
                })},
                {"phi", new Parameter(ParameterType.Signal , value=>
                {
                    phi = value.ToScalar();
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
        public override List<CodeAST> ConstructConfigurationAST()
        {
            var codeASTs = new List<CodeAST>();
            AssignCodeAST ctxAST = new AssignCodeAST();
            ctxAST.Destination = $"{GlobalName}.ctx";
            ctxAST.Source = "&ctx";
            codeASTs.Add(ctxAST);
            AssignCodeAST amplAST = new AssignCodeAST();
            amplAST.Destination = $"{GlobalName}.ampl";
            amplAST.Source = ampl.ToString();
            codeASTs.Add(amplAST);
            AssignCodeAST omegaAST = new AssignCodeAST();
            omegaAST.Destination = $"{GlobalName}.omega";
            omegaAST.Source = omega.ToString();
            codeASTs.Add(omegaAST);
            AssignCodeAST phiAST = new AssignCodeAST();
            phiAST.Destination = $"{GlobalName}.phi";
            phiAST.Source = phi.ToString();
            codeASTs.Add(phiAST);
            return codeASTs;
        }
    }
}