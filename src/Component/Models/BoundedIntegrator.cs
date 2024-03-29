/* Copyright (C) 2019-2022 Junruoyu Zheng. Home page: https://junruoyu-zheng.gitee.io/ligral

   Distributed under MIT license.
   See file LICENSE for detail or copy at https://opensource.org/licenses/MIT
*/

using Ligral.Simulation;
using MathNet.Numerics.LinearAlgebra;
using Ligral.Syntax.CodeASTs;
using System.Collections.Generic;

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
                    upper=value.ToScalar();
                });
            Parameters["lower"] = new Parameter(ParameterType.Signal , value=>
                {
                    lower = value.ToScalar();
                });
        }
        protected override void AfterConfigured()
        {
            base.AfterConfigured();
            if (lower>=upper)
            {
                throw logger.Error(new ModelException(this, $"Configuration conflict: lower bound {lower} is greater than upper bound {upper}."));
            }
        }
        protected override void InputUpdate(Matrix<double> inputSignal)
        {
            handle.SetDerivative(inputSignal.Map2(GetBoundedDerivative, handle.GetState()));
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
        public override List<CodeAST> ConstructConfigurationAST()
        {
            var codeASTs = Integrator.ConstructConfigurationAST(GlobalName, handle, initial);
            AssignCodeAST upperAST = new AssignCodeAST();
            upperAST.Destination = $"{GlobalName}.upper";
            upperAST.Source = upper.ToString();
            codeASTs.Add(upperAST);
            AssignCodeAST lowerAST = new AssignCodeAST();
            lowerAST.Destination = $"{GlobalName}.lower";
            lowerAST.Source = lower.ToString();
            codeASTs.Add(lowerAST);
            return codeASTs;
        }
    }
}