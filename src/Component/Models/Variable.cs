/* Copyright (C) 2019-2021 Junruoyu Zheng. Home page: https://junruoyu-zheng.gitee.io/ligral

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
        private int rowNo = 0;
        private int colNo = 0;
        private Signal initial = new Signal();
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
                    colNo = System.Convert.ToInt32(value);
                    InPortList[0].ColNo = colNo;
                }, ()=>{})},
                {"row", new Parameter(ParameterType.Signal , value=>
                {
                    rowNo = System.Convert.ToInt32(value);
                    InPortList[0].RowNo = rowNo;
                }, ()=>{})},
                {"initial", new Parameter(ParameterType.Signal , value=>
                {
                    initial.Pack(value);
                }, ()=>{})}
            };
        }
        public override void Prepare()
        {
            varName = varName ?? GivenName ?? Name;
        }
        public override void Check()
        {
            handle = Solution.CreateSolution(varName, rowNo, colNo, initial);
            OutPortList[0].SetShape(rowNo, colNo);
        }
        protected override List<Signal> Calculate(List<Signal> values)
        {
            Signal outputSignal = Results[0];
            outputSignal.Clone(handle.GetGuessedValue());
            return Results;
        }
    }
}