/* Copyright (C) 2019-2022 Junruoyu Zheng. Home page: https://junruoyu-zheng.gitee.io/ligral

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
using Ligral.Syntax.CodeASTs;

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
        private int skip = 0;
        protected override void SetUpPorts()
        {
            OutPortList.Add(new OutPort("source", this));
        }
        public override void Check()
        {
            if (table == null)
            {
                throw logger.Error(new ModelException(this, $"Interpolation table undefined"));
            }
            if (colNo * rowNo > table.ColumnCount - 1 - skip)
            {
                throw logger.Error(new ModelException(this, $"Not enough data in playback"));
            }
            OutPortList[0].SetShape(rowNo, colNo);
        }
        protected override void SetUpParameters()
        {
            Parameters = new ParameterDictionary()
            {
                {"file", new Parameter(ParameterType.String , value=>
                {
                    if (table != null)
                    {
                        throw logger.Error(new ModelException(this, "Playback table redefined"));
                    }
                    table = new Storage((string)value, true);
                    if (table.ColumnCount < 2 || table.GetColumnName(0).ToLower() != "time")
                    {
                        throw logger.Error(new ModelException(this,"Invalid playback file"));
                    }
                }, ()=>{})},
                {"table", new Parameter(ParameterType.Signal , value=>
                {
                    if (table != null)
                    {
                        throw logger.Error(new ModelException(this, "Playback table redefined"));
                    }
                    table = new Storage(value.ToMatrix());
                    if (table.ColumnCount < 2)
                    {
                        throw logger.Error(new ModelException(this,"Playback interpolation file"));
                    }
                }, ()=>{})},
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
                }, ()=>{})},
                {"skip", new Parameter(ParameterType.Signal , value=>
                {
                    skip = value.ToInt();
                    if (skip<0)
                    {
                        throw logger.Error(new ModelException(this, $"skip number should be at least 0 but {skip} received"));
                    }
                }, ()=>{})}
            };
        }
        protected override List<Matrix<double>> Calculate(List<Matrix<double>> values)
        {
            List<double> playback = table.ColumnInterpolate(Solver.Time, skip, colNo*rowNo);
            MatrixBuilder<double> m = Matrix<double>.Build;
            Results[0] = m.Dense(colNo, rowNo, playback.ToArray()).Transpose();
            return Results;
        }
        public override List<int> GetCharacterSize()
        {
            return new List<int>() {rowNo, colNo, table.Data.Count};
        }
        public override List<CodeAST> ConstructConfigurationAST()
        {
            var codeASTs = new List<CodeAST>();
            AssignCodeAST ctxAST = new AssignCodeAST();
            ctxAST.Destination = $"{GlobalName}.ctx";
            ctxAST.Source = "&ctx";
            codeASTs.Add(ctxAST);
            LShiftCodeAST timeAST = new LShiftCodeAST();
            timeAST.Destination = $"{GlobalName}.time";
            timeAST.Source = string.Join(',', table.GetColumn(0));
            codeASTs.Add(timeAST);
            LShiftCodeAST tableAST = new LShiftCodeAST();
            tableAST.Destination = $"{GlobalName}.table";
            tableAST.Source = string.Join(",\n\t\t", table.Data.ConvertAll(line =>
                string.Join(',', line.Skip(1+skip).Take(rowNo*colNo))));
            codeASTs.Add(tableAST);
            return codeASTs;
        }
    }
}