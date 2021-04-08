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
    class Interpolation : Model
    {
        protected override string DocString
        {
            get
            {
                return "This model generates an interpolation of the given input provided by a csv file.";
            }
        }
        private Storage table;
        private int rowNo = 1;
        private int colNo = 1;
        public override void Check()
        {
            if (InPortList[0].RowNo != 1 || InPortList[0].ColNo != 1)
            {
                throw logger.Error(new ModelException(this, "Interpolation only accept scalar input"));
            }
            OutPortList[0].SetShape(rowNo, colNo);
        }
        protected override void SetUpParameters()
        {
            Parameters = new ParameterDictionary()
            {
                {"file", new Parameter(ParameterType.String , value=>
                {
                    table = new Storage((string)value, true);
                    if (table.Columns.Count < 2)
                    {
                        throw logger.Error(new ModelException(this,"Invalid playback file"));
                    }
                })},
                {"col", new Parameter(ParameterType.Signal , value=>
                {
                    colNo = value.ToInt();
                    if (colNo<1)
                    {
                        throw logger.Error(new ModelException(this, $"column number should be at least one but {colNo} received"));
                    }
                }, ()=>{})},
                {"row", new Parameter(ParameterType.Signal , value=>
                {
                    rowNo = value.ToInt();
                    if (rowNo<1)
                    {
                        throw logger.Error(new ModelException(this, $"row number should be at least one but {rowNo} received"));
                    }
                }, ()=>{})}
            };
        }
        private List<double> Interpolate(double val)
        {
            List<double> before = table.Data.FindLast(row => row[0] < val);
            List<double> after = table.Data.Find(row => row[0] > val);
            List<double> current = table.Data.Find(row => row[0] == val);
            if (current != null)
            {
                return current;
            }
            else if (before != null && after != null)
            {
                // Results.Add(before.Data+(after.Data-before.Data)/(after.Time-before.Time)*(time-before.Time));
                double tb = before[0];
                double ta = after[0];
                return before.Zip(after, (b, a) => b + (a - b) / (ta - tb) * (val - tb)).ToList();
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
                throw logger.Error(new ModelException(this, $"Invalid interpolation input at value {val}"));
            }
        }
        protected override List<Matrix<double>> Calculate(List<Matrix<double>> values)
        {
            double inputVal = values[0].ToScalar();
            List<double> interpolationVal = Interpolate(inputVal);
            if (colNo * rowNo == interpolationVal.Count - 1)
            {
                MatrixBuilder<double> m = Matrix<double>.Build;
                Results[0] = m.Dense(colNo, rowNo, interpolationVal.Skip(1).ToArray()).Transpose();
            }
            else
            {
                throw logger.Error(new ModelException(this, $"Inconsistency of row, col and playback"));
            }
            return Results;
        }
    }
}