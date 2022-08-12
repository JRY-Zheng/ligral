/* Copyright (C) 2019-2022 Junruoyu Zheng. Home page: https://junruoyu-zheng.gitee.io/ligral

   Distributed under MIT license.
   See file LICENSE for detail or copy at https://opensource.org/licenses/MIT
*/

using System.Collections.Generic;
using ParameterDictionary = System.Collections.Generic.Dictionary<string, Ligral.Component.Parameter>;
using System;
using MathNet.Numerics.LinearAlgebra;
using Ligral.Component;
using Ligral.Syntax.CodeASTs;

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
        public override void Confirm()
        {
            stack = Matrix<double>.Build.DenseOfMatrix(initial);
            Results[0] = Matrix<double>.Build.Dense(rowNo, colNo);
        }
        protected override void InputUpdate(Matrix<double> inputSignal)
        {
            inputSignal.CopyTo(stack);
        }
        protected override List<Matrix<double>> Calculate(List<Matrix<double>> values)
        {
            stack.CopyTo(Results[0]);
            return Results;
        }
        public override List<CodeAST> ConstructConfigurationAST()
        {
            var codeASTs = new List<CodeAST>();
            LShiftCodeAST stackAST = new LShiftCodeAST();
            stackAST.Destination = $"{GlobalName}.stack";
            stackAST.Source = string.Join(',', initial.ToColumnMajorArray());
            codeASTs.Add(stackAST);
            return codeASTs;
        }
        public override List<CodeAST> ConstructRefreshAST()
        {
            var codeASTs = new List<CodeAST>();
            CallCodeAST refreshAST = new CallCodeAST();
            refreshAST.FunctionName = $"{GlobalName}.refresh";
            codeASTs.Add(refreshAST);
            return codeASTs;
        }
    }
}