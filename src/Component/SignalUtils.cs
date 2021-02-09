/* Copyright (C) 2019-2021 Junruoyu Zheng. Home page: https://junruoyu-zheng.gitee.io/ligral

   Distributed under MIT license.
   See file LICENSE for detail or copy at https://opensource.org/licenses/MIT
*/

using MathNet.Numerics.LinearAlgebra;
using Ligral.Component.Models;
using Dict=System.Collections.Generic.Dictionary<string,object>;

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
        private static Group Calculate(this ILinkable left, ILinkable right, string operant)
        {
            if (left.OutPortCount()!=1||right.OutPortCount()!=1)
            {
                throw logger.Error(new LigralException("Out port number should be 1 when calculated"));
            }
            Calculator calculator = ModelManager.Create("Calculator") as Calculator;
            Dict dictionary = new Dict(){{"type", operant}};
            calculator.Configure(dictionary);
            left.Connect(0, calculator.Expose(0));
            right.Connect(0, calculator.Expose(1));
            Group group = new Group();
            group.AddInputModel(left);
            group.AddInputModel(right);
            group.AddOutputModel(calculator);
            return group;
        }
        public static Group Add(this ILinkable left, ILinkable right)
        {
            return Calculate(left, right, "+");
        }
        public static Group Subtract(this ILinkable left, ILinkable right)
        {
            return Calculate(left, right, "-");
        }
        public static Group Multiply(this ILinkable left, ILinkable right)
        {
            return Calculate(left, right, "*");
        }
        public static Group Divide(this ILinkable left, ILinkable right)
        {
            return Calculate(left, right, "/");
        }
        public static Group Power(this ILinkable left, ILinkable right)
        {
            return Calculate(left, right, "^");
        }
        public static Group BroadcastMultiply(this ILinkable left, ILinkable right)
        {
            return Calculate(left, right, ".*");
        }
        public static Group BroadcastDivide(this ILinkable left, ILinkable right)
        {
            return Calculate(left, right, "./");
        }
        public static Group BroadcastPower(this ILinkable left, ILinkable right)
        {
            return Calculate(left, right, ".^");
        }
        public static Group Positive(this ILinkable linkable)
        {
            Group group = new Group();
            group.AddInputModel(linkable);
            group.AddOutputModel(linkable);
            return group;
        }
        public static Group Negative(this ILinkable linkable)
        {
            Group group = new Group();
            group.AddInputModel(linkable);
            for (int i = 0; i < linkable.OutPortCount(); i++)
            {
                Gain gain = ModelManager.Create("Gain") as Gain;
                Dict dictionary = new Dict(){{"value", -1}};
                gain.Configure(dictionary);
                linkable.Connect(i, gain.Expose(0));
                group.AddOutputModel(gain);
            }
            return group;
        }
        public static Matrix<double> Stack(Matrix<double> upper, Matrix<double> lower)
        {
            if (upper is null) return lower;
            else if (lower is null) return upper;
            else return upper.Stack(lower);
        }
        public static Matrix<double> Stack(params Matrix<double>[] matrice)
        {
            var mat = matrice[0];
            for (int i = 1; i < matrice.Length; i++)
            {
                mat = SignalUtils.Stack(mat, matrice[i]);
            }
            return mat;
        }
        public static Matrix<double> Append(Matrix<double> upper, Matrix<double> lower)
        {
            if (upper is null) return lower;
            else if (lower is null) return upper;
            else return upper.Append(lower);
        }
        public static Matrix<double> Append(params Matrix<double>[] matrice)
        {
            var mat = matrice[0];
            for (int i = 1; i < matrice.Length; i++)
            {
                mat = SignalUtils.Append(mat, matrice[i]);
            }
            return mat;
        }
        public static string SPrint(Matrix<double> matrix, string name)
        {
            if (matrix is Matrix<double> mat)
            {
                if (mat.ColumnCount == 0 || mat.RowCount == 0)
                {
                    return $"The matrix {name} is of shape ({mat.RowCount} {mat.ColumnCount})";
                }
                else
                {
                    return $"The matrix {name} is {mat}";
                }
            }
            else
            {
                return $"The matrix {name} is null";
            }
        }
    }
}