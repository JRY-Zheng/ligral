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
                Matrix<double> result = m.DenseOfMatrix(rightMatrix);
                left.Map2((l, r)=>l*r, rightMatrix, result);
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
        public static Signal PowerOf(this Signal left, Signal right)
        {
            if (left.IsMatrix)
            {
                return (left.Unpack() as Matrix<double>).PowerOf(right);
            }
            else
            {
                return ((double) left.Unpack()).PowerOf(right);
            }
        }
        public static Signal PowerOf(this Matrix<double> left, Signal right)
        {
            if (right.IsMatrix)
            {
                throw logger.Error(new LigralException("Matrix power matrix base is not supported."));
            }
            else
            {
                int index = System.Convert.ToInt32(right.Unpack());
                MatrixBuilder<double> m = Matrix<double>.Build;
                Matrix<double> result = m.DenseOfMatrix(left);
                left.Power(index, result);
                return new Signal(result);
            }
        }
        public static Signal PowerOf(this double left, Signal right)
        {
            if (right.IsMatrix)
            {
                Matrix<double> rightMatrix = right.Unpack() as Matrix<double>;
                MatrixBuilder<double> m = Matrix<double>.Build;
                Matrix<double> result = m.DenseOfMatrix(rightMatrix);
                // [TODO] QR decomposition
                return new Signal(left/(right.Unpack() as Matrix<double>));
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