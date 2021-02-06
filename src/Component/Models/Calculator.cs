/* Copyright (C) 2019-2021 Junruoyu Zheng. Home page: https://junruoyu-zheng.gitee.io/ligral

   Distributed under MIT license.
   See file LICENSE for detail or copy at https://opensource.org/licenses/MIT
*/

using System.Collections.Generic;
using ParameterDictionary = System.Collections.Generic.Dictionary<string, Ligral.Component.Parameter>;
using Ligral.Component;

namespace Ligral.Component.Models
{
    class Calculator : Model
    {
        protected override string DocString
        {
            get
            {
                return "This model outputs the result of the algebraic operation defined by parameter type (+-*/^@).";
            }
        }
        private string type;
        protected override void SetUpPorts()
        {
            InPortList.Add(new InPort("left", this));
            InPortList.Add(new InPort("right", this));
            OutPortList.Add(new OutPort("result", this));
        }
        protected override void SetUpParameters()
        {
            Parameters = new ParameterDictionary()
            {
                {"type", new Parameter(ParameterType.String, value=>
                {
                    string operType = System.Convert.ToString(value);
                    if (operType=="+"||operType=="-"||operType=="*"||operType=="/"||operType=="^"||operType==".*"||operType=="./"||operType==".^")
                    {
                        type = operType;
                    }
                    else
                    {
                        throw logger.Error(new ModelException(this,"Invalid calculation operator " + operType));
                    }
                })}
            };
        }
        public override void Check()
        {
            var leftRowNo = InPortList[0].RowNo;
            var leftColNo = InPortList[0].ColNo;
            var rightRowNo = InPortList[0].RowNo;
            var rightColNo = InPortList[0].ColNo;
            int rowNo; 
            int colNo;
            switch(type)
            {
            case "+":
            case "-":
            case ".*":
            case "./":
            case ".^":
                if (leftRowNo == rightRowNo || rightRowNo <= 1)
                {
                    rowNo = leftRowNo;
                }
                else if (leftColNo == 1)
                {
                    rowNo = rightRowNo;
                }
                else
                {
                    throw logger.Error(new ModelException(this, $"Cannot broadcast row numbers from {leftRowNo} to {rightRowNo}"));
                }
                if (leftColNo == rightColNo || rightColNo <= 1)
                {
                    colNo = leftColNo;
                }
                else if (leftColNo == 1)
                {
                    colNo = rightColNo;
                }
                else
                {
                    throw logger.Error(new ModelException(this, $"Cannot broadcast column numbers from {leftColNo} to {rightColNo}"));
                }
                break;
            case "*":
                if (leftColNo == 0 && leftRowNo == 0)
                {
                    rowNo = rightRowNo;
                    colNo = rightColNo;
                }
                else if (rightColNo == 0 && rightRowNo == 0)
                {
                    rowNo = leftRowNo;
                    colNo = leftColNo;
                }
                else if (leftColNo != rightRowNo)
                {
                    throw logger.Error(new ModelException(this, $"Shape inconsistency: Left column number {leftColNo} not equal to right row number {rightRowNo}"));
                }
                else
                {
                    rowNo = leftRowNo;
                    colNo = rightColNo;
                }
                break;
            case "/":
                if (rightRowNo == 0 && rightColNo == 0)
                {
                    rowNo = leftRowNo;
                    colNo = leftColNo;
                }
                if (leftRowNo == 0 && leftColNo == 0)
                {
                    rowNo = rightRowNo;
                    colNo = rightColNo;
                }
                else if (rightColNo == rightRowNo && rightColNo == leftColNo)
                {
                    rowNo = leftRowNo;
                    colNo = leftColNo;
                }
                else
                {
                    throw logger.Error(new ModelException(this, $"Right matrix in division should be square and equal to left column number but ({leftRowNo}, {leftColNo})/({rightRowNo}, {rightColNo})"));
                }
                break;
            case "^":
                if (rightColNo != 0 || rightRowNo != 0)
                {
                    throw logger.Error(new ModelException(this, "Cannot raise matrices or scalars to the power of a matrix which may causes complex value"));
                }
                else if (leftColNo != leftRowNo)
                {
                    throw logger.Error(new ModelException(this, $"Cannot raise a non-square matrix ({leftRowNo}, {leftColNo}) to the power of a scalar"));
                }
                else
                {
                    rowNo = leftRowNo;
                    colNo = leftColNo;
                }
                break;
            default:
                throw logger.Error(new ModelException(this, $"Unknown type {type}"));
            }
            OutPortList[0].SetShape(rowNo, colNo);
        }
        protected override List<Signal> Calculate(List<Signal> values)
        {
            try
            {
                switch(type)
                {
                case "+":
                    return AdditionCalculate(values);
                case "-":
                    return SubstractionCalculate(values);
                case "*":
                    return MultiplicationCalculate(values);
                case "/":
                    return DivisionCalculate(values);
                case "^":
                    return PowerCalculate(values);
                case ".*":
                    return BroadcastMultiplicationCalculate(values);
                case "./":
                    return BroadcastDivisionCalculate(values);
                case ".^":
                    return BroadcastPowerCalculate(values);
                default:
                    throw logger.Error(new ModelException(this, $"Unknown type {type}"));
                }// validation of operator is done in configure
            }
            catch (System.ArgumentException e)
            {
                string message = e.Message;
                int indexOfParenthesis = message.IndexOf('(');
                if (indexOfParenthesis>=0)
                {
                    message = message.Substring(0, indexOfParenthesis);
                }
                throw logger.Error(new ModelException(this, message));
            }
        }
        private List<Signal> AdditionCalculate(List<Signal> values)
        {
            Signal leftSignal = values[0];
            Signal rightSignal = values[1];
            Signal outputSignal = Results[0];
            outputSignal.Clone(leftSignal + rightSignal);
            return Results;
        }
        private List<Signal> SubstractionCalculate(List<Signal> values)
        {
            Signal leftSignal = values[0];
            Signal rightSignal = values[1];
            Signal outputSignal = Results[0];
            outputSignal.Clone(leftSignal - rightSignal);
            return Results;
        }
        private List<Signal> MultiplicationCalculate(List<Signal> values)
        {
            Signal leftSignal = values[0];
            Signal rightSignal = values[1];
            Signal outputSignal = Results[0];
            outputSignal.Clone(leftSignal * rightSignal);
            return Results;
        }
        private List<Signal> DivisionCalculate(List<Signal> values)
        {
            Signal leftSignal = values[0];
            Signal rightSignal = values[1];
            Signal outputSignal = Results[0];
            outputSignal.Clone(leftSignal / rightSignal);
            return Results;
        }
        private List<Signal> PowerCalculate(List<Signal> values)
        {
            Signal leftSignal = values[0];
            Signal rightSignal = values[1];
            Signal outputSignal = Results[0];
            outputSignal.Clone(leftSignal.RaiseToPower(rightSignal));
            return Results;
        }
        private List<Signal> BroadcastMultiplicationCalculate(List<Signal> values)
        {
            Signal leftSignal = values[0];
            Signal rightSignal = values[1];
            Signal outputSignal = Results[0];
            outputSignal.Clone(leftSignal.BroadcastMultiply(rightSignal));
            return Results;
        }
        private List<Signal> BroadcastDivisionCalculate(List<Signal> values)
        {
            Signal leftSignal = values[0];
            Signal rightSignal = values[1];
            Signal outputSignal = Results[0];
            outputSignal.Clone(leftSignal.BroadcastDivide(rightSignal));
            return Results;
        }
        private List<Signal> BroadcastPowerCalculate(List<Signal> values)
        {
            Signal leftSignal = values[0];
            Signal rightSignal = values[1];
            Signal outputSignal = Results[0];
            outputSignal.Clone(leftSignal.BroadcastPower(rightSignal));
            return Results;
        }
    }
}