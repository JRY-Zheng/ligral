/* Copyright (C) 2019-2021 Junruoyu Zheng. Home page: https://junruoyu-zheng.gitee.io/ligral

   Distributed under MIT license.
   See file LICENSE for detail or copy at https://opensource.org/licenses/MIT
*/

using System;
using System.Collections.Generic;
using Ligral.Component;
using System.Linq;
using MathNet.Numerics.LinearAlgebra;

namespace Ligral.Simulation
{
    static class Algorithm
    {
        private static Logger logger = new Logger("Algorithm");
        public static int PartialMaximumIteration = 100;
        public static double PartialDifferenceTolerant = 1e-3;
        public static double PartialBias = 1e-5;
        public static double AsymptoticDifferenceTolerant = 1e-7;
        public static Matrix<double> PrecisePartial(Func<double, Matrix<double>> f, double d0)
        {
            var vp = f(d0);
            var vn = f(-d0);
            var slopes = (vp-vn)/d0/2;
            double d = d0/2;
            for (int i=0; i < PartialMaximumIteration; i++)
            {
                vp = f(d);
                vn = f(-d);
                var slope = (vp-vn)/d/2;
                var difference = slope.Column(0) - slopes.Column(slopes.ColumnCount-1);
                if (difference.All(diff=>Math.Abs(diff)<PartialDifferenceTolerant))
                {
                    if (slopes.ColumnCount > 3) break;
                }
                slopes = slopes.Append(slope);
                d = d/2;
            }
            return slopes.ToRowArrays().Select((row, index)=>Asymptotic(row)).ToList().ToColumnVector();
        }
        public static Matrix<double> RoughPartial(Func<Matrix<double>, Matrix<double>> f, Matrix<double> x)
        {
            int n = x.RowCount;
            if (x.ColumnCount != 1)
            {
                throw logger.Error(new LigralException($"Only x with shape nx1 is supported, but we got {n}x{x.ColumnCount}"));
            }
            var fx = f(x);
            int m = x.RowCount;
            if (fx.ColumnCount != 1)
            {
                throw logger.Error(new LigralException($"Only f(x) with shape nx1 is supported, but we got {m}x{fx.ColumnCount}"));
            }
            var build = Matrix<double>.Build;
            var gradient = build.Dense(n, m, 1);
            for (int i = 0; i < n; i++)
            {
                var item = x[i, 0];
                x[i, 0] = item + PartialBias;
                gradient.SetRow(i, (f(x)-fx).Column(0)/PartialBias);
            }
            return gradient;
        }
        public static Matrix<double> RoughHessian(Func<Matrix<double>, Matrix<double>> f, Matrix<double> x)
        {
            return RoughPartial(x => RoughPartial(f, x), x);
        }
        public static double Asymptotic(double[] series)
        {
            if (series.Length <= 3)
            {
                throw logger.Error(new LigralException("At least 4 values are necessary for asymptotic point estimate."));
            }
            logger.Debug($"Check asymptotic point of list [{string.Join(", ", series.Skip(series.Count()-10))}]");
            double diff = series[series.Length-2] - series[series.Length-1];
            if (Math.Abs(diff)<AsymptoticDifferenceTolerant)
            {
                logger.Debug("Constant list detected, the last value of this list will be regarded as the asymptotic point.");
                return series[series.Length - 1];
            } 
            int mode = diff > 0 ? 1 : -1;
            List<double> tail = new List<double>();
            tail.Add(diff * mode);
            for (int i=series.Length-2; i>0; i--)
            {
                diff = series[i-1] - series[i];
                var val = diff * mode;
                if (val > tail.Last())
                {
                    tail.Insert(0, val);
                }
                else
                {
                    break;
                }
            }
            if (tail.Count <= 3)
            {
                throw logger.Error(new LigralException($"Asymptotic point estimate fails because the series is not convergent.\nThe last values are [{string.Join(", ", series.Skip(series.Count()-10))}]"));
            }
            var y = tail.ConvertAll(val => Math.Log(val));
            var yBar = y.Average();
            var xBar = (y.Count-1)/2.0;
            var xSqrSum = y.Count*xBar*(y.Count*2-1)/3;
            var xySum = y.Select((val, index) => val*index).Sum();
            logger.Debug($"bar(x) = {xBar}, bar(y) = {yBar}, sum(x*y) = {xySum}, sum(x^2) = {xSqrSum}");
            double kHat = (xySum-y.Count*xBar*yBar)/(xSqrSum-y.Count*xBar*xBar);
            if (kHat > 0)
            {
                throw logger.Error(new LigralException($"Asymptotic point estimate fails because the series is not stable.\nThe last values are [{string.Join(", ", series.Skip(series.Count()-10))}]"));
            }
            double bHat = yBar-kHat*xBar;
            logger.Debug($"Linear fit: y = {kHat}*x+{bHat}");
            double b = Math.Exp(bHat);
            double a = Math.Exp(kHat);
            logger.Debug($"Logarithmic fit: z = e^({a}*x)*e^{b}");
            return series[series.Length-1]-mode*b*Math.Pow(a, y.Count)/(1-a);
        }
    }
}