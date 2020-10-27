using System.Collections.Generic;
using ParameterDictionary = System.Collections.Generic.Dictionary<string, Ligral.Block.Parameter>;
using System;
using MathNet.Numerics.LinearAlgebra;
using Ligral.Block;

namespace Ligral.Models
{
    class Constant : Model
    {
        protected override string DocString
        {
            get
            {
                return "This model outputs the given constant.";
            }
        }
        protected override void SetUpPorts()
        {
            OutPortList.Add(new OutPort("value", this));
        }
        protected override void SetUpParameters()
        {
            Parameters = new ParameterDictionary()
            {
                {"value", new Parameter(value=>
                {
                    Matrix<double> matrix = value as Matrix<double>;
                    if (matrix!=null)
                    {
                        Results[0].Pack(matrix);
                    }
                    else
                    {
                        Results[0].Pack(Convert.ToDouble(value));
                    }
                })}
            };
        }
        protected override List<Signal> Calculate(List<Signal> values)
        {
            return Results;
        }
    }
}