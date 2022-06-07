/* Copyright (C) 2019-2022 Junruoyu Zheng. Home page: https://junruoyu-zheng.gitee.io/ligral

   Distributed under MIT license.
   See file LICENSE for detail or copy at https://opensource.org/licenses/MIT
*/

using System.Collections.Generic;
using MathNet.Numerics.LinearAlgebra;
using Ligral.Component;
using Ligral.Simulation;
using Ligral.Syntax.CodeASTs;
using ParameterDictionary = System.Collections.Generic.Dictionary<string, Ligral.Component.Parameter>;

namespace Ligral.Component.Models
{
    class TransferFunction : InitializeableModel
    {
        protected override string DocString
        {
            get
            {
                return "This model generate state space system according to transfer function with observable canonical form.";
            }
        }
        private string varName;
        private Matrix<double> numerator;
        private Matrix<double> denominator;
        private Matrix<double> A;
        private Matrix<double> B;
        private Matrix<double> C;
        private Matrix<double> D;
        protected StateHandle handle;
        protected override void SetUpParameters()
        {
            Parameters = new ParameterDictionary()
            {
                {"initial", new Parameter(ParameterType.Signal , value=>
                {
                    initial = value.ToMatrix();
                }, ()=>{})},
                {"name", new Parameter(ParameterType.String , value=>
                {
                    varName=(string)value;
                }, ()=>{})},
                {"num", new Parameter(ParameterType.String , value=>
                {
                    numerator = value.ToMatrix();
                })},
                {"den", new Parameter(ParameterType.String , value=>
                {
                    denominator = value.ToMatrix();
                })},
                {"col", new Parameter(ParameterType.Signal , value=>
                {
                    colNo = value.ToInt();
                    if (colNo <= 0)
                    {
                        throw logger.Error(new ModelException(this, $"column number should be positive but got {colNo}"));
                    }
                    InPortList[0].ColNo = colNo;
                }, ()=>{})},
            };
        }
        protected override void AfterConfigured()
        {
            if (initial == null)
            {
                
            }
            else if (initial.RowCount != 1)
            {
                throw logger.Error(new ModelException(this, "Initial of transfer function should be (1, n)"));
            }
            else if (colNo == 0)
            {
                colNo = initial.ColumnCount; 
            }
            else if (colNo != initial.ColumnCount)
            {
                throw logger.Error(new ModelException(this, $"Initial column number is {initial.ColumnCount}, expect {colNo}"));
            }
            if (numerator.RowCount !=1 )
            {
                throw logger.Error(new ModelException(this, "Numerator of transfer function should have one row"));
            }
            if (denominator.RowCount !=1 )
            {
                throw logger.Error(new ModelException(this, "Denominator of transfer function should have one row"));
            }
            if (denominator.ColumnCount < numerator.ColumnCount)
            {
                throw logger.Error(new ModelException(this, "Denominator should be longer than or the same with numerator"));
            }
            if (denominator[0, 0] == 0)
            {
                throw logger.Error(new ModelException(this, "The first element of denominator cannot be 0"));
            }
            int N = denominator.ColumnCount;
            int M = numerator.ColumnCount;
            numerator /= denominator[0,0];
            denominator /= denominator[0,0];
            var expandedNumerator = Matrix<double>.Build.Dense(1, N, 0);
            for (int i=0; i<M; i++)
            {
                expandedNumerator[0, N-M+i] = numerator[0, i];
            }
            D = expandedNumerator.SubMatrix(0,1,0,1);
            var reducedNumerator = (expandedNumerator - D[0,0]*denominator).SubMatrix(0,1,1,N-1);
            B = Matrix<double>.Build.Dense(N-1, 1, 0);
            for (int i=0; i<N-1; i++)
            {
                B[i, 0] = reducedNumerator[0, N-i-2];
            }
            C = Matrix<double>.Build.Dense(1, N-1, 0);
            C[0, N-2] = 1;
            A = Matrix<double>.Build.Dense(N-1, N-1, 0);
            for (int i=0; i<N-1; i++)
            {
                A[N-2, N-2-i] = -denominator[0, i+1];
            }
            for (int i=1; i<N-1; i++)
            {
                A[i, i-1] = 1;
            }
            Results[0] = initial;
        }
        public override void Prepare()
        {
            varName = varName ?? Name;
        }
        public override void Check()
        {
            if (InPortList[0].RowNo != 1)
            {
                throw logger.Error(new ModelException(this, "Input of transfer function should be (1, n)"));
            }
            if (colNo > 0 && InPortList[0].ColNo > 0 && colNo != InPortList[0].ColNo)
            {
                throw logger.Error(new ModelException(this, $"Input column number is {InPortList[0].ColNo}, expect {colNo}"));
            }
            else if (colNo == 0)
            {
                colNo = InPortList[0].ColNo;
            }
            if (initial == null)
            {
                initial = Matrix<double>.Build.Dense(1, colNo, 0);
            }
            OutPortList[0].SetShape(1, colNo);
            var x0 = Matrix<double>.Build.Dense(denominator.ColumnCount-1, colNo, 0);
            for (int i=0; i<colNo; i++)
            {
                x0[x0.RowCount-1, i] = initial[0, i];
            }
            handle = State.CreateState(varName, denominator.ColumnCount-1, colNo, x0);
        }
        public override void Confirm()
        {
            int inputColNo = InPortList[0].ColNo;
            if (inputColNo != colNo)
            {
                throw logger.Error(new ModelException(this, $"Column number in consistent, got {inputColNo}, but {colNo} expected."));
            }
            if (colNo == 0)
            {
                throw logger.Error(new ModelException(this, $"Column number cannot be determined."));
            }
        }
        protected override void InputUpdate(Matrix<double> u)
        {
            try
            {
                var x = handle.GetState();
                handle.SetDerivative(A*x+B*u);
            }
            catch (LigralException)
            {
                throw logger.Error(new ModelException(this));
            }
        }
        protected override List<Matrix<double>> Calculate(List<Matrix<double>> values)
        {
            Results[0] = C*handle.GetState()+D*values[0]    ;
            return Results;
        }
        public static List<CodeAST> ConstructConfigurationAST(string GlobalName, StateHandle handle, Matrix<double> initial)
        {
            var codeASTs = new List<CodeAST>();
            AssignCodeAST ctxAST = new AssignCodeAST();
            ctxAST.Destination = $"{GlobalName}.ctx";
            ctxAST.Source = "&ctx";
            codeASTs.Add(ctxAST);
            LShiftCodeAST initialAST = new LShiftCodeAST();
            initialAST.Destination = $"{GlobalName}.initial";
            initialAST.Source = string.Join(',', initial.ToColumnMajorArray());
            codeASTs.Add(initialAST);
            AssignCodeAST indexAST = new AssignCodeAST();
            indexAST.Destination = $"{GlobalName}.index";
            indexAST.Source = State.StatePool.IndexOf(handle.space[0]).ToString();
            codeASTs.Add(indexAST);
            CallCodeAST configAST = new CallCodeAST();
            configAST.FunctionName = $"{GlobalName}.config";
            codeASTs.Add(configAST);
            return codeASTs;
        }
        public override List<CodeAST> ConstructConfigurationAST()
        {
            return ConstructConfigurationAST(GlobalName, handle, initial);
        }
    }
}