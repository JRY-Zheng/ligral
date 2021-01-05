/* Copyright (C) 2019-2021 Junruoyu Zheng. Home page: https://junruoyu-zheng.gitee.io/ligral

   Distributed under MIT license.
   See file LICENSE for detail or copy at https://opensource.org/licenses/MIT
*/

using ParameterDictionary = System.Collections.Generic.Dictionary<string, Ligral.Component.Parameter>;
using System;
using MathNet.Numerics.LinearAlgebra;

namespace Ligral.Component
{
    class InitializeableModel : Model
    {
        protected Signal initial = new Signal();
        protected bool isMatrix {get {return initial.IsMatrix;}}
        protected int colNo = -1;
        protected int rowNo = -1;
        protected bool Initialized = false;
        public virtual void Initialize()
        {
            Signal inputSignal = new Signal();
            if (isMatrix)
            {
                MatrixBuilder<double> m = Matrix<double>.Build;
                Matrix<double> matrix = m.Dense(rowNo, colNo, 0);
                inputSignal.Pack(matrix);
            }
            else
            {
                inputSignal.Pack(0);
            }
            InPortList.FindAll(inPort=>!inPort.Visited).ForEach(inPort=>inPort.Input(inputSignal));
            Initialized = true;
        }
        
        public override bool IsReady()
        {
            return Initialized || base.IsReady();
        }
        protected override void SetUpParameters()
        {
            Parameters = new ParameterDictionary()
            {
                {"initial", new Parameter(ParameterType.Signal , value=>
                {
                    initial.Pack(value);
                }, ()=>{})},
                {"col", new Parameter(ParameterType.Signal , value=>
                {
                    colNo = Convert.ToInt32(value);
                }, ()=>{})},
                {"row", new Parameter(ParameterType.Signal , value=>
                {
                    rowNo = Convert.ToInt32(value);
                }, ()=>{})}
            };
        }
        protected override void AfterConfigured()
        {
            if (colNo == 0 && rowNo == 0)
            {
                if (!initial.Packed)
                {
                    initial.Pack(0);
                }
                else if (initial.IsMatrix)
                {
                    throw logger.Error(new ModelException(this, $"Matrix row and col should be positive non-zero: {colNo}x{rowNo}"));
                }
                else // initial is double
                { }
            }
            else if (colNo > 0 && rowNo > 0)
            {
                if (!initial.Packed)
                {
                    MatrixBuilder<double> m = Matrix<double>.Build;
                    Matrix<double> matrix = m.Dense(rowNo, colNo, 0);
                    initial.Pack(matrix);
                }
                else if (initial.IsMatrix)
                { 
                    Matrix<double> initialMatrix = initial.Unpack() as Matrix<double>;
                    if (colNo != initialMatrix.ColumnCount || rowNo != initialMatrix.RowCount)
                    {
                        throw logger.Error(new ModelException(this, $"Inconsistency between initial value and shape"));
                    }
                }
                else // initial is double
                {
                    throw logger.Error(new ModelException(this, $"For scalar, row and col should be 0: {colNo}x{rowNo}"));
                }
            }
            else if (colNo == -1 && rowNo == -1) // implicit
            {
                if (!initial.Packed)
                {
                    initial.Pack(0);
                    rowNo = 0;
                    colNo = 0;
                }
                else if (initial.IsMatrix)
                {
                    Matrix<double> matrix = initial.Unpack() as Matrix<double>;
                    rowNo = matrix.RowCount;
                    colNo = matrix.ColumnCount;
                }
                else // initial is double
                {
                    rowNo = 0;
                    colNo = 0;
                }
            }
            else
            {
                throw logger.Error(new ModelException(this, $"Matrix row and col should be positive non-zero, for scalar both zeros\n but we get: {colNo}x{rowNo}"));
            }
        }
    }
}