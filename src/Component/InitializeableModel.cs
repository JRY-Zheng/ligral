/* Copyright (C) 2019-2021 Junruoyu Zheng. Home page: https://junruoyu-zheng.gitee.io/ligral

   Distributed under MIT license.
   See file LICENSE for detail or copy at https://opensource.org/licenses/MIT
*/

using ParameterDictionary = System.Collections.Generic.Dictionary<string, Ligral.Component.Parameter>;
using System;
using MathNet.Numerics.LinearAlgebra;

namespace Ligral.Component
{
    public class InitializeableModel : Model
    {
        protected Matrix<double> initial = null;
        protected int colNo = 0;
        protected int rowNo = 0;
        protected bool Initialized = false;
        public virtual void Initialize()
        {
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
                    initial = value.ToMatrix();
                }, ()=>{})},
                {"col", new Parameter(ParameterType.Signal , value=>
                {
                    colNo = Convert.ToInt32(value);
                    InPortList[0].ColNo = colNo;
                }, ()=>{})},
                {"row", new Parameter(ParameterType.Signal , value=>
                {
                    rowNo = Convert.ToInt32(value);
                    InPortList[0].RowNo  = rowNo;
                }, ()=>{})}
            };
        }
        protected override void AfterConfigured()
        {
            if (colNo == 0 && rowNo == 0) // implicit
            {
                // if initial is null, let check function to determine initial
                if (initial!=null)
                {
                    rowNo = initial.RowCount;
                    colNo = initial.ColumnCount;
                }
            }
            else if (colNo > 0 && rowNo > 0)
            {
                if (initial == null)
                {
                    MatrixBuilder<double> m = Matrix<double>.Build;
                    initial = m.Dense(rowNo, colNo, 0);
                }
                else if (colNo != initial.ColumnCount || rowNo != initial.RowCount)
                {
                    throw logger.Error(new ModelException(this, $"Inconsistency between initial value and shape"));
                }
            }
            else
            {
                throw logger.Error(new ModelException(this, $"Matrix row and col should be positive non-zero\n but we get: {colNo}x{rowNo}"));
            }
        }
        public override void Check()
        {
            int inputRowNo = InPortList[0].RowNo;
            int inputColNo = InPortList[0].ColNo;
            var build = Matrix<double>.Build;
            if (rowNo == 0 && colNo == 0)
            {
                if (inputRowNo == 0 && inputColNo == 0)
                {
                    initial = 0.ToMatrix();
                    rowNo = 1;
                    colNo = 1;
                }
                else
                {
                    rowNo = inputRowNo;
                    colNo = inputColNo;
                    initial = build.Dense(rowNo, colNo, 0);
                }
            }
            else if (inputRowNo > 0 && inputColNo > 0)
            {
                if (inputColNo != colNo)
                {
                    throw logger.Error(new ModelException(this, $"Column number in consistent, got {inputColNo}, but {colNo} expected."));
                }
                else if (inputRowNo != rowNo)
                {
                    throw logger.Error(new ModelException(this, $"Row number in consistent, got {inputRowNo}, but {rowNo} expected."));
                }
            }
            OutPortList[0].SetShape(rowNo, colNo);
        }
    }
}