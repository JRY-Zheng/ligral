/* Copyright (C) 2019-2022 Junruoyu Zheng. Home page: https://junruoyu-zheng.gitee.io/ligral

   Distributed under MIT license.
   See file LICENSE for detail or copy at https://opensource.org/licenses/MIT
*/

using System.Collections.Generic;
using ParameterDictionary = System.Collections.Generic.Dictionary<string, Ligral.Component.Parameter>;
using Ligral.Simulation;
using MathNet.Numerics.LinearAlgebra;
using Ligral.Syntax.CodeASTs;

namespace Ligral.Component.Models
{
    class OutputSink : Model
    {
        protected override string DocString
        {
            get
            {
                return "This model marks thee input signal as control output.";
            }
        }
        private string varName;
        private int rowNo = 0;
        private int colNo = 0;
        private ObservationHandle handle;
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
                }, ()=>{})}
            };
        }
        public override void Prepare()
        {
            varName = Model.GetVarName(varName, this);
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
            handle = Observation.CreateObservation(varName, rowNo, colNo);
        }
        protected override List<Matrix<double>> Calculate(List<Matrix<double>> values)
        {
            Matrix<double> inputSignal = values[0];
            handle.Cache(inputSignal);
            return Results;
        }
        
        public static List<CodeAST> ConstructConfigurationAST(string GlobalName, ObservationHandle handle)
        {
            var codeASTs = new List<CodeAST>();
            AssignCodeAST ctxAST = new AssignCodeAST();
            ctxAST.Destination = $"{GlobalName}.ctx";
            ctxAST.Source = "&ctx";
            codeASTs.Add(ctxAST);
            AssignCodeAST indexAST = new AssignCodeAST();
            indexAST.Destination = $"{GlobalName}.index";
            indexAST.Source = Observation.ObservationPool.IndexOf(handle.space[0]).ToString();
            codeASTs.Add(indexAST);
            return codeASTs;
        }
        public override List<CodeAST> ConstructConfigurationAST()
        {
            return ConstructConfigurationAST(GlobalName, handle);
        }
    }
}