/* Copyright (C) 2019-2021 Junruoyu Zheng. Home page: https://junruoyu-zheng.gitee.io/ligral

   Distributed under MIT license.
   See file LICENSE for detail or copy at https://opensource.org/licenses/MIT
*/

using Ligral.Component;
using Ligral.Simulation;

namespace Ligral.Component.Models
{
    class BoundedIntegrator : Integrator
    {
        protected override string DocString
        {
            get
            {
                return "This model outputs the value of the integral of its input signal with respect to time, which is limited by bounds.";
            }
        }
        private double upper;
        private double lower;
        protected override void SetUpParameters()
        {
            base.SetUpParameters();
            Parameters["upper"] = new Parameter(ParameterType.Signal , value=>
                {
                    upper=(double)value;
                });
            Parameters["lower"] = new Parameter(ParameterType.Signal , value=>
                {
                    lower = (double)value;
                });
        }
        protected override void DerivativeUpdate(Signal inputSignal)
        {
            if (isMatrix && inputSignal.IsMatrix)
            {
                inputSignal.ZipApply<State>(states, (deriv, state) => {
                    state.Derivative = GetBoundedDerivative(state, deriv);
                });
            }
            else if (!isMatrix && !inputSignal.IsMatrix)
            {
                State state = states[0];
                state.Derivative = GetBoundedDerivative(state, (double) inputSignal.Unpack());
            }
            else
            {
                throw logger.Error(new ModelException(this, "Type conflict"));
            }
        }
        private double GetBoundedDerivative(State state, double deriv)
        {
            if ((state.StateVariable < upper && state.StateVariable > lower) ||
                (state.StateVariable >= upper && deriv < 0) ||
                (state.StateVariable <= lower && deriv > 0))
            {
                return deriv;
            }
            else
            {
                return 0;
            }
        }
        // protected override List<Signal> Calculate(List<Signal> values)
        // {
        //     if ((Results[0] < upper && Results[0] > lower) ||
        //         (Results[0] >= upper && values[0] < 0) ||
        //         (Results[0] <= lower && values[0] > 0))
        //     {
        //         Results[0] += values[0] * (time - lastTime);
        //     }
        //     lastTime = time;
        //     return Results;
        // }
    }
}