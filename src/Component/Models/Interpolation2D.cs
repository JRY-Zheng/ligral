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
    class Interpolation2D : Model
    {
        protected override string DocString
        {
            get
            {
                return "This model generates an interpolation of the given 2 inputs.";
            }
        }
        private Storage table;
        protected override void SetUpPorts()
        {
            InPortList.Add(new InPort("colval", this));
            InPortList.Add(new InPort("rowval", this));
            OutPortList.Add(new OutPort("output", this));
        }
        public override void Check()
        {
            if (table == null)
            {
                throw logger.Error(new ModelException(this, $"Interpolation table undefined"));
            }
            if (InPortList[0].RowNo != 1 || InPortList[0].ColNo != 1 && InPortList[1].RowNo != 1 || InPortList[1].ColNo != 1)
            {
                throw logger.Error(new ModelException(this, "Interpolation2D only accept scalar inputs"));
            }
            OutPortList[0].SetShape(1, 1);
        }
        protected override void SetUpParameters()
        {
            Parameters = new ParameterDictionary()
            {
                {"file", new Parameter(ParameterType.String , value=>
                {
                    if (table != null)
                    {
                        throw logger.Error(new ModelException(this, "Interpolation table redefined"));
                    }
                    table = new Storage((string)value, false);
                    if (table.ColumnCount < 2)
                    {
                        throw logger.Error(new ModelException(this,"Invalid interpolation file"));
                    }
                }, ()=>{})},
                {"table", new Parameter(ParameterType.Signal , value=>
                {
                    if (table != null)
                    {
                        throw logger.Error(new ModelException(this, "Interpolation table redefined"));
                    }
                    table = new Storage(value.ToMatrix());
                    if (table.ColumnCount < 2)
                    {
                        throw logger.Error(new ModelException(this,"Invalid interpolation file"));
                    }
                }, ()=>{})},
            };
        }
        protected override List<Matrix<double>> Calculate(List<Matrix<double>> values)
        {
            double colVal = values[0].ToScalar();
            double rowVal = values[1].ToScalar();
            double interpolationVal = table.Interpolate2D(colVal, rowVal);
            MatrixBuilder<double> m = Matrix<double>.Build;
            Results[0] = m.Dense(1, 1, interpolationVal);
            return Results;
        }
    }
}