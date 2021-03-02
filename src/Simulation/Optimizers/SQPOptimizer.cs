/* Copyright (C) 2019-2021 Junruoyu Zheng. Home page: https://junruoyu-zheng.gitee.io/ligral

   Distributed under MIT license.
   See file LICENSE for detail or copy at https://opensource.org/licenses/MIT
*/

using System;
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
        private bool isQuadraticCost = false;
        public override Matrix<double> Optimize(Func<Matrix<double>> expect, Matrix<double> x0, MFunc equal, MFunc bound)
        {
            isQuadraticCost = true;
            return Optimize(x => 
            {
                var dx = x - expect();
                return dx.Transpose() * dx;
            }, x0, equal, bound);
        }
        public override Matrix<double> Optimize(MFunc cost, Matrix<double> x0, MFunc equal, MFunc bound)
        {
            int n = x0.RowCount;
            if (x0.ColumnCount != 1)
            {
                throw logger.Error(new LigralException($"Only x with shape nx1 is supported, but we got {n}x{x0.ColumnCount}"));
            }
            var x = x0;
            var c = cost(x);
            Matrix<double> H = null;
            Matrix<double> C = null;
            if (isQuadraticCost)
            {
                H = Matrix<double>.Build.DiagonalIdentity(x0.RowCount) * 2;
            }
            for (int i = 0; i < MaximumIteration; i++)
            {
                if (isQuadraticCost)
                {
                    C = x * 2;
                }
                else
                {
                    H = Algorithm.RoughHessian(cost, x);
                    C = Algorithm.RoughPartial(cost, x);
                }
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
                logger.Debug(SignalUtils.SPrint(Ae, "Ae"));
                logger.Debug(SignalUtils.SPrint(Be, "Be"));
                var Ab = Algorithm.RoughPartial(bound, x);
                var Bb = bound(x);
                if (Ab != null)
                {
                    Ab = Ab.Transpose();
                    int bRowCount = Bb.RowCount;
                    for (int j = 0, k = 0; j < bRowCount; j++)
                    {
                        if (Bb[j-k, 0] < BoundaryConstrainTolerant)
                        {
                            Ab = Ab.RemoveRow(j-k);
                            Bb = Bb.RemoveRow(j-k);
                            k++;
                        }
                    }
                }
                logger.Debug(SignalUtils.SPrint(Ab, "Ab"));
                logger.Debug(SignalUtils.SPrint(Bb, "Bb"));
                (var A, var B) = Filter(Ab, Bb, Ae, Be);
                logger.Debug(SignalUtils.SPrint(A, "A"));
                logger.Debug(SignalUtils.SPrint(B, "B"));
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
                logger.Debug(SignalUtils.SPrint(K, "K"));
                var p = SignalUtils.Stack(-C, -B);
                logger.Debug(SignalUtils.SPrint(p, "p"));
                var r = K.Solve(p);
                var s = r.SubMatrix(0, n, 0, 1);
                logger.Debug(SignalUtils.SPrint(s, "s"));
                var lambda = r.SubMatrix(n, mp, 0, 1);
                logger.Debug(SignalUtils.SPrint(lambda, "lambda"));
                x += s;
                logger.Debug(SignalUtils.SPrint(x, "x"));
                var cp = cost(x);
                if (lambda.RowAbsoluteSums().ForAll(sum => sum < OptimizationStopLambdaTolerant))
                {
                    logger.Info("SQP Optimizer stops as lambda is close to zero");
                    break;
                }
                else if ((cp-c).RowAbsoluteSums().ForAll(sum => sum < OptimizationStopCostTolerant))
                {
                    logger.Info($"SQP Optimizer stops as costs did not change: from {c} to {cp}.");
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
                else if (SignalUtils.Append(At, Bt).Rank() != At.RowCount)
                {
                    logger.Debug($"Same constrains detected at NO.{i}, which will be ignored.");
                }
                else
                {
                    logger.Debug(SignalUtils.SPrint(At, "At"));
                    logger.Debug(SignalUtils.SPrint(Bt, "Bt"));
                    throw logger.Error(new LigralException($"Conflict constrains detected at NO.{i}."));
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
                    logger.Debug($"Conflict constrains detected at NO.{i}, which are resulted from bound constrains and will be ignore.");
                }
            }
            return (Ap, Bp);
        }
    }
}