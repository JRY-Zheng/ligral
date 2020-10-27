using System.Collections.Generic;
using ParameterDictionary = System.Collections.Generic.Dictionary<string, Ligral.Block.Parameter>;
using Ligral.Block;

namespace Ligral.Models
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
        private char type;
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
                {"type", new Parameter(value=>
                {
                    char operType = System.Convert.ToChar(value);
                    if (operType=='+'||operType=='-'||operType=='*'||operType=='/'||operType=='^'||operType=='@')
                    {
                        type = operType;
                    }
                    else
                    {
                        throw new ModelException(this,"Invalid calculation operator " + operType);
                    }
                })}
            };
        }
        protected override List<Signal> DefaultCalculate(List<Signal> values)
        {
            // Results.Clear();
            Signal leftSignal = values[0];
            Signal rightSignal = values[1];
            Signal outputSignal = Results[0];
            if (type == '+')
            {
                outputSignal.Clone(leftSignal + rightSignal);
            }
            else if (type == '-')
            {
                outputSignal.Clone(leftSignal - rightSignal);
            }
            else if (type == '*')
            {
                outputSignal.Clone(leftSignal * rightSignal);
            }
            else if (type == '/')
            {
                outputSignal.Clone(leftSignal / rightSignal);
            }
            else if (type == '^')
            {
                outputSignal.Clone(leftSignal ^ rightSignal);
            }
            else if (type == '@')
            {
                outputSignal.Clone(leftSignal & rightSignal);
            }// validation of operator is done in configure
            return Results;
        }
    }
}