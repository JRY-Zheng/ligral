/* Copyright (C) 2019-2022 Junruoyu Zheng. Home page: https://junruoyu-zheng.gitee.io/ligral

   Distributed under MIT license.
   See file LICENSE for detail or copy at https://opensource.org/licenses/MIT
*/

using System.Collections.Generic;
using System;
using System.Linq;
using MathNet.Numerics.LinearAlgebra;
using ParameterDictionary = System.Collections.Generic.Dictionary<string, Ligral.Component.Parameter>;
using Ligral.Simulation;

namespace Ligral.Component.Models
{
    class Print : Model
    {
        protected override string DocString
        {
            get
            {
                return "This model prints the data to the console.";
            }
        }
        private static int maxLength = 10;
        private static string format = "Time: {0,-8:0.0000} {1,10} = {2:0.00}";
        private static int length
        {
            set
            {
                if (value > maxLength)
                {
                    maxLength = value;
                    format = "Time: {0,-8:0.0000} {1,"+ maxLength +"} = {2:0.00}";
                }
            }
        }
        private string varName;
        private double start = 0;
        private double end = 0;
        private int rowNo = 0;
        private int colNo = 0;
        private ObservationHandle handle;
        protected override void SetUpPorts()
        {
            InPortList.Add(new InPort("input", this));
        }
        protected override void SetUpParameters()
        {
            Parameters = new ParameterDictionary()
            {
                {"name", new Parameter(ParameterType.String , value=>
                {
                    varName = (string) value;
                }, ()=>{})},
                {"start", new Parameter(ParameterType.Signal , value=>
                {
                    start = value.ToScalar();
                }, ()=>
                {
                    start = 0;
                })},
                {"stop", new Parameter(ParameterType.Signal , value=>
                {
                    end = value.ToScalar();
                }, ()=>
                {
                    end = Double.PositiveInfinity;
                })},
                {"col", new Parameter(ParameterType.Signal , value=>
                {
                    colNo = value.ToInt();
                }, ()=>{})},
                {"row", new Parameter(ParameterType.Signal , value=>
                {
                    rowNo = value.ToInt();
                }, ()=>{})}
            };
        }
        public override void Prepare()
        {
            if (varName == null)
            {
                string inputSignalName = InPortList[0].Source.SignalName;
                varName = GivenName ?? inputSignalName ?? Name;
                if (Scope != null)
                {
                    if (GivenName != null || inputSignalName == null)
                    {
                        varName = Scope + "." + varName;
                    }
                }
            }
            length = varName.Length;
        }
        public override void Check()
        {
            if (rowNo>0 || colNo>0)
            {
                if (rowNo != InPortList[0].RowNo)
                {
                    throw logger.Error(new ModelException(this, $"Shape inconsistency. {InPortList[0].RowNo} rows got, {rowNo} expected."));
                }
                else if (colNo != InPortList[0].ColNo)
                {
                    throw logger.Error(new ModelException(this, $"Shape inconsistency. {InPortList[0].ColNo} columns got, {colNo} expected."));
                }
            }
            else
            {
                rowNo = InPortList[0].RowNo;
                colNo = InPortList[0].ColNo;
            }
            handle = Observation.CreateObservation(varName, rowNo, colNo);
        }
        protected override List<Matrix<double>> Calculate(List<Matrix<double>> values)
        {
            Matrix<double> inputSignal = values[0];
            handle.Cache(inputSignal);
            return Results;
        }
        public override void Refresh()
        {
            if (Solver.Time > end || Solver.Time < start) return;
            Matrix<double> outputVariableSignal = handle.GetObservation();
            logger.Prompt(string.Format(format, Solver.Time, varName, outputVariableSignal.ToStringInLine()));
        }
    }
}