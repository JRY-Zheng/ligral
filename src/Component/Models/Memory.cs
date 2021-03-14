/* Copyright (C) 2019-2021 Junruoyu Zheng. Home page: https://junruoyu-zheng.gitee.io/ligral

   Distributed under MIT license.
   See file LICENSE for detail or copy at https://opensource.org/licenses/MIT
*/

using System.Collections.Generic;
using ParameterDictionary = System.Collections.Generic.Dictionary<string, Ligral.Component.Parameter>;
using System;
using MathNet.Numerics.LinearAlgebra;
using Ligral.Component;
using Ligral.Simulation;

namespace Ligral.Component.Models
{
    class Memory : InitializeableModel
    {
        protected override string DocString
        {
            get
            {
                return "This model outputs the value from the previous input.";
            }
        }
        private Matrix<double> stack;
        protected override void SetUpParameters()
        {
            base.SetUpParameters();
        }
        protected override void AfterConfigured()
        {
            base.AfterConfigured();
            initial.CopyTo(stack);
        }
        protected override List<Matrix<double>> Calculate(List<Matrix<double>> values)
        {
            stack.CopyTo(Results[0]);
            values[0].CopyTo(stack);
            return Results;
        }
    }
}