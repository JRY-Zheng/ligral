using Ligral.Block;
using Ligral.Simulation;

namespace Ligral.Models
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
            Parameters["upper"] = new Parameter(value=>
                {
                    upper=(double)value;
                });
            Parameters["lower"] = new Parameter(value=>
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
                throw new ModelException(this, "Type conflict");
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