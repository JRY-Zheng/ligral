/* Copyright (C) 2019-2022 Junruoyu Zheng. Home page: https://junruoyu-zheng.gitee.io/ligral

   Distributed under MIT license.
   See file LICENSE for detail or copy at https://opensource.org/licenses/MIT
*/

using System.Collections.Generic;
using ParameterDictionary = System.Collections.Generic.Dictionary<string, Ligral.Component.Parameter>;
using MathNet.Numerics.LinearAlgebra;
using Ligral.Simulation;
using Ligral.Syntax.CodeASTs;

namespace Ligral.Component.Models
{
    class Step : Model
    {
        protected override string DocString
        {
            get
            {
                return "This model provides a step between 0 and a definable level at a specified time.";
            }
        }
        private double start = 0;
        private double level = 1;
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
                {"start", new Parameter(ParameterType.Signal , value=>
                {
                    start = (double)value;
                }, ()=>
                {
                    start = 0;
                })},
                {"level", new Parameter(ParameterType.Signal , value=>
                {
                    level = (double)value;
                }, ()=>
                {
                    level = 1;
                })},
            };
        }
        protected override List<Matrix<double>> Calculate(List<Matrix<double>> values)
        {
            if (Solver.Time >= start)
            {
                Results[0] = level.ToMatrix();
            }
            else
            {
                Results[0] = 0.ToMatrix();
            }
            return Results;
        }
        public override List<CodeAST> ConstructConfigurationAST()
        {
            var codeASTs = new List<CodeAST>();
            AssignCodeAST ctxAST = new AssignCodeAST();
            ctxAST.Destination = $"{GlobalName}.ctx";
            ctxAST.Source = "&ctx";
            codeASTs.Add(ctxAST);
            AssignCodeAST startAST = new AssignCodeAST();
            startAST.Destination = $"{GlobalName}.start";
            startAST.Source = start.ToString();
            codeASTs.Add(startAST);
            AssignCodeAST levelAST = new AssignCodeAST();
            levelAST.Destination = $"{GlobalName}.level";
            levelAST.Source = level.ToString();
            codeASTs.Add(levelAST);
            return codeASTs;
        }
    }
}