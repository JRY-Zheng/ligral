/* Copyright (C) 2019-2021 Junruoyu Zheng. Home page: https://junruoyu-zheng.gitee.io/ligral

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
        private int rowNo = -1;
        private int colNo = -1;
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
                    start = Convert.ToInt32(value);
                }, ()=>
                {
                    start = 0;
                })},
                {"stop", new Parameter(ParameterType.Signal , value=>
                {
                    end = Convert.ToInt32(value);
                }, ()=>
                {
                    Settings settings = Settings.GetInstance();
                    end = settings.StopTime;
                })},
                {"col", new Parameter(ParameterType.Signal , value=>
                {
                    colNo = System.Convert.ToInt32(value);
                }, ()=>{})},
                {"row", new Parameter(ParameterType.Signal , value=>
                {
                    rowNo = System.Convert.ToInt32(value);
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
        protected override List<Signal> DefaultCalculate(List<Signal> values)
        {
            Signal inputSignal = values[0];
            if ((rowNo>=0 || colNo>=0) && !inputSignal.CheckShape(rowNo, colNo))
            {
                throw logger.Error(new ModelException(this, $"Shape inconsistency. {inputSignal.Shape()} got, ({rowNo}, {colNo}) expected."));
            }
            (rowNo, colNo) = inputSignal.Shape();
            handle = Observation.CreateObservation(varName, rowNo, colNo);
            Calculate = PostCalculate;
            return Calculate(values);
        }
        protected List<Signal> PostCalculate(List<Signal> values)
        {
            Signal inputSignal = values[0];
            handle.Cache(inputSignal);
            return Results;
        }
        public override void Refresh()
        {
            if (Solver.Time > end || Solver.Time < start) return;
            Signal outputVariableSignal = handle.GetObservation();
            logger.Prompt(string.Format(format, Solver.Time, varName, outputVariableSignal.ToStringInLine()));
        }
    }
}