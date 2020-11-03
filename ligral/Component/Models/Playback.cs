using System.Collections.Generic;
using System.Linq;
using ParameterDictionary = System.Collections.Generic.Dictionary<string, Ligral.Component.Parameter>;
using System;
using MathNet.Numerics.LinearAlgebra;
using DoubleCsvTable;
using Ligral.Component;
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
        private CsvTable table;
        private int rowNo = 0;
        private int colNo = 0;
        protected override void SetUpPorts()
        {
            OutPortList.Add(new OutPort("source", this));
        }
        protected override void SetUpParameters()
        {
            Parameters = new ParameterDictionary()
            {
                {"file", new Parameter(value=>
                {
                    table = new CsvTable((string)value, true);
                    if (table.Columns.Count < 2 || table.GetColumnName(0) != "Time")
                    {
                        throw new ModelException(this,"Invalid playback file");
                    }
                })},
                {"col", new Parameter(value=>
                {
                    colNo = Convert.ToInt32(value);
                }, ()=>{})},
                {"row", new Parameter(value=>
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
                throw new ModelException(this, $"Invalid playback input at time {Solver.Time}");
            }
        }
        protected override List<Signal> DefaultCalculate(List<Signal> values)
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
                throw new ModelException(this, $"Inconsistency of row, col and playback");
            }
            return Results;
        }
    }
}