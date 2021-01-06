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
        protected override List<Signal> DefaultCalculate(List<Signal> values)
        {
            switch(type)
            {
            case "+":
                Calculate = AdditionCalculate; break;
            case "-":
                Calculate = SubstractionCalculate; break;
            case "*":
                Calculate = MultiplicationCalculate; break;
            case "/":
                Calculate = DivisionCalculate; break;
            case "^":
                Calculate = PowerCalculate; break;
            case ".*":
                Calculate = BroadcastMultiplicationCalculate; break;
            case "./":
                Calculate = BroadcastDivisionCalculate; break;
            case ".^":
                Calculate = BroadcastPowerCalculate; break;
            }// validation of operator is done in configure
            return Calculate(values);
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