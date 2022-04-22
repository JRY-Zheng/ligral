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
        private int rowNo = 1;
        private int colNo = 1;
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
                    if (table.ColumnCount < 2 || table.GetColumnName(0) != "Time")
                    {
                        throw logger.Error(new ModelException(this,"Invalid playback file"));
                    }
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
        protected override List<Matrix<double>> Calculate(List<Matrix<double>> values)
        {
            List<double> playback = table.ColumnInterpolate(Solver.Time);
            if (colNo * rowNo == playback.Count - 1)
            {
                MatrixBuilder<double> m = Matrix<double>.Build;
                Results[0] = m.Dense(colNo, rowNo, playback.Skip(1).ToArray()).Transpose();
            }
            else
            {
                throw logger.Error(new ModelException(this, $"Inconsistency of row, col and playback"));
            }
            return Results;
        }
    }
}