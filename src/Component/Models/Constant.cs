/* Copyright (C) 2019-2021 Junruoyu Zheng. Home page: https://junruoyu-zheng.gitee.io/ligral

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
        internal override ConfigurationCodeAST ConstructConfigurationAST()
        {
            ConfigurationCodeAST configurationCodeAST = new ConfigurationCodeAST();
            DeclareCodeAST declareCodeAST = new DeclareCodeAST();
            declareCodeAST.Type = new CodeToken(CodeTokenType.WORD, GetTypeName());
            declareCodeAST.Instance= new CodeToken(CodeTokenType.WORD, Name);
            configurationCodeAST.declareCodeAST = declareCodeAST;
            CopyCodeAST valueConfiguration = new CopyCodeAST();
            valueConfiguration.Destination = new CodeToken(CodeTokenType.WORD, $"{Name}.value");
            valueConfiguration.Source = new CodeToken(CodeTokenType.WORD, $"{Results[0].ToScalar()}");// matrix handle
            List<CopyCodeAST> copyCodeASTs = new List<CopyCodeAST>();
            copyCodeASTs.Add(valueConfiguration);
            configurationCodeAST.copyCodeASTs = copyCodeASTs;
            return configurationCodeAST;
        }
    }
}