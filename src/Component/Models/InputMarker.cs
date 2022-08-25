/* Copyright (C) 2019-2022 Junruoyu Zheng. Home page: https://junruoyu-zheng.gitee.io/ligral

   Distributed under MIT license.
   See file LICENSE for detail or copy at https://opensource.org/licenses/MIT
*/

using System.Collections.Generic;
using System.Linq;
using MathNet.Numerics.LinearAlgebra;
using ParameterDictionary = System.Collections.Generic.Dictionary<string, Ligral.Component.Parameter>;
using Ligral.Simulation;
using Ligral.Syntax.CodeASTs;

namespace Ligral.Component.Models
{
    class InputMarker : Model, IFixable
    {
        protected override string DocString
        {
            get
            {
                return "This model marks the input signal as control input.";
            }
        }
        private string varName;
        private int rowNo = 0;
        private int colNo = 0;
        private ControlInputHandle handle;
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
                    if (colNo < 1)
                    {
                        throw logger.Error(new ModelException(this, $"column number should be at least 1 but {colNo} received"));
                    }
                    InPortList[0].ColNo = colNo;
                }, ()=>{})},
                {"row", new Parameter(ParameterType.Signal , value=>
                {
                    rowNo = value.ToInt();
                    if (rowNo < 1)
                    {
                        throw logger.Error(new ModelException(this, $"row number should be at least 1 but {rowNo} received"));
                    }
                    InPortList[0].RowNo = rowNo;
                }, ()=>{})}
            };
        }
        public bool FixConnection()
        {
            ILinkable constant = ModelManager.Create("Constant", ModelToken);
            var dict = new Dictionary<string, object>();
            if (rowNo > 0 && colNo > 0)
            {
                MatrixBuilder<double> m = Matrix<double>.Build;
                Matrix<double> matrix = m.Dense(rowNo, colNo, 0);
                dict.Add("value", matrix);
            }
            else
            {
                dict.Add("value", 0);
            }
            constant.Configure(dict);
            constant.Connect(this);
            return true;
        }
        public override void Prepare()
        {
            string inputSignalName = InPortList[0].Source is null ? null : InPortList[0].Source.SignalName;
            varName = varName ?? GivenName;
            if (varName != null && Scope != null)
            {
                varName = Scope +"." + varName;
            }
            else if (varName == null && inputSignalName!= null) 
            {
                varName = inputSignalName;
            }
            else if (varName == null)
            {
                varName = Name;
                if (Scope != null) varName = Scope+"."+varName;
            }
        }
        public override void Check()
        {
            int inputRowNo = InPortList[0].RowNo;
            int inputColNo = InPortList[0].ColNo;
            if (inputRowNo <= 0 && inputColNo <= 0)
            {
                if (rowNo <= 0 && colNo <= 0)
                {
                    rowNo = 1;
                    colNo = 1;
                }
                else
                {
                    InPortList[0].RowNo = rowNo;
                    InPortList[0].ColNo = colNo;
                }
            }
            else if (rowNo <= 0 && colNo <= 0)
            {
                rowNo = inputRowNo;
                colNo = inputColNo;
            }
            else if (inputColNo != colNo)
            {
                throw logger.Error(new ModelException(this, $"Column number inconsistent, got {inputColNo}, but {colNo} expected."));
            }
            else if (inputRowNo != rowNo)
            {
                throw logger.Error(new ModelException(this, $"Row number inconsistent, got {inputRowNo}, but {rowNo} expected."));
            }
            handle = ControlInput.CreateInput(varName??Name, rowNo, colNo);
            OutPortList[0].SetShape(rowNo, colNo);
        }
        protected override List<Matrix<double>> Calculate(List<Matrix<double>> values)
        {
            try
            {
                handle.SetClosedLoopInput(values[0]);
            }
            catch (LigralException)
            {
                throw logger.Error(new ModelException(this));
            }
            Results[0] = handle.GetInput();
            return Results;
        }
        public static List<CodeAST> ConstructConfigurationAST(string GlobalName, ControlInputHandle handle)
        {
            var codeASTs = new List<CodeAST>();
            AssignCodeAST ctxAST = new AssignCodeAST();
            ctxAST.Destination = $"{GlobalName}.ctx";
            ctxAST.Source = "&ctx";
            codeASTs.Add(ctxAST);
            AssignCodeAST indexAST = new AssignCodeAST();
            indexAST.Destination = $"{GlobalName}.index";
            indexAST.Source = ControlInput.InputPool.IndexOf(handle.space[0]).ToString();
            codeASTs.Add(indexAST);
            return codeASTs;
        }
        public override List<CodeAST> ConstructConfigurationAST()
        {
            return ConstructConfigurationAST(GlobalName, handle);
        }
    }
}