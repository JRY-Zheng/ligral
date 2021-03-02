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
        private static Logger logger = new Logger("MatrixUtils");
        public static Matrix<double> ToColumnVector(this List<double> list)
        {
            MatrixBuilder<double> m = Matrix<double>.Build;
            return m.DenseOfRowMajor(list.Count, 1, list);
        }
        public static string ToLigralFormat(this Matrix<double> matrix, string indent)
        {
            if (matrix.RowCount == 0 || matrix.ColumnCount == 0)
            {
                logger.Warn("Empty matrix is printed, which is not supported in ligral.");
                return "";
            }
            return string.Join(";\n"+indent, matrix.Transpose().ToColumnArrays().Select((row, index)=>
            {
                return string.Join(", ", row);
            }));
        }
    }
    public class Problem
    {
        private List<Model> routine;
        private Logger logger = new Logger("Problem");
        private Matrix<double> x0;
        private string optimizerName = "sqp";
        private Optimizer optimizer;
        private double algebraicErrorTolerant = 1e-5;
        public string Name;
        public Problem(string name, List<Model> routine)
        {
            Name = name;
            this.routine = routine;
            if (Solution.SolutionPool.Count > 0)
            {
                x0 = Solution.SolutionPool.ConvertAll(solution => solution.InitialValue).ToColumnVector();
                if (optimizer == null) optimizer = Optimizer.GetOptimizer(optimizerName);
            }
        }
        public Matrix<double> InitialValues()
        {
            return State.StatePool.ConvertAll(state => state.InitialValue).ToColumnVector();
        }
        private Matrix<double> Cost()
        {
            return x0;
        }
        private Matrix<double> Equal(Matrix<double> x)
        {
            var solutionArray = x.AsColumnMajorArray();
            for (int i = 0; i < solutionArray.Length; i++)
            {
                Solution.SolutionPool[i].GuessedValue = solutionArray[i];
            }
            foreach(Model node in routine)
            {
                node.Propagate();
            }
            return Function.FunctionPool.ConvertAll(function => function.Value).ToColumnVector();
        }
        private Matrix<double> Bound(Matrix<double> x)
        {
            var build = Matrix<double>.Build;
            return build.Dense(0, 1);
        }
        private void SolveAlgebraicLoops()
        {
            Matrix<double> x0p = Matrix<double>.Build.DenseOfMatrix(x0);
            x0 = optimizer.Optimize(Cost, x0p, Equal, Bound);
            var err = Equal(x0).L2Norm();
            if (err > algebraicErrorTolerant)
            {
                throw logger.Error(new LigralException($"Algebraic loop cannot be solved, with error norm {err}"));
            }
        }
        public Matrix<double> SystemDynamicFunction(Matrix<double> states)
        {
            var stateArray = states.AsColumnMajorArray();
            for (int i = 0; i < stateArray.Length; i++)
            {
                State.StatePool[i].StateVariable = stateArray[i];
            }
            if (Solution.SolutionPool.Count > 0)
            {
                SolveAlgebraicLoops();
            }
            else foreach(Model node in routine)
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