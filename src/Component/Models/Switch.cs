/* Copyright (C) 2019-2022 Junruoyu Zheng. Home page: https://junruoyu-zheng.gitee.io/ligral

   Distributed under MIT license.
   See file LICENSE for detail or copy at https://opensource.org/licenses/MIT
*/

using System;
using System.Collections.Generic;
using ParameterDictionary = System.Collections.Generic.Dictionary<string, Ligral.Component.Parameter>;
using Ligral.Component;
using MathNet.Numerics.LinearAlgebra;
using Ligral.Syntax.CodeASTs;

namespace Ligral.Component.Models
{
    class Switch : Model
    {
        protected override string DocString
        {
            get
            {
                return "This model accepts one condition and two values, returns first if the condition reaches threshold otherwise second.";
            }
        }
        private double threshold;
        protected override void SetUpPorts()
        {
            InPortList.Add(new InPort("condition", this));
            InPortList.Add(new InPort("first", this));
            InPortList.Add(new InPort("second", this));
            OutPortList.Add(new OutPort("result", this));
        }
        public override void Check()
        {
            int conditionRowNo = InPortList[0].RowNo;
            int conditionColNo = InPortList[0].ColNo;
            int firstRowNo = InPortList[1].RowNo;
            int firstColNo = InPortList[1].ColNo;
            int secondRowNo = InPortList[2].RowNo;
            int secondColNo = InPortList[2].ColNo;
            if (firstColNo != secondColNo)
            {
                throw logger.Error(new ModelException(this, $"Two inputs must have same column number but we got {firstColNo} and {secondColNo}"));
            }
            else if (firstRowNo != secondRowNo)
            {
                throw logger.Error(new ModelException(this, $"Two inputs must have same row number but we got {firstRowNo} and {secondRowNo}"));
            }
            try
            {
                (int xRowNo, int xColNo) = MatrixIteration.BroadcastShape(InPortList[0].RowNo, InPortList[0].ColNo, InPortList[1].RowNo, InPortList[1].ColNo);
                OutPortList[0].SetShape(xRowNo, xColNo);
            }
            catch (System.Exception e)
            {
                throw logger.Error(new ModelException(this, e.Message));
            }
        }
        protected override void SetUpParameters()
        {
            Parameters = new ParameterDictionary()
            {
                {"threshold", new Parameter(ParameterType.Signal , value=>
                {
                    threshold = (double)value;
                }, ()=>
                {
                    threshold = 0;
                })}
            };
        }
        protected override List<Matrix<double>> Calculate(List<Matrix<double>> values)
        {
            var condition = values[0].Map<double>(item => item >= threshold ? 1 : 0);
            Results[0] = condition.DotMul(values[1]) + (1 - condition).DotMul(values[2]);
            return Results;
        }
        public override List<int> GetCharacterSize()
        {
            return new List<int>() 
            {
                Math.Max(InPortList[0].RowNo, InPortList[1].RowNo),
                Math.Max(InPortList[0].ColNo, InPortList[1].ColNo)
            };
        }
        public override List<CodeAST> ConstructConfigurationAST()
        {
            Settings settings = Settings.GetInstance();
            var codeASTs = new List<CodeAST>();
            AssignCodeAST thresholdAST = new AssignCodeAST();
            thresholdAST.Destination = $"{GlobalName}.threshold";
            thresholdAST.Source = threshold.ToString();
            codeASTs.Add(thresholdAST);
            return codeASTs;
        }
    }
}