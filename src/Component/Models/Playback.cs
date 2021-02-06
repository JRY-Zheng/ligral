/* Copyright (C) 2019-2021 Junruoyu Zheng. Home page: https://junruoyu-zheng.gitee.io/ligral

   Distributed under MIT license.
   See file LICENSE for detail or copy at https://opensource.org/licenses/MIT
*/

using System.Collections.Generic;
using System.Linq;
using ParameterDictionary = System.Collections.Generic.Dictionary<string, Ligral.Component.Parameter>;
using System;
using MathNet.Numerics.LinearAlgebra;
using Ligral.Tools;
using Ligral.Simulation;

namespace Ligral.Component.Models
{
    class Playback : Model
    {
        protected override string DocString
        {
            get
            {
                return "This model generates a playback provided by a time-data csv file.";
            }
        }
        private Storage table;
        private int rowNo = 0;
        private int colNo = 0;
        protected override void SetUpPorts()
        {
            OutPortList.Add(new OutPort("source", this));
        }
        public override void Check()
        {
            OutPortList[0].SetShape(rowNo, colNo);
        }
        protected override void SetUpParameters()
        {
            Parameters = new ParameterDictionary()
            {
                {"file", new Parameter(ParameterType.String , value=>
                {
                    table = new Storage((string)value, true);
                    if (table.Columns.Count < 2 || table.GetColumnName(0) != "Time")
                    {
                        throw logger.Error(new ModelException(this,"Invalid playback file"));
                    }
                })},
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
        private List<double> Interpolate()
        {
            List<double> before = table.Data.FindLast(row => row[0] < Solver.Time);
            List<double> after = table.Data.Find(row => row[0] > Solver.Time);
            List<double> current = table.Data.Find(row => row[0] == Solver.Time);
            if (current != null)
            {
                return current;
            }
            else if (before != null && after != null)
            {
                // Results.Add(before.Data+(after.Data-before.Data)/(after.Time-before.Time)*(time-before.Time));
                double tb = before[0];
                double ta = after[0];
                return before.Zip(after, (b, a) => b + (a - b) / (ta - tb) * (Solver.Time - tb)).ToList();
            }
            else if (before == null && after != null)
            {
                return after.ToList();
            }
            else if (before != null && after == null)
            {
                return before.ToList();
            }
            else
            {
                throw logger.Error(new ModelException(this, $"Invalid playback input at time {Solver.Time}"));
            }
        }
        protected override List<Signal> Calculate(List<Signal> values)
        {
            // Results.Clear();
            Signal outputSignal = Results[0];
            List<double> playback = Interpolate();
            if (colNo == 0 && rowNo == 0 && playback.Count == 2)
            {
                outputSignal.Pack(playback[1]);
            }
            else if (colNo * rowNo == playback.Count - 1)
            {
                MatrixBuilder<double> m = Matrix<double>.Build;
                Matrix<double> matrix = m.Dense(colNo, rowNo, playback.Skip(1).ToArray()).Transpose();
                outputSignal.Pack(matrix);
            }
            else
            {
                throw logger.Error(new ModelException(this, $"Inconsistency of row, col and playback"));
            }
            return Results;
        }
    }
}