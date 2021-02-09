/* Copyright (C) 2019-2021 Junruoyu Zheng. Home page: https://junruoyu-zheng.gitee.io/ligral

   Distributed under MIT license.
   See file LICENSE for detail or copy at https://opensource.org/licenses/MIT
*/

using MathNet.Numerics.LinearAlgebra;
using Ligral.Component;
using MFunc = System.Func<MathNet.Numerics.LinearAlgebra.Matrix<double>, MathNet.Numerics.LinearAlgebra.Matrix<double>>;

namespace Ligral.Simulation.Optimizers
{
    public class SQPOptimizer : Optimizer
    {
        public int MaximumIteration {get; set;} = 1000;
        public double BoundaryConstrainTolerant {get; set;} = 1e-5; 
        public double OptimizationStopLambdaTolerant {get; set;} = 1e-3;
        public double OptimizationStopCostTolerant {get; set;} = 1e-8;
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
                var Ae = Algorithm.RoughPartial(equal, x);
                var Be = equal(x);
                if (Ae != null)
                {
                    Ae = Ae.Transpose();
                    (Ae, Be) = Filter(Ae, Be);
                    if (Be.ColumnCount != 1)
                    {
                        throw logger.Error(new LigralException($"Only h(x) with shape mx1 is supported, but we got {Be.RowCount}x{Be.ColumnCount}"));
                    }
                }
                if (Ae != null) logger.Debug($"The Ae matrix is {new Signal(Ae).ToStringInLine()}");
                else logger.Debug("The Ae matrix is null");
                if (Be != null) logger.Debug($"The Be matrix is {new Signal(Be).ToStringInLine()}");
                else logger.Debug("The Be matrix is null");
                var Ab = Algorithm.RoughPartial(bound, x);
                var Bb = bound(x);
                if (Ab != null)
                {
                    Ab = Ab.Transpose();
                    for (int j = 0, k = 0; j < Bb.RowCount; j++)
                    {
                        if (Bb[j-k, 0] < BoundaryConstrainTolerant)
                        {
                            Ab = Ab.RemoveRow(j-k);
                            Bb = Bb.RemoveRow(j-k);
                            k++;
                        }
                    }
                }
                if (Ab != null) logger.Debug($"The Ab matrix is {new Signal(Ab).ToStringInLine()}");
                else logger.Debug("The Ab matrix is null");
                if (Bb != null) logger.Debug($"The Bb matrix is {new Signal(Bb).ToStringInLine()}");
                else logger.Debug("The Bb matrix is null");
                (var A, var B) = Filter(Ab, Bb, Ae, Be);
                if (A != null) logger.Debug($"The A matrix is {new Signal(A).ToStringInLine()}");
                else logger.Debug("The A matrix is null");
                if (B != null) logger.Debug($"The B matrix is {new Signal(B).ToStringInLine()}");
                else logger.Debug("The B vector is null");
                if (B != null && B.ColumnCount != 1)
                {
                    throw logger.Error(new LigralException($"Only g(x) with shape px1 is supported, but we got {B.RowCount-Be.RowCount}x{B.ColumnCount}"));
                }
                var build = Matrix<double>.Build;
                int mp = B==null?0:B.RowCount;
                var K = SignalUtils.Stack(
                    SignalUtils.Append(H, A==null?A:A.Transpose()),
                    SignalUtils.Append(A, mp==0?null:build.Dense(mp, mp, 0))
                );
                if (K == null)
                {
                    logger.Warn("No constrain is given, optimizer quit");
                    return x;
                }
                logger.Debug($"The K matrix is {new Signal(K).ToStringInLine()}");
                var p = K.Solve(SignalUtils.Stack(-C, B));
                var s = p.SubMatrix(0, n, 0, 1);
                logger.Debug($"The s vector is {new Signal(s).ToStringInLine()}");
                var lambda = p.SubMatrix(n, mp, 0, 1);
                if (lambda != null) logger.Debug($"The lambda is {new Signal(lambda).ToStringInLine()}");
                else logger.Debug($"The lambda is null");
                x += s;
                var cp = cost(x);
                if (lambda.RowAbsoluteSums().ForAll(sum => sum < OptimizationStopLambdaTolerant))
                {
                    logger.Info("SQP Optimizer stops as lambda is close to zero");
                    break;
                }
                else if ((cp-c).RowAbsoluteSums().ForAll(sum => sum < OptimizationStopCostTolerant))
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
            for (int i = 1; i < A.RowCount; i++)
            {
                var At = SignalUtils.Stack(Ap, A.SubMatrix(i, 1, 0, A.ColumnCount));
                var Bt = SignalUtils.Stack(Bp, B.SubMatrix(i, 1, 0, B.ColumnCount));
                if (At.Rank() == At.RowCount)
                {
                    Ap = At; 
                    Bp = Bt;
                }
                else if (SignalUtils.Append(At, Bt).Rank() == At.RowCount)
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
            if (A == null || B == null) return (A0, B0);
            if (A0 == null || B0 == null) return Filter(A, B);
            var Ap = A0;
            var Bp = B0;
            if (Ap.ColumnCount != A.ColumnCount)
            {
                throw logger.Error(new LigralException($"Constrain dimensions do not agree in SQP(A) {Ap.ColumnCount}: {A.ColumnCount}"));
            }
            if (Bp.ColumnCount != B.ColumnCount)
            {
                throw logger.Error(new LigralException($"Constrain dimensions do not agree in SQP(B) {Bp.ColumnCount}: {B.ColumnCount}"));
            }
            for (int i = 0; i < A.RowCount; i++)
            {
                var At = SignalUtils.Stack(Ap, A.SubMatrix(i, 1, 0, A.ColumnCount));
                var Bt = SignalUtils.Stack(Bp, B.SubMatrix(i, 1, 0, B.ColumnCount));
                if (At.Rank() == At.RowCount)
                {
                    Ap = At; 
                    Bp = Bt;
                }
                else if (SignalUtils.Append(At, Bt).Rank() == At.RowCount)
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