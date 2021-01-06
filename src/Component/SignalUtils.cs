/* Copyright (C) 2019-2021 Junruoyu Zheng. Home page: https://junruoyu-zheng.gitee.io/ligral

   Distributed under MIT license.
   See file LICENSE for detail or copy at https://opensource.org/licenses/MIT
*/

using MathNet.Numerics.LinearAlgebra;

namespace Ligral.Component
{
    
    static class SignalUtils
    {
        private static Logger logger = new Logger("SignalUtils");
        public static Signal BroadcastMultiply(this Signal left, Signal right)
        {
            if (left.IsMatrix)
            {
                return (left.Unpack() as Matrix<double>).BroadcastMultiply(right);
            }
            else
            {
                return ((double) left.Unpack()).BroadcastMultiply(right);
            }
        }
        public static Signal BroadcastMultiply(this Matrix<double> left, Signal right)
        {
            if (right.IsMatrix)
            {
                Matrix<double> rightMatrix = right.Unpack() as Matrix<double>;
                MatrixBuilder<double> m = Matrix<double>.Build;
                Matrix<double> result = left.PointwiseMultiply(rightMatrix);
                return new Signal(result);
            }
            else
            {
                return new Signal(left*((double) right.Unpack()));
            }
        }
        public static Signal BroadcastMultiply(this double left, Signal right)
        {
            if (right.IsMatrix)
            {
                return new Signal(left*(right.Unpack() as Matrix<double>));
            }
            else
            {
                return new Signal(left*((double) right.Unpack()));
            }
        }
        public static Signal BroadcastDivide(this Signal left, Signal right)
        {
            if (left.IsMatrix)
            {
                return (left.Unpack() as Matrix<double>).BroadcastDivide(right);
            }
            else
            {
                return ((double) left.Unpack()).BroadcastDivide(right);
            }
        }
        public static Signal BroadcastDivide(this Matrix<double> left, Signal right)
        {
            if (right.IsMatrix)
            {
                Matrix<double> rightMatrix = right.Unpack() as Matrix<double>;
                MatrixBuilder<double> m = Matrix<double>.Build;
                Matrix<double> result = left.PointwiseDivide(rightMatrix);
                return new Signal(result);
            }
            else
            {
                return new Signal(left/((double) right.Unpack()));
            }
        }
        public static Signal BroadcastDivide(this double left, Signal right)
        {
            if (right.IsMatrix)
            {
                return new Signal(left/(right.Unpack() as Matrix<double>));
            }
            else
            {
                return new Signal(left/((double) right.Unpack()));
            }
        }
        public static Signal BroadcastPower(this Signal left, Signal right)
        {
            if (left.IsMatrix)
            {
                return (left.Unpack() as Matrix<double>).BroadcastPower(right);
            }
            else
            {
                return ((double) left.Unpack()).BroadcastPower(right);
            }
        }
        public static Signal BroadcastPower(this Matrix<double> left, Signal right)
        {
            if (right.IsMatrix)
            {
                Matrix<double> rightMatrix = right.Unpack() as Matrix<double>;
                MatrixBuilder<double> m = Matrix<double>.Build;
                Matrix<double> result = left.PointwisePower(rightMatrix);
                return new Signal(result);
            }
            else
            {
                return new Signal(left.PointwisePower((double)right.Unpack()));
            }
        }
        public static Signal BroadcastPower(this double left, Signal right)
        {
            if (right.IsMatrix)
            {
                return new Signal((right.Unpack() as Matrix<double>).Map(v => System.Math.Pow(left, v)));
            }
            else
            {
                return new Signal(System.Math.Pow(left, ((double) right.Unpack())));
            }
        }
        public static Signal RaiseToPower(this Signal left, Signal right)
        {
            if (left.IsMatrix)
            {
                return (left.Unpack() as Matrix<double>).RaiseToPower(right);
            }
            else
            {
                return ((double) left.Unpack()).RaiseToPower(right);
            }
        }
        public static Signal RaiseToPower(this Matrix<double> left, Signal right)
        {
            if (right.IsMatrix)
            {
                throw logger.Error(new LigralException("Matrix to the power of a matrix is not supported."));
            }
            else
            {
                double indexDouble = (double) right.Unpack();
                int index = System.Convert.ToInt32(indexDouble);
                if (indexDouble - index > double.Epsilon*10)
                {
                    throw logger.Error(new LigralException($"Index of a matrix must be integer but {indexDouble} received, which would make the result to be complex."));
                }
                MatrixBuilder<double> m = Matrix<double>.Build;
                Matrix<double> result = m.DenseOfMatrix(left);
                if (left.RowCount != left.ColumnCount)
                {
                    throw logger.Error(new LigralException($"Cannot raise non-square matrix ({left.RowCount}x{left.ColumnCount}) to the power of a scalar."));
                }
                left.Power(index, result);
                return new Signal(result);
            }
        }
        public static Signal RaiseToPower(this double left, Signal right)
        {
            if (right.IsMatrix)
            {
                throw logger.Error(new LigralException("Scalar to the power of a matrix is not supported, because the result can be a complex matrix."));
            }
            else
            {
                double rightValue = (double) right.Unpack();
                return new Signal(System.Math.Pow(left, rightValue));
            }
        }
    }
    partial class Signal
    {
        
