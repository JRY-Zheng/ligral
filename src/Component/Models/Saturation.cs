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
    class Saturation : Model
    {
        private double upper = 1;
        private double lower = -1;
        protected override string DocString
        {
            get
            {
                return "This model produces an output signal that is the value of the input signal bounded to the upper and lower.";
            }
        }
        protected override void SetUpParameters()
        {
            Parameters = new ParameterDictionary()
            {
                {"upper", new Parameter(ParameterType.Signal , value=>
                {
                    upper = value.ToScalar();
                })},
                {"lower", new Parameter(ParameterType.Signal , value=>
                {
                    lower = value.ToScalar();
                })},
            };
        }
        protected override List<Matrix<double>> Calculate(List<Matrix<double>> values)
        {
            Results[0] = values[0].Map((item) =>
            {
                if (item < upper && item > lower)
                {
                    return item;
                }
                else if (item <= lower)
                {
                    return lower;
                }
                else //item>=upper
                {
                    return upper;
                }
            });
            return Results;
        }
        public override List<CodeAST> ConstructConfigurationAST()
        {
            var codeASTs = new List<CodeAST>();
            AssignCodeAST upperConfiguration = new AssignCodeAST();
            upperConfiguration.Destination = $"{GlobalName}.upper";
            upperConfiguration.Source = upper.ToString();
            codeASTs.Add(upperConfiguration);
            AssignCodeAST lowerConfiguration = new AssignCodeAST();
            lowerConfiguration.Destination = $"{GlobalName}.lower";
            lowerConfiguration.Source = lower.ToString();
            codeASTs.Add(lowerConfiguration);
            return codeASTs;
        }
    }
}