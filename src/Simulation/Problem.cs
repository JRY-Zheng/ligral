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
        public Matrix<double> ObservationFunction()
        {
            return Observation.ObservationPool.ConvertAll(item => item.Item2.OutputVariable).ToColumnVector();
        }
    }
}