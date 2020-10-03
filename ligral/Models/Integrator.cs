using System.Collections.Generic;
using ParameterDictionary = System.Collections.Generic.Dictionary<string, Ligral.Parameter>;
using System;
using MathNet.Numerics.LinearAlgebra;

namespace Ligral.Models
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
                state.Config(1e-5, 10);
                state.DerivativeReceived += s=>{};
                states.Add(state);
            }
        }
        // protected override void AfterConfigured()
        // {
        //     state = State.CreateState(initial);
        //     state.Config(1e-5, 10);
        //     state.DerivativeReceived += (state) => { };
        // }
        protected override List<Signal> Calculate(List<Signal> values)
        {
            // Results[0] += values[0]*(time-lastTime);
            // lastTime = time;
            Signal inputSignal = values[0];
            Signal outputSignal = Results[0];
            if (isMatrix && inputSignal.IsMatrix)
            {
                inputSignal.ZipApply<State, int>(states, (deriv, state) => {
                    StateCalculate(state, deriv);
                    return 0;
                });
                MatrixBuilder<double> m = Matrix<double>.Build;
                Matrix<double> matrix = m.Dense(colNo, rowNo, states.ConvertAll(state => state.StateVariable).ToArray()).Transpose();
                outputSignal.Pack(matrix);
            }
            else if (!isMatrix && !inputSignal.IsMatrix)
            {
                State state = states[0];
                StateCalculate(state, (double) inputSignal.Unpack());
                outputSignal.Pack(state.StateVariable);
            }
            else
            {
                throw new ModelException(this, "Type conflict");
            }
            return Results;
        }

        protected virtual void StateCalculate(State state, double deriv)
        {
            state.SetDerivative(deriv, time);
            state.EulerPropagate();
        }
    }
}