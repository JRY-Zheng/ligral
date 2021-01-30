/* Copyright (C) 2019-2021 Junruoyu Zheng. Home page: https://junruoyu-zheng.gitee.io/ligral

   Distributed under MIT license.
   See file LICENSE for detail or copy at https://opensource.org/licenses/MIT
*/

using System.Collections.Generic;
using Ligral.Component;
using System.Linq;
using MathNet.Numerics.LinearAlgebra;

namespace Ligral.Simulation
{
    static class MatrixUtils
    {
        public static Matrix<double> ToColumnVector(this List<double> list)
        {
            MatrixBuilder<double> m = Matrix<double>.Build;
            return m.DenseOfRowMajor(list.Count, 1, list);
        }
    }
    public class Problem
    {
        private List<Model> routine;
        private Logger logger = new Logger("Problem");
        public Problem(List<Model> routine)
        {
            this.routine = routine;
        }
        public Matrix<double> InitialValues()
        {
            return State.StatePool.ConvertAll(state => state.InitialValue).ToColumnVector();
        }
        public Matrix<double> SystemDynamicFunction(Matrix<double> states)
        {
            var stateArray = states.AsColumnMajorArray();
            for (int i = 0; i < stateArray.Length; i++)
            {
                State.StatePool[i].StateVariable = stateArray[i];
            }
            foreach(Model node in routine)
            {
                node.Propagate();
            }
            return State.StatePool.ConvertAll(state => state.Derivative).ToColumnVector();
        }
        public Matrix<double> SystemDynamicFunction(Matrix<double> states, Matrix<double> inputs)
        {
            if (!ControlInput.IsOpenLoop)
            {
                throw logger.Error(new LigralException("Only in open-loop mode can you define input"));
            }
            var inputArray = inputs.AsColumnMajorArray();
            if (inputArray.Length != ControlInput.InputPool.Count)
            {
                throw logger.Error(new LigralException($"Control input number unmatched, {inputArray.Length} got but {ControlInput.InputPool.Count} expected."));
            }
            for (int i = 0; i < inputArray.Length; i++)
            {
                ControlInput.InputPool[i].OpenLoopInput = inputArray[i];
            }
            return SystemDynamicFunction(states);
        }
        public Matrix<double> ObservationFunction()
        {
            Observation.ObservationPool.ForEach(observation => observation.Commit());
            Observation.TimeList.Add(Solver.Time);
            return Observation.ObservationPool.ConvertAll(observation => observation.OutputVariable).ToColumnVector();
        }
    }
}