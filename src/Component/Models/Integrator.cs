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
        // private double lastTime = 0;
        protected List<State> states = new List<State>();
        protected override void AfterConfigured()
        {
            base.AfterConfigured();
            Results[0].Clone(initial);
            foreach (double initialValue in initial.ToList())
            {
                State state = State.CreateState(initialValue, $"{Name}{states.Count+1}");
                states.Add(state);
            }
            InPort inPort = InPortList[0];
            inPort.InPortValueReceived += DerivativeUpdate;
        }
        protected virtual void DerivativeUpdate(Signal inputSignal)
        {
            if (isMatrix && inputSignal.IsMatrix)
            {
                inputSignal.ZipApply<State>(states, (deriv, state) => {
                    state.Derivative = deriv;
                });
            }
            else if (!isMatrix && !inputSignal.IsMatrix)
            {
                State state = states[0];
                state.Derivative = (double) inputSignal.Unpack();
            }
            else
            {
                throw logger.Error(new ModelException(this, "Type conflict. If a matrix is passed to the integrator, the size must be declared."));
            }
        }
        // protected override void AfterConfigured()
        // {
        //     state = State.CreateState(initial);
        //     state.Config(1e-5, 10);
        //     state.DerivativeReceived += (state) => { };
        // }
        protected override List<Signal> DefaultCalculate(List<Signal> values)
        {
            // Results[0] += values[0]*(time-lastTime);
            // lastTime = time;
            Signal inputSignal = values[0];
            Signal outputSignal = Results[0];
            if (isMatrix && inputSignal.IsMatrix)
            {
                // inputSignal.ZipApply<State>(states, (deriv, state) => {
                //     StateCalculate(state, deriv);
                // });
                MatrixBuilder<double> m = Matrix<double>.Build;
                Matrix<double> matrix = m.Dense(colNo, rowNo, states.ConvertAll(state => state.StateVariable).ToArray()).Transpose();
                outputSignal.Pack(matrix);
            }
            else if (!isMatrix && !inputSignal.IsMatrix)
            {
                State state = states[0];
                // StateCalculate(state, (double) inputSignal.Unpack());
                outputSignal.Pack(state.StateVariable);
            }
            else
            {
                throw logger.Error(new ModelException(this, "Type conflict"));
            }
            return Results;
        }

        // protected virtual void StateCalculate(State state, double deriv)
        // {
        //     // state.SetDerivative(deriv, time);
        //     state.EulerPropagate();
        // }
    }
}