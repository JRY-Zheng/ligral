/* Copyright (C) 2019-2022 Junruoyu Zheng. Home page: https://junruoyu-zheng.gitee.io/ligral

   Distributed under MIT license.
   See file LICENSE for detail or copy at https://opensource.org/licenses/MIT
*/

using System.Collections.Generic;
using Ligral.Component;
using MathNet.Numerics.LinearAlgebra;
using Ligral.Simulation;
using Ligral;
using ParameterDictionary = System.Collections.Generic.Dictionary<string, Ligral.Component.Parameter>;


namespace LigralPlugins.Control.Models
{
    public class PIDController : InitializeableModel
    {

        protected override string DocString
        {
            get
            {
                return "This model generates PID control signal of input.";
            }
        }
        private string varName;
        private Matrix<double> A;
        private Matrix<double> B;
        private Matrix<double> C;
        private Matrix<double> D;
        private Matrix<double> x0;
        private double Ki;
        private double Kp;
        private double Kd;
        private double tau;
        protected StateHandle handle;
        protected override void SetUpPorts()
        {
            base.SetUpPorts();
            InPortList[0].Name = "u";
            OutPortList[0].Name = "y";
        }protected override void SetUpParameters()
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
                {"Ki", new Parameter(ParameterType.Signal , value=>
                {
                    Ki = value.ToScalar();
                })},
                {"Kp", new Parameter(ParameterType.Signal , value=>
                {
                    Kp = value.ToScalar();
                })},
                {"Kd", new Parameter(ParameterType.Signal , value=>
                {
                    Kd = value.ToScalar();
                })},
                {"tau", new Parameter(ParameterType.Signal , value=>
                {
                    tau = value.ToScalar();
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
            if ( tau <= 0 )
            {
                throw logger.Error(new ModelException(this, "Parameter tau should be positive"));
            }
            A = Matrix<double>.Build.DenseOfColumnMajor(2, 2, new double[]{0, 1, 0, -1/tau});
            B = Matrix<double>.Build.DenseOfColumnMajor(2, 1, new double[]{Ki/tau, -Kd/tau/tau+Ki});
            C = Matrix<double>.Build.DenseOfColumnMajor(1, 2, new double[]{0, 1});
            D = Matrix<double>.Build.DenseOfColumnMajor(1, 1, new double[]{Kd/tau+Kp});
            Results[0] = initial;
        }
        public override void Prepare()
        {
            varName = PIDController.GetVarName(varName, this);
        }
        public override void Check()
        {
            Guessing = false;
            if (InPortList[0].RowNo > 1)
            {
                throw logger.Error(new ModelException(this, "Input of transfer function should be (1, n)"));
            }
            if (colNo > 0 && InPortList[0].ColNo > 0 && colNo != InPortList[0].ColNo)
            {
                throw logger.Error(new ModelException(this, $"Input column number is {InPortList[0].ColNo}, expect {colNo}"));
            }
            else if (colNo == 0 && InPortList[0].ColNo == 0)
            {
                colNo = 1;
                Guessing = true;
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
            x0 = Matrix<double>.Build.Dense(2, colNo, 0);
            for (int i=0; i<colNo; i++)
            {
                x0[x0.RowCount-1, i] = initial[0, i];
            }
            handle = State.CreateState(varName, 2, colNo, x0);
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
            Results[0] = C*handle.GetState()+D*(values[0]??Matrix<double>.Build.Dense(1, colNo, 0));
            return Results;
        }
    }
}