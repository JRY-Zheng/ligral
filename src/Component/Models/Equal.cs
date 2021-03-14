/* Copyright (C) 2019-2021 Junruoyu Zheng. Home page: https://junruoyu-zheng.gitee.io/ligral

   Distributed under MIT license.
   See file LICENSE for detail or copy at https://opensource.org/licenses/MIT
*/

using System.Collections.Generic;
using MathNet.Numerics.LinearAlgebra;
using Ligral.Simulation;

namespace Ligral.Component.Models
{
    class Equal : InitializeableModel
    {
        protected override string DocString
        {
            get
            {
                return "This model marks an algebraic loop to be solved.";
            }
        }
        private string varName;
        protected SolutionHandle solutionHandle;
        protected FunctionHandle functionHandle;
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
            InPort inPort = InPortList[0];
            inPort.InPortValueReceived += ActualValueUpdate;
        }
        public override void Check()
        {
            base.Check();
            solutionHandle = Solution.CreateSolution(varName, rowNo, colNo, initial);
            functionHandle = Function.CreateFunction(varName, rowNo, colNo);
        }
        private void ActualValueUpdate(Matrix<double> inputSignal)
        {
            try
            {
                var x = solutionHandle.GetGuessedValue();
                functionHandle.SetActualValue(inputSignal-x);
            }
            catch (LigralException)
            {
                throw logger.Error(new ModelException(this));
            }
        }
        protected override List<Matrix<double>> Calculate(List<Matrix<double>> values)
        {
            Matrix<double> outputSignal = Results[0];
            Results[0] = solutionHandle.GetGuessedValue();
            return Results;
        }
    }
}