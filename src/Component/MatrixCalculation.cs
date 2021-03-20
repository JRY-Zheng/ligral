/* Copyright (C) 2019-2021 Junruoyu Zheng. Home page: https://junruoyu-zheng.gitee.io/ligral

   Distributed under MIT license.
   See file LICENSE for detail or copy at https://opensource.org/licenses/MIT
*/

using System;
using MathNet.Numerics.LinearAlgebra;

namespace Ligral.Component 
{
    public static class MatrixCalculation
    {
        public static bool IsScalar(this Matrix<double> matrix)
        {
            return matrix.ColumnCount==1 && matrix.RowCount==1;
        }
        public static bool IsVector(this Matrix<double> matrix)
        {
            return matrix.ColumnCount==1 || matrix.RowCount==1;
        }
        public static bool IsColumnVector(this Matrix<double> matrix)
        {
            return matrix.ColumnCount==1;
        }
        public static bool IsRowVector(this Matrix<double> matrix)
        {
            return matrix.RowCount==1;
        }
        public static bool IsEmpty(this Matrix<double> matrix)
        {
            return matrix.ColumnCount==0 || matrix.RowCount==0;
        }
        public static bool IsSingular(this Matrix<double> matrix)
        {
            var rank = matrix.TolerantRank();
            return rank < matrix.ColumnCount && rank < matrix.RowCount;
        }
        public static bool EqualTo(this Matrix<double> matrix, Matrix<double> goal)
        {
            if (matrix.ColumnCount != goal.ColumnCount) return false;
            if (matrix.RowCount != goal.RowCount) return false;
            for (int row=0; row < matrix.RowCount; row++)
            {
                for (int col=0; col < matrix.ColumnCount; col++)
                {
                    if (Math.Abs(matrix[row, col] - goal[row, col]) > 1e-10 )
                    {
                        return false;
                    }
                }
            }
            return true;
        }
        public static Matrix<double> ToMatrix(this object o)
        {
            switch (o)
            {
            case Matrix<double> matrix:
                return matrix;
            case int i:
                return Matrix<double>.Build.Dense(1, 1, i);
            case double d:
                return Matrix<double>.Build.Dense(1, 1, d);
            default:
                throw new ArgumentException($"Cannot convert type {o.GetType().Name} to matrix");
            }
        }
        public static Matrix<double> ToMatrix(this double d)
        {
            return Matrix<double>.Build.Dense(1, 1, d);
        }
        public static Matrix<double> ToMatrix(this int d)
        {
            return Matrix<double>.Build.Dense(1, 1, d);
        }
        public static double ToScalar(this double d)
        {
            return d;
        }
        public static double ToScalar(this int d)
        {
            return d;
        }
        public static double ToScalar(this object o)
        {
            switch (o)
            {
            case Matrix<double> matrix:
                if (!matrix.IsScalar())
                {
                    throw new ArgumentException($"Cannot cast matrix with shape {matrix.ShapeString()} to scalar");
                }
                return matrix[0,0];
            case int i:
                return i;
            case double d:
                return d;
            default:
                throw new ArgumentException($"Cannot convert type {o.GetType().Name} to matrix");
            }
        }
        public static string ShapeString(this Matrix<double> matrix)
        {
            return $"{matrix.RowCount}x{matrix.ColumnCount}";
        }
        public static Matrix<double> MatAdd(this Matrix<double> left, Matrix<double> right)
        {
            return left.Broadcast(right, (x, y) => x + y);
        }
        public static Matrix<double> MatSub(this Matrix<double> left, Matrix<double> right)
        {
            return left.Broadcast(right, (x, y) => x - y);
        }
        public static Matrix<double> DotMul(this Matrix<double> left, Matrix<double> right)
        {
            return left.Broadcast(right, (x, y) => x * y);
        }
        public static Matrix<double> DotDiv(this Matrix<double> left, Matrix<double> right)
        {
            return left.Broadcast(right, (x, y) => 
            {
                if (y==0)
                {
                    throw new DivideByZeroException();
                }
                else
                {
                    return x/y;
                }
            });
        }
        public static Matrix<double> MatMul(this Matrix<double> left, Matrix<double> right)
        {
            if (left.IsScalar()) 
            {
                return left[0,0]*right;
            }
            else if (right.IsScalar()) 
            {
                return left*right[0,0];
            }
            else return left*right;
        }
        public static Matrix<double> RightDiv(this Matrix<double> left, Matrix<double> right)
        {
            if (left.IsScalar()) 
            {
                return left[0,0]/right;
            }
            else if (right.IsScalar()) 
            {
                if (right[0,0]==0)
                {
                    throw new DivideByZeroException();
                }
                return left/right[0,0];
            }
            else if (right.IsSingular())
            {
                throw new DivideByZeroException();
            }
            else return left*right.Inverse();
        }
        public static Matrix<double> LeftDiv(this Matrix<double> left, Matrix<double> right)
        {
            if (left.IsScalar()) 
            {
                if (left[0,0]==0)
                {
                    throw new DivideByZeroException();
                }
                return right/left[0,0];
            }
            else if (right.IsScalar()) 
            {
                return right[0,0]/left;
            }
            else if (left.IsSingular())
            {
                throw new DivideByZeroException();
            }
            else 
            {
                return left.Solve(right);
            }
        }
        public static Matrix<double> DotPow(this Matrix<double> left, Matrix<double> right)
        {
            return left.Broadcast(right, Math.Pow);
        }
        public static Matrix<double> MatPow(this Matrix<double> left, Matrix<double> right)
        {
            if (right.IsScalar()) 
            {
                int exponent = Convert.ToInt32(right[0,0]);
                if (Math.Abs(right[0,0] - exponent) > double.Epsilon*10)
                {
                    throw new ArgumentException($"Index of a matrix must be integer but {right[0,0]} received, which would make the result to be complex");
                }
                if (exponent < 0)
                {
                    if (left.IsSingular())
                    {
                        throw new DivideByZeroException();
                    }
                    else
                    {
                        return left.Inverse().Power(-exponent);
                    }
                }
                else return left.Power(exponent);
            }
            else 
            {
                throw new ArgumentException($"The index must be a scalar but a matrix {right.ShapeString()} received");
            }
        }
        public static double UpperBound(this double self, params double[] bounds)
        {
            double m = self;
            foreach (double bound in bounds)
            {
                if (m > bound)
                {
                    m = bound;
                }
            }
            return m;
        }
        public static double LowerBound(this double self, params double[] bounds)
        {
            double m = self;
            foreach (double bound in bounds)
            {
                if (m < bound)
                {
                    m = bound;
                }
            }
            return m;
        }
    }
}