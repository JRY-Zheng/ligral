/* Copyright (C) 2019-2022 Junruoyu Zheng. Home page: https://junruoyu-zheng.gitee.io/ligral

   Distributed under MIT license.
   See file LICENSE for detail or copy at https://opensource.org/licenses/MIT
*/

using System.Collections.Generic;
using ParameterDictionary = System.Collections.Generic.Dictionary<string, Ligral.Component.Parameter>;
using System;
using MathNet.Numerics.LinearAlgebra;
using Ligral.Syntax;
using Ligral.Syntax.CodeASTs;

namespace Ligral.Component.Models
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
        protected override bool HasConfiguration {get => true; }
        protected override void SetUpPorts()
        {
            OutPortList.Add(new OutPort("value", this));
        }
        public override void Check()
        {
            OutPortList[0].SetShape(Results[0].RowCount, Results[0].ColumnCount);
        }
        protected override void SetUpParameters()
        {
            Parameters = new ParameterDictionary()
            {
                {"value", new Parameter(ParameterType.Signal , value=>
                {
                    Results[0] = value.ToMatrix();
                })}
            };
        }
        protected override List<Matrix<double>> Calculate(List<Matrix<double>> values)
        {
            return Results;
        }
        internal override ConfigCodeAST ConstructConfigurationAST()
        {
            ConfigCodeAST configCodeAST = new ConfigCodeAST();
            DeclareCodeAST declareCodeAST = new DeclareCodeAST();
            declareCodeAST.Type = GetTypeName();
            declareCodeAST.Instance= GlobalName;
            configCodeAST.declareCodeAST = declareCodeAST;
            CopyCodeAST valueConfiguration = new CopyCodeAST();
            valueConfiguration.Destination = $"{GlobalName}.value";
            valueConfiguration.Source = $"{Results[0].ToScalar()}";// matrix handle
            List<CopyCodeAST> copyCodeASTs = new List<CopyCodeAST>();
            copyCodeASTs.Add(valueConfiguration);
            configCodeAST.copyCodeASTs = copyCodeASTs;
            return configCodeAST;
        }
    }
}