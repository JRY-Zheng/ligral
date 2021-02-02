/* Copyright (C) 2019-2021 Junruoyu Zheng. Home page: https://junruoyu-zheng.gitee.io/ligral

   Distributed under MIT license.
   See file LICENSE for detail or copy at https://opensource.org/licenses/MIT
*/

using MathNet.Numerics.LinearAlgebra;
using MFunc = System.Func<MathNet.Numerics.LinearAlgebra.Matrix<double>, MathNet.Numerics.LinearAlgebra.Matrix<double>>;

namespace Ligral.Simulation.Optimizers
{
    public abstract class SQPOptimizer : Optimizer
    {
        public int MaximumIteration {get; set;} = 1000;
        public double BoundaryConstrainTolerant {get; set;} = 1e-5; 
        public double OptimizationStopTolerant {get; set;} = 1e-3;
        public override Matrix<double> Optimize(MFunc cost, Matrix<double> x0, MFunc equal, MFunc bound)
        {
            int n = x0.RowCount;
            if (x0.ColumnCount != 1)
            {
                throw logger.Error(new LigralException($"Only x with shape nx1 is supported, but we got {n}x{x0.ColumnCount}"));
            }
            var x = x0;
            var c = cost(x);
            for (int i = 0; i < MaximumIteration; i++)
            {
                var H = Algorithm.RoughHessian(cost, x);
                var C = Algorithm.RoughPartial(cost, x);
                var Ae = Algorithm.RoughPartial(equal, x).Transpose();
                var Be = equal(x);
                (Ae, Be) = Filter(Ae, Be);
                if (Be.ColumnCount != 1)
                {
                    throw logger.Error(new LigralException($"Only h(x) with shape mx1 is supported, but we got {Be.RowCount}x{Be.ColumnCount}"));
                }
                var Ab = Algorithm.RoughPartial(bound, x).Transpose();
                var Bb = bound(x);
                for (int j = 0, k = 0; j < Bb.RowCount; j++)
                {
                    if (Bb[j-k, 0] < BoundaryConstrainTolerant)
                    {
                        Ab = Ab.RemoveRow(j-k);
                        Bb = Bb.RemoveRow(j-k);
                        k++;
                    }
                }
                (var A, var B) = Filter(Ab, Ab, Ae, Be);
                if (B.ColumnCount != 1)
                {
                    throw logger.Error(new LigralException($"Only g(x) with shape px1 is supported, but we got {B.RowCount-Be.RowCount}x{B.ColumnCount}"));
                }
                var build = Matrix<double>.Build;
                int mp = B.RowCount;
                var K = H.Append(A.Transpose()).Stack(A.Append(build.Dense(mp, mp, 0)));
                var p = K.Solve(-C.Append(B));
                var s = p.SubMatrix(0, n, 0, 1);
                var lambda = p.SubMatrix(n, mp, 0, 1);
                x += s;
                var cp = cost(x);
                if ((lambda.RowAbsoluteSums()-OptimizationStopTolerant).ForAll(sum => sum < OptimizationStopTolerant))
                {
                    logger.Info("SQP Optimizer stops as lambda is close to zero");
                    break;
                }
                else if (((cp-c).RowAbsoluteSums()-OptimizationStopTolerant).ForAll(sum => sum < OptimizationStopTolerant))
                {
                    logger.Info("SQP Optimizer stops as costs did not change.");
                    break;
                }
                else
                {
                    c = cp;
                }
            }
            return x;
        }
        private (Matrix<double>, Matrix<double>) Filter(Matrix<double> A, Matrix<double> B)
        {
            var Ap = A.SubMatrix(0, 1, 0, A.ColumnCount);
            var Bp = B.SubMatrix(0, 1, 0, B.ColumnCount);
            for (int i = 1; i <= A.RowCount; i++)
            {
                var At = Ap.Stack(A.SubMatrix(i, 1, 0, A.ColumnCount));
                var Bt = Bp.Stack(B.SubMatrix(i, 1, 0, B.ColumnCount));
                if (At.Rank() == At.RowCount)
                {
                    Ap = At; 
                    Bp = Bt;
                }
                else if (At.Append(Bt).Rank() == At.RowCount)
                {
                    logger.Debug($"Same constrains detected at NO.{i}, which will be ignored.");
                }
                else
                {
                    logger.Debug($"Conflict constrains detected at NO.{i}, which are resulted from bound constrains and will be ignore.");
                }
            }
            return (Ap, Bp);
        }
        private (Matrix<double>, Matrix<double>) Filter(Matrix<double> A, Matrix<double> B, Matrix<double> A0, Matrix<double> B0)
        {
            var Ap = A0;
            var Bp = B0;
            for (int i = 1; i <= A.RowCount; i++)
            {
                var At = Ap.Stack(A.SubMatrix(i, 1, 0, A.ColumnCount));
                var Bt = Bp.Stack(B.SubMatrix(i, 1, 0, B.ColumnCount));
                if (At.Rank() == At.RowCount)
                {
                    Ap = At; 
                    Bp = Bt;
                }
                else if (At.Append(Bt).Rank() == At.RowCount)
                {
                    logger.Debug($"Same constrains detected at NO.{i}, which will be ignored.");
                }
                else
                {
                    throw logger.Error(new LigralException($"Conflict constrains detected at NO.{i}."));
                }
            }
            return (Ap, Bp);
        }
    }
}