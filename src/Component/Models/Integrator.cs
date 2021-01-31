/* Copyright (C) 2019-2021 Junruoyu Zheng. Home page: https://junruoyu-zheng.gitee.io/ligral

   Distributed under MIT license.
   See file LICENSE for detail or copy at https://opensource.org/licenses/MIT
*/

using System.Collections.Generic;
using MathNet.Numerics.LinearAlgebra;
using Ligral.Component;
using Ligral.Simulation;

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
            Parameters["name"] = new Parameter(ParameterType.Signal , value=>
            {
                varName=(string)value;
            }, ()=>{});
        }
        protected override void AfterConfigured()
        {
            base.AfterConfigured();
            Results[0].Clone(initial);
            InPort inPort = InPortList[0];
            inPort.InPortValueReceived += DerivativeUpdate;
        }
        public override void Prepare()
        {
            string inputSignalName = InPortList[0].Source.SignalName;
            varName = varName ?? GivenName ?? inputSignalName ?? Name;
            handle = State.CreateState(varName, rowNo, colNo, initial);
        }
        protected virtual void DerivativeUpdate(Signal inputSignal)
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
        protected override List<Signal> DefaultCalculate(List<Signal> values)
        {
            Signal inputSignal = values[0];
            Signal outputSignal = Results[0];
            outputSignal.Clone(handle.GetState());
            return Results;
        }

        // protected virtual void StateCalculate(State state, double deriv)
        // {
        //     // state.SetDerivative(deriv, time);
        //     state.EulerPropagate();
        // }
    }
}