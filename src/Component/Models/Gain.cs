/* Copyright (C) 2019-2021 Junruoyu Zheng. Home page: https://junruoyu-zheng.gitee.io/ligral

   Distributed under MIT license.
   See file LICENSE for detail or copy at https://opensource.org/licenses/MIT
*/

using System.Collections.Generic;
using ParameterDictionary = System.Collections.Generic.Dictionary<string, Ligral.Component.Parameter>;
using MathNet.Numerics.LinearAlgebra;

namespace Ligral.Component.Models
{
    class Gain : Model
    {
        private Matrix<double> gain;
        private bool leftProduct = true;
        protected override string DocString
        {
            get
            {
                return "This model amplifies the input by a given constant.";
            }
        }
        protected override void SetUpParameters()
        {
            Parameters = new ParameterDictionary()
            {
                {"value", new Parameter(ParameterType.Signal , value=>
                {
                    gain = value.ToMatrix();
                })},
                {"prod", new Parameter(ParameterType.String , value=>
                {
                    string prod = (string) value;
                    switch (prod)
                    {
                    case "left":
                        leftProduct = true; break;
                    case "right":
                        leftProduct = false; break;
                    default:
                        throw logger.Error(new ModelException(this, $"Invalid enum prod {prod}"));
                    }
                }, ()=>{})}
            };
        }
        protected override List<Matrix<double>> Calculate(List<Matrix<double>> values)
        {
            try
            {
                if (leftProduct)
                {
                    Results[0] = gain.MatMul(values[0]);
                }
                else
                {
                    Results[0] = values[0].MatMul(gain);
                }
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
            return Results;
        }
    }
}