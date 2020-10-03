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
        protected override void StateCalculate(State state, double deriv)
        {
            if ((state.StateVariable < upper && state.StateVariable > lower) ||
                (state.StateVariable >= upper && deriv < 0) ||
                (state.StateVariable <= lower && deriv > 0))
            {
                base.StateCalculate(state, deriv);
            }
            else
            {
                base.StateCalculate(state, 0);
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