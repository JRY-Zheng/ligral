/* Copyright (C) 2019-2022 Junruoyu Zheng. Home page: https://junruoyu-zheng.gitee.io/ligral

   Distributed under MIT license.
   See file LICENSE for detail or copy at https://opensource.org/licenses/MIT
*/

using System.Collections.Generic;
using MathNet.Numerics.LinearAlgebra;
using Ligral.Component;
using Ligral.Simulation;
using Ligral.Syntax.CodeASTs;

namespace Ligral.Component.Models
{
    class Integrator : InitializeableModel
    {
        protected override string DocString
        {
            get
            {
                return "This model outputs the value of the integral of its input signal with respect to time.";
            }
        }
        private string varName;
        protected StateHandle handle;
        protected override void SetUpParameters()
        {
            base.SetUpParameters();
            Parameters["name"] = new Parameter(ParameterType.String , value=>
            {
                varName=(string)value;
            }, ()=>{});
        }
        protected override void AfterConfigured()
        {
            base.AfterConfigured();
            Results[0] = initial;
        }
        public override void Prepare()
        {
            string inputSignalName = InPortList[0].Source.SignalName;
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
        public override void Confirm()
        {
            handle = State.CreateState(varName, rowNo, colNo, initial);
        }
        protected override void InputUpdate(Matrix<double> inputSignal)
        {
            try
            {
                handle.SetDerivative(inputSignal);
            }
            catch (LigralException)
            {
                throw logger.Error(new ModelException(this));
            }
        }
        protected override List<Matrix<double>> Calculate(List<Matrix<double>> values)
        {
            Matrix<double> inputSignal = values[0];
            Matrix<double> outputSignal = Results[0];
            Results[0] = handle.GetState();
            return Results;
        }
        public static List<CodeAST> ConstructConfigurationAST(string GlobalName, StateHandle handle, Matrix<double> initial)
        {
            var codeASTs = new List<CodeAST>();
            AssignCodeAST ctxAST = new AssignCodeAST();
            ctxAST.Destination = $"{GlobalName}.ctx";
            ctxAST.Source = "&ctx";
            codeASTs.Add(ctxAST);
            LShiftCodeAST initialAST = new LShiftCodeAST();
            initialAST.Destination = $"{GlobalName}.initial";
            initialAST.Source = string.Join(',', initial.ToColumnMajorArray());
            codeASTs.Add(initialAST);
            AssignCodeAST indexAST = new AssignCodeAST();
            indexAST.Destination = $"{GlobalName}.index";
            indexAST.Source = State.StatePool.IndexOf(handle.space[0]).ToString();
            codeASTs.Add(indexAST);
            CallCodeAST configAST = new CallCodeAST();
            configAST.FunctionName = $"{GlobalName}.config";
            codeASTs.Add(configAST);
            return codeASTs;
        }
        public override List<CodeAST> ConstructConfigurationAST()
        {
            return ConstructConfigurationAST(GlobalName, handle, initial);
        }
    }
}