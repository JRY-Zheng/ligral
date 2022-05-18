/* Copyright (C) 2019-2022 Junruoyu Zheng. Home page: https://junruoyu-zheng.gitee.io/ligral

   Distributed under MIT license.
   See file LICENSE for detail or copy at https://opensource.org/licenses/MIT
*/

using System.Collections.Generic;
using Ligral.Component;
using System.Linq;
using MathNet.Numerics.LinearAlgebra;

namespace Ligral.Simulation
{
    class StateSetter : IConfigurable
    {
        private Logger logger = new Logger("StateSetter");
        public void Configure(Dictionary<string, object> dict)
        {
            foreach (string item in dict.Keys)
            {
                object val = dict[item];
                if (!State.StateHandles.ContainsKey(item))
                {
                    throw logger.Error(new SettingException(item, val, $"No state handle named {item}"));
                }
                StateHandle handle = State.StateHandles[item];
                try
                {
                    handle.SetStateVariable(val.ToMatrix());
                }
                catch (LigralException)
                {
                    throw logger.Error(new SettingException(item, val));
                }
                catch (System.InvalidCastException)
                {
                    throw logger.Error(new SettingException(item, val, $"Invalid type {val.GetType()} in state setter."));
                }
            }
        }
    }
    class InputSetter : IConfigurable
    {
        private Logger logger = new Logger("InputSetter");
        public void Configure(Dictionary<string, object> dict)
        {
            foreach (string item in dict.Keys)
            {
                object val = dict[item];
                if (!ControlInput.InputHandles.ContainsKey(item))
                {
                    throw logger.Error(new SettingException(item, val, $"No input handle named {item}"));
                }
                ControlInputHandle handle = ControlInput.InputHandles[item];
                try
                {
                    handle.SetOpenLoopInput(val.ToMatrix());
                }
                catch (LigralException)
                {
                    throw logger.Error(new SettingException(item, val));
                }
                catch (System.InvalidCastException)
                {
                    throw logger.Error(new SettingException(item, val, $"Invalid type {val.GetType()} in input setter."));
                }
            }
        }
    }
    class Linearizer : IConfigurable
    {
        private Logger logger = new Logger("Linearizer");
        private StateSetter stateSetter = new StateSetter();
        private InputSetter inputSetter = new InputSetter();
        public Matrix<double> A {get; private set;}
        public Matrix<double> B {get; private set;}
        public Matrix<double> C {get; private set;}
        public Matrix<double> D {get; private set;}
        private string modelName;
        public void Linearize(Problem problem)
        {
            modelName = problem.Name;
            var xStar = State.StatePool.ConvertAll(state => state.StateVariable).ToColumnVector();
            var uStar = ControlInput.InputPool.ConvertAll(input => input.Input).ToColumnVector();
            var dx = 0.1*xStar.PointwiseAbs()+1;
            var du = 0.1*uStar.PointwiseAbs()+1;
            var f0 = problem.SystemDynamicFunction(xStar, uStar);
            var g0 = problem.ObservationFunction();
            var build = Matrix<double>.Build;
            A = build.DenseOfColumnVectors(xStar.Column(0).Select((row, index) => 
            {
                return Algorithm.PrecisePartial(d => 
                {
                    return problem.SystemDynamicFunction(xStar+Mask(xStar.RowCount, index, d), uStar) - f0;
                }, dx[index, 0]);
            }).Select((mat, i)=>mat.Column(0)));
            B = build.DenseOfColumnVectors(uStar.Column(0).Select((row, index) => 
            {
                return Algorithm.PrecisePartial(d => 
                {
                    return problem.SystemDynamicFunction(xStar, uStar+Mask(uStar.RowCount, index, d)) - f0;
                }, du[index, 0]);
            }).Select((mat, i)=>mat.Column(0)));
            C = build.DenseOfColumnVectors(xStar.Column(0).Select((row, index) => 
            {
                return Algorithm.PrecisePartial(d => 
                {
                    problem.SystemDynamicFunction(xStar+Mask(xStar.RowCount, index, d), uStar);
                    return problem.ObservationFunction() - g0;
                }, dx[index, 0]);
            }).Select((mat, i)=>mat.Column(0)));
            D = build.DenseOfColumnVectors(uStar.Column(0).Select((row, index) => 
            {
                return Algorithm.PrecisePartial(d => 
                {
                    problem.SystemDynamicFunction(xStar, uStar+Mask(uStar.RowCount, index, d));
                    return problem.ObservationFunction() - g0;
                }, du[index, 0]);
            }).Select((mat, i)=>mat.Column(0)));
        }
        public override string ToString()
        {
            return $@"# Auto-generated by Ligral (c)

# The state space matrices of system {modelName} are

let A = [{A.ToLigralFormat("         ")}];
# where the states are {State.GetNames()}

let B = [{B.ToLigralFormat("         ")}];
# where the inputs are {ControlInput.GetNames()}

let C = [{C.ToLigralFormat("         ")}];
# where the outputs are {Observation.GetNames()}

let D = [{D.ToLigralFormat("         ")}];
";
        }
        public Matrix<double> Mask(int length, int index, double value)
        {
            var build = Matrix<double>.Build;
            var vec = build.Dense(length, 1, 0);
            vec[index, 0] = value;
            return vec;
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
                        stateSetter.Configure((Dictionary<string, object>) val);
                        break;
                    case "input":
                    case "u":
                        inputSetter.Configure((Dictionary<string, object>) val);
                        break;
                    case "time":
                    case "t":
                        Solver.Time = System.Convert.ToDouble(val);
                        break;
                    default:
                        throw logger.Error(new SettingException(item, val, "Unsupported setting in linearizer."));
                    }
                }
                catch (System.InvalidCastException)
                {
                    throw logger.Error(new SettingException(item, val, $"Invalid type {val.GetType()} in linearizer."));
                }
            }
        }
    }
}