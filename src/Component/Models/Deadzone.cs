/* Copyright (C) 2019-2021 Junruoyu Zheng. Home page: https://junruoyu-zheng.gitee.io/ligral

   Distributed under MIT license.
   See file LICENSE for detail or copy at https://opensource.org/licenses/MIT
*/

using System.Collections.Generic;
using ParameterDictionary = System.Collections.Generic.Dictionary<string, Ligral.Component.Parameter>;
using MathNet.Numerics.LinearAlgebra;

namespace Ligral.Component.Models
{
    class Deadzone : Model
    {
        private double left = -1;
        private double right = 1;
        protected override string DocString
        {
            get
            {
                return "This model generates zero output within a specified region [left, right].";
            }
        }
        protected override void SetUpParameters()
        {
            Parameters = new ParameterDictionary()
            {
                {"left", new Parameter(ParameterType.Signal , value=>
                {
                    left = (double)value;
                })},
                {"right", new Parameter(ParameterType.Signal , value=>
                {
                    right = (double)value;
                })},
            };
        }
        protected override List<Matrix<double>> Calculate(List<Matrix<double>> values)
        {
            Matrix<double> matrix = values[0];
            Results[0] = matrix.Map((item) =>
            {
                if (item < right && item > left)
                {
                    return 0;
                }
                else if (item <= left)
                {
                    return item - left;
                }
                else //item>=right
                {
                    return item - right;
                }
            });
            return Results;
        }
    }
}