/* Copyright (C) 2019-2022 Junruoyu Zheng. Home page: https://junruoyu-zheng.gitee.io/ligral

   Distributed under MIT license.
   See file LICENSE for detail or copy at https://opensource.org/licenses/MIT
*/

using System.Collections.Generic;
using ParameterDictionary = System.Collections.Generic.Dictionary<string, Ligral.Component.Parameter>;
using Ligral.Simulation;
using MathNet.Numerics.LinearAlgebra;

namespace Ligral.Component.Models
{
    class EqualToZero : Model
    {
        protected override string DocString
        {
            get
            {
                return "This model marks the input signal as the value of a function which should be zero.";
            }
        }
        private string varName;
        private int rowNo = 0;
        private int colNo = 0;
        private FunctionHandle handle;
        protected override void SetUpPorts()
        {
            InPortList.Add(new InPort("input", this));
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
                    if (colNo <= 0)
                    {
                        throw logger.Error(new ModelException(this, $"column number should be positive but got {colNo}"));
                    }
                }, ()=>{})},
                {"row", new Parameter(ParameterType.Signal , value=>
                {
                    rowNo = value.ToInt();
                    if (rowNo <= 0)
                    {
                        throw logger.Error(new ModelException(this, $"row number should be positive but got {rowNo}"));
                    }
                }, ()=>{})}
            };
        }
        protected override void AfterConfigured()
        {
            base.AfterConfigured();
            // if ()
        }
        public override void Prepare()
        {
            string inputSignalName = InPortList[0].Source.SignalName;
            if (varName == null)
            {
                varName = GivenName ?? inputSignalName ?? Name;
                if (Scope != null)
                {
                    if (GivenName != null || inputSignalName == null)
                    {
                        varName = Scope + "." + varName;
                    }
                }
            }
        }
        public override void Check()
        {
            if (rowNo>0 || colNo>0)
            {
                if (rowNo != InPortList[0].RowNo)
                {
                    throw logger.Error(new ModelException(this, $"Shape inconsistency. {InPortList[0].RowNo} rows got, {rowNo} expected."));
                }
                else if (colNo != InPortList[0].ColNo)
                {
                    throw logger.Error(new ModelException(this, $"Shape inconsistency. {InPortList[0].ColNo} columns got, {colNo} expected."));
                }
            }
            else
            {
                rowNo = InPortList[0].RowNo;
                colNo = InPortList[0].ColNo;
            }
            handle = Function.CreateFunction(varName, rowNo, colNo);
        }
        protected override List<Matrix<double>> Calculate(List<Matrix<double>> values)
        {
            Matrix<double> inputSignal = values[0];
            handle.SetActualValue(inputSignal);
            return Results;
        }
        public override List<int> GetCharacterSize()
        {
            throw logger.Error(new ModelException(this, "This model does not support code generation"));
        }
    }
}