        public static Signal operator+(Signal left, Signal right)
        {
            if (left.IsMatrix)
            {
                return (left.Unpack() as Matrix<double>)+right;
            }
            else
            {
                return ((double) left.Unpack())+right;
            }
        }
        public static Signal operator+(Matrix<double> left, Signal right)
        {
            if (right.IsMatrix)
            {
                return new Signal(left+(right.Unpack() as Matrix<double>));
            }
            else
            {
                return new Signal(left+((double) right.Unpack()));
            }
        }
        public static Signal operator+(double left, Signal right)
        {
            if (right.IsMatrix)
            {
                return new Signal(left+(right.Unpack() as Matrix<double>));
            }
            else
            {
                return new Signal(left+((double) right.Unpack()));
            }
        }
        public static Signal operator-(Signal left, Signal right)
        {
            if (left.IsMatrix)
            {
                return (left.Unpack() as Matrix<double>)-right;
            }
            else
            {
                return ((double) left.Unpack())-right;
            }
        }
        public static Signal operator-(Matrix<double> left, Signal right)
        {
            if (right.IsMatrix)
            {
                return new Signal(left-(right.Unpack() as Matrix<double>));
            }
            else
            {
                return new Signal(left-((double) right.Unpack()));
            }
        }
        public static Signal operator-(double left, Signal right)
        {
            if (right.IsMatrix)
            {
                return new Signal(left-(right.Unpack() as Matrix<double>));
            }
            else
            {
                return new Signal(left-((double) right.Unpack()));
            }
        }
        public static Signal operator*(Signal left, Signal right)
        {
            if (left.IsMatrix)
            {
                return (left.Unpack() as Matrix<double>)*right;
            }
            else
            {
                return ((double) left.Unpack())*right;
            }
        }
        public static Signal operator*(Matrix<double> left, Signal right)
        {
            if (right.IsMatrix)
            {
                return new Signal(left*(right.Unpack() as Matrix<double>));
            }
            else
            {
                return new Signal(left*((double) right.Unpack()));
            }
        }
        public static Signal operator*(double left, Signal right)
        {
            if (right.IsMatrix)
            {
                return new Signal(left*(right.Unpack() as Matrix<double>));
            }
            else
            {
                return new Signal(left*((double) right.Unpack()));
            }
        }
        public static Signal operator/(Signal left, Signal right)
        {
            if (left.IsMatrix)
            {
                return (left.Unpack() as Matrix<double>)/right;
            }
            else
            {
                return ((double) left.Unpack())/right;
            }
        }
        public static Signal operator/(Matrix<double> left, Signal right)
        {
            if (right.IsMatrix)
            {
                return new Signal(left*(right.Unpack() as Matrix<double>).Inverse());
            }
            else
            {
                return new Signal(left/((double) right.Unpack()));
            }
        }
        public static Signal operator/(double left, Signal right)
        {
            if (right.IsMatrix)
            {
                return new Signal(left/(right.Unpack() as Matrix<double>));
            }
            else
            {
                return new Signal(left/((double) right.Unpack()));
            }
        }
    }
}