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
            handle.SetDerivative(inputSignal.ZipApply(handle.GetState(), GetBoundedDerivative));
        }
        private double GetBoundedDerivative(double deriv, double state)
        {
            if ((state < upper && state > lower) ||
                (state >= upper && deriv < 0) ||
                (state <= lower && deriv > 0))
            {
                return deriv;
            }
            else
            {
                return 0;
            }
        }
    }
}