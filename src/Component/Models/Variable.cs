/* Copyright (C) 2019-2022 Junruoyu Zheng. Home page: https://junruoyu-zheng.gitee.io/ligral

   Distributed under MIT license.
   See file LICENSE for detail or copy at https://opensource.org/licenses/MIT
*/

using System.Collections.Generic;
using System.Linq;
using MathNet.Numerics.LinearAlgebra;
using ParameterDictionary = System.Collections.Generic.Dictionary<string, Ligral.Component.Parameter>;
using Ligral.Simulation;


namespace Ligral.Component.Models
{
    class Variable : Model
    {
        protected override string DocString
        {
            get
            {
                return "This model marks the variable in functions";
            }
        }
        private string varName;
        private int rowNo = 1;
        private int colNo = 1;
        private Matrix<double> initial;
        private SolutionHandle handle;
        protected override void SetUpPorts()
        {
            OutPortList.Add(new OutPort("value", this));
        }
        protected override void SetUpParameters()
        {
            Parameters = new ParameterDictionary()
            {
                {"name", new Parameter(ParameterType.String , value=>
                {
                    varName = (string) value;
                }, ()=>{})},
                {"col", new Parameter(ParameterType.Signal , value=>
                {
                    colNo = value.ToInt();
                    InPortList[0].ColNo = colNo;
                }, ()=>{})},
                {"row", new Parameter(ParameterType.Signal , value=>
                {
                    rowNo = value.ToInt();
                    InPortList[0].RowNo = rowNo;
                }, ()=>{})},
                {"initial", new Parameter(ParameterType.Signal , value=>
                {
                    initial = value.ToMatrix();
                }, ()=>{})}
            };
        }
        public override void Prepare()
        {
            varName = varName ?? GivenName ?? Name;
        }
        public override void Check()
        {
            if (initial==null)
            {
                initial = Matrix<double>.Build.Dense(rowNo, colNo, 0);
            }
            handle = Solution.CreateSolution(varName, rowNo, colNo, initial);
            OutPortList[0].SetShape(rowNo, colNo);
        }
        protected override List<Matrix<double>> Calculate(List<Matrix<double>> values)
        {
            Results[0] = handle.GetGuessedValue();
            return Results;
        }
        public override List<int> GetCharacterSize()
        {
            throw logger.Error(new ModelException(this, "This model does not support code generation"));
        }
    }
}