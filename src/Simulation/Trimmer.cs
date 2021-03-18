/* Copyright (C) 2019-2021 Junruoyu Zheng. Home page: https://junruoyu-zheng.gitee.io/ligral

   Distributed under MIT license.
   See file LICENSE for detail or copy at https://opensource.org/licenses/MIT
*/

using System.Collections.Generic;
using System.Linq;
using MathNet.Numerics.LinearAlgebra;
using Ligral.Component;

namespace Ligral.Simulation
{
    class Trimmer : IConfigurable
    {
        private Logger logger = new Logger("Trimmer");
        private string modelName; 
        private string optimizerName = "sqp";
        private int n;
        private int m;
        private int p;
        private Matrix<double> x;
        private Matrix<double> u;
        private Matrix<double> x0;
        private Matrix<double> u0;
        private Matrix<double> y0;
        private Matrix<double> dx0;
        private Matrix<double> xUpper;
        private Matrix<double> uUpper;
        private Matrix<double> yUpper;
        private Matrix<double> dxUpper;
        private Matrix<double> xLower;
        private Matrix<double> uLower;
        private Matrix<double> yLower;
        private Matrix<double> dxLower;
        private Matrix<int> xConstrain;
        private Matrix<int> uConstrain;
        private Matrix<int> yConstrain;
        private Matrix<int> dxConstrain;
        private Problem problem;
        private Optimizer optimizer;
        public void GetCondition()
        {
            n = State.StatePool.Count;
            m = Observation.ObservationPool.Count;
            p = ControlInput.InputPool.Count;
            x0 = State.StatePool.ConvertAll(state => state.StateVariable).ToColumnVector();
            u0 = ControlInput.InputPool.ConvertAll(input => input.Input).ToColumnVector();
            y0 = Observation.ObservationPool.ConvertAll(output => output.OutputVariable).ToColumnVector();
            dx0 = State.StatePool.ConvertAll(state => state.Derivative).ToColumnVector();
            xUpper = State.StatePool.ConvertAll(state => state.StateUpperBound).ToColumnVector();
            uUpper = ControlInput.InputPool.ConvertAll(input => input.InputUpperBound).ToColumnVector();
            yUpper = Observation.ObservationPool.ConvertAll(output => output.OutputUpperBound).ToColumnVector();
            dxUpper = State.StatePool.ConvertAll(state => state.DerivativeUpperBound).ToColumnVector();
            xLower = State.StatePool.ConvertAll(state => state.StateLowerBound).ToColumnVector();
            uLower = ControlInput.InputPool.ConvertAll(input => input.InputLowerBound).ToColumnVector();
            yLower = Observation.ObservationPool.ConvertAll(output => output.OutputLowerBound).ToColumnVector();
            dxLower = State.StatePool.ConvertAll(state => state.DerivativeLowerBound).ToColumnVector();
            xConstrain = State.StatePool.ConvertAll(state => state.IsConstrained?1:0).ToColumnVector();
            uConstrain = ControlInput.InputPool.ConvertAll(input => input.IsConstrained?1:0).ToColumnVector();
            yConstrain = Observation.ObservationPool.ConvertAll(output => output.IsConstrained?1:0).ToColumnVector();
            dxConstrain = State.StatePool.ConvertAll(state => state.IsDerivativeConstrained?1:0).ToColumnVector();
        }
        private Matrix<double> Cost()
        {
            // var x = z.SubMatrix(0, n, 0, 1) - x0;
            // var u = z.SubMatrix(n, p, 0, 1) - u0;
            // return x.Transpose()*x+u.Transpose()*u;
            return SignalUtils.Stack(x0, u0);
        }
        private Matrix<double> Filter(Matrix<double> val, Matrix<double> goal, Matrix<int> constrain)
        {
            int num = constrain.Column(0).ToArray().Count(b => b==1);
            if (num == 0) return null;
            var build = Matrix<double>.Build;
            var matrix = build.Dense(num, 1);
            for (int i = 0, c = 0; c < constrain.RowCount; c++)
            {
                if (constrain[c, 0]==1)
                {
                    matrix[i, 0] = val[c, 0] - goal[c, 0];
                    i++;
                }
            }
            return matrix;
        }
        private Matrix<double> Equal(Matrix<double> z)
        {
            var x = z.SubMatrix(0, n, 0, 1);
            var u = z.SubMatrix(n, p, 0, 1);
            var dx = problem.SystemDynamicFunction(x, u);
            var y = problem.ObservationFunction();
            var hx = Filter(x, x0, xConstrain);
            var hu = Filter(u, u0, uConstrain);
            var hy = Filter(y, y0, yConstrain);
            var hdx = Filter(dx, dx0, dxConstrain);
            return SignalUtils.Stack(hx, hu, hy, hdx);
        }
        private Matrix<double> Bound(Matrix<double> z)
        {
            var x = z.SubMatrix(0, n, 0, 1);
            var u = z.SubMatrix(n, p, 0, 1);
            var dx = problem.SystemDynamicFunction(x, u);
            var y = problem.ObservationFunction();
            var gxUpper = Filter(x, xUpper, xUpper.Map<int>(bnd => !double.IsInfinity(bnd)?1:0));
            var guUpper = Filter(u, uUpper, uUpper.Map<int>(bnd => !double.IsInfinity(bnd)?1:0));
            var gyUpper = Filter(y, yUpper, yUpper.Map<int>(bnd => !double.IsInfinity(bnd)?1:0));
            var gdxUpper = Filter(dx, dxUpper, dxUpper.Map<int>(bnd => !double.IsInfinity(bnd)?1:0));
            var gxLower = Filter(xLower, x, xLower.Map<int>(bnd => !double.IsInfinity(bnd)?1:0));
            var guLower = Filter(uLower, u, uLower.Map<int>(bnd => !double.IsInfinity(bnd)?1:0));
            var gyLower = Filter(yLower, y, yLower.Map<int>(bnd => !double.IsInfinity(bnd)?1:0));
            var gdxLower = Filter(dxLower, dx, dxLower.Map<int>(bnd => !double.IsInfinity(bnd)?1:0));
            return SignalUtils.Stack(gxUpper, guUpper, gyUpper, gdxUpper, gxLower, guLower, gyLower, gdxLower);
        }
        public void Trim(Problem problem)
        {
            modelName = problem.Name;
            this.problem = problem;
            if (optimizer == null) optimizer = Optimizer.GetOptimizer(optimizerName);
            var z = optimizer.Optimize(Cost, SignalUtils.Stack(x0, u0), Equal, Bound);
            x = z.SubMatrix(0, n, 0, 1);
            u = z.SubMatrix(n, p, 0, 1);
        }
        public override string ToString()
        {
            return $@"# Auto-generated by Ligral (c)

# The trim point of system {modelName} is

let x0 = [{x.ToLigralFormat("         ")}];
# where the states are {State.GetNames()}

let u0 = [{u.ToLigralFormat("         ")}];
# where the inputs are {ControlInput.GetNames()}
";
        }
        public void Configure(Dictionary<string, object> dict)
        {
            foreach (string item in dict.Keys)
            {
                object val = dict[item];
                try
                {
                    switch (item.ToLower())
                    {
                    case "state":
                    case "x":
                        ConditionsSetter<StateCondition> stateSetter = new ConditionsSetter<StateCondition>();
                        stateSetter.Configure((Dictionary<string, object>) val);
                        break;
                    case "state_derivative":
                    case "derivative":
                    case "dx":
                        ConditionsSetter<StateDerivativeCondition> derivativeSetter = new ConditionsSetter<StateDerivativeCondition>();
                        derivativeSetter.Configure((Dictionary<string, object>) val);
                        break;
                    case "input":
                    case "u":
                        ConditionsSetter<InputCondition> inputSetter = new ConditionsSetter<InputCondition>();
                        inputSetter.Configure((Dictionary<string, object>) val);
                        break;
                    case "output":
                    case "y":
                        ConditionsSetter<OutputCondition> outputSetter = new ConditionsSetter<OutputCondition>();
                        outputSetter.Configure((Dictionary<string, object>) val);
                        break;
                    case "time":
                    case "t":
                        Solver.Time = System.Convert.ToDouble(val);
                        break;
                    case "optimizer":
                    case "opt":
                        optimizerName = (string) val; break;
                    default:
                        throw logger.Error(new SettingException(item, val, "Unsupported setting in trimmer."));
                    }
                }
                catch (LigralException)
                {
                    throw logger.Error(new SettingException(item, val));
                }
                catch (System.InvalidCastException)
                {
                    throw logger.Error(new SettingException(item, val, $"Invalid type {val.GetType()} in trimmer."));
                }
            }
        }
    }
}