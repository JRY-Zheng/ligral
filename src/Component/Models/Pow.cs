/* Copyright (C) 2019-2022 Junruoyu Zheng. Home page: https://junruoyu-zheng.gitee.io/ligral

   Distributed under MIT license.
   See file LICENSE for detail or copy at https://opensource.org/licenses/MIT
*/

using System.Collections.Generic;
using ParameterDictionary = System.Collections.Generic.Dictionary<string, Ligral.Component.Parameter>;
using MathNet.Numerics.LinearAlgebra;
using Ligral.Syntax.CodeASTs;

namespace Ligral.Component.Models
{
    class Pow : Model
    {
        protected override string DocString
        {
            get
            {
                return "This model calculates y=x^power.";
            }
        }
        private int power;
        protected override void SetUpPorts()
        {
            InPortList.Add(new InPort("x", this));
            OutPortList.Add(new OutPort("y", this));
        }
        protected override void SetUpParameters()
        {
            Parameters = new ParameterDictionary()
            {
                {"power", new Parameter(ParameterType.Signal , value=>
                {
                    power = value.ToInt();
                })}
            };
        }
        public override void Check()
        {
            if (InPortList[0].RowNo != InPortList[0].ColNo)
            {
                throw logger.Error(new ModelException(this, $"The base matrix should be a square matrix, but a matrix with shape {InPortList[0].RowNo}x{InPortList[0].ColNo} received"));
            }
            else
            {
                OutPortList[0].SetShape(InPortList[0].RowNo, InPortList[0].ColNo);
            }
        }
        protected override List<Matrix<double>> Calculate(List<Matrix<double>> values)
        {
            Results[0] = values[0].Power(power);
            return Results;
        }
        public override List<CodeAST> ConstructConfigurationAST()
        {
            var codeASTs = new List<CodeAST>();
            AssignCodeAST powerAST = new AssignCodeAST();
            powerAST.Destination = $"{GlobalName}.power";
            powerAST.Source = power.ToString();
            codeASTs.Add(powerAST);
            return codeASTs;
        }
        public override List<int> GetCharacterSize()
        {
            return new List<int>() { InPortList[0].RowNo };
        }
    }
}