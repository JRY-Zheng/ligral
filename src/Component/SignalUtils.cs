/* Copyright (C) 2019-2021 Junruoyu Zheng. Home page: https://junruoyu-zheng.gitee.io/ligral

   Distributed under MIT license.
   See file LICENSE for detail or copy at https://opensource.org/licenses/MIT
*/

using MathNet.Numerics.LinearAlgebra;
using Ligral.Component.Models;
using Ligral.Syntax;
using Dict=System.Collections.Generic.Dictionary<string,object>;

namespace Ligral.Component
{
    
    static class SignalUtils
    {
        private static Logger logger = new Logger("SignalUtils");
        private static Group Calculate(this ILinkable left, ILinkable right, Model calculator)
        {
            if (left.OutPortCount()!=1||right.OutPortCount()!=1)
            {
                throw logger.Error(new LigralException("Out port number should be 1 when calculated"));
            }
            left.Connect(0, calculator.Expose(0));
            right.Connect(0, calculator.Expose(1));
            Group group = new Group();
            group.AddInputModel(left);
            group.AddInputModel(right);
            group.AddOutputModel(calculator);
            return group;
        }
        public static Group Add(this ILinkable left, ILinkable right, Token token)
        {
            return Calculate(left, right, ModelManager.Create("Add", token));
        }
        public static Group Subtract(this ILinkable left, ILinkable right, Token token)
        {
            return Calculate(left, right, ModelManager.Create("Sub", token));
        }
        public static Group Multiply(this ILinkable left, ILinkable right, Token token)
        {
            return Calculate(left, right, ModelManager.Create("Mul", token));
        }
        public static Group Divide(this ILinkable left, ILinkable right, Token token)
        {
            return Calculate(left, right, ModelManager.Create("Div", token));
        }
        public static Group ReverseDivide(this ILinkable left, ILinkable right, Token token)
        {
            return Calculate(left, right, ModelManager.Create("RDiv", token));
        }
        public static Group Power(this ILinkable left, ILinkable right, Token token)
        {
            return Calculate(left, right, ModelManager.Create("Pow2", token));
        }
        public static Group BroadcastMultiply(this ILinkable left, ILinkable right, Token token)
        {
            return Calculate(left, right, ModelManager.Create("DotMul", token));
        }
        public static Group BroadcastDivide(this ILinkable left, ILinkable right, Token token)
        {
            return Calculate(left, right, ModelManager.Create("DotDiv", token));
        }
        public static Group BroadcastPower(this ILinkable left, ILinkable right, Token token)
        {
            return Calculate(left, right, ModelManager.Create("DotPow", token));
        }
        public static Group Positive(this ILinkable linkable)
        {
            Group group = new Group();
            group.AddInputModel(linkable);
            group.AddOutputModel(linkable);
            return group;
        }
        public static Group Negative(this ILinkable linkable, Token token)
        {
            Group group = new Group();
            group.AddInputModel(linkable);
            for (int i = 0; i < linkable.OutPortCount(); i++)
            {
                Gain gain = ModelManager.Create("Gain", token) as Gain;
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