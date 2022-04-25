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
    class InterpolationHD : Model
    {
        protected override string DocString
        {
            get
            {
                return "This model generates an interpolation of the given n inputs.";
            }
        }
        private Matrix<double> table;
        private Matrix<double> axesMat;
        private List<List<double>> axes;
        private List<int> dimension;
        private List<int> indicesList;
        public override void Check()
        {
            if (table == null)
            {
                throw logger.Error(new ModelException(this, $"Interpolation table undefined"));
            }
            if (table.ColumnCount != dimension.Last())
            {
                throw logger.Error(new ModelException(this, $"Table column count should match the last dimension"));
            }
            if (axesMat.RowCount != dimension.Count || axesMat.ColumnCount < dimension.Max())
            {
                throw logger.Error(new ModelException(this, "Axes should be nxm matrix where m >= largest dimension"));
            }
            else
            {
                axes = new List<List<double>>();
                for (int i=0; i<dimension.Count; i++)
                {
                    axes.Add(axesMat.Row(i).Take(dimension[i]).ToList());
                }
            }
            if (InPortList[0].RowNo != 1 || InPortList[0].ColNo != dimension.Count)
            {
                throw logger.Error(new ModelException(this, "The input should be a 1xn vector that matches dimension"));
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
                    table = new Storage((string)value, false).ToMatrix();
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
                    table = value.ToMatrix();
                    if (table.ColumnCount < 2)
                    {
                        throw logger.Error(new ModelException(this,"Invalid interpolation file"));
                    }
                }, ()=>{})},
                {"axes", new Parameter(ParameterType.Signal , value=>
                {
                    axesMat = value.ToMatrix();
                })},
                {"dim", new Parameter(ParameterType.Signal , value=>
                {
                    var mat = value.ToMatrix();
                    if (mat.RowCount != 1)
                    {
                        throw logger.Error(new ModelException(this,"Dimension should be 1xn vector"));
                    }
                    try
                    {
                        dimension= mat.Row(0).Select((d, i) => d.ToInt()).ToList();
                        indicesList = new List<int>();
                        int size = 1;
                        // the last dimension is in column span
                        foreach (var dim in dimension.Reverse<int>().Skip(1))
                        {
                            indicesList.Insert(0, size);
                            size *= dim;
                        }
                    }
                    catch (ArgumentException e)
                    {
                        throw logger.Error(new ModelException(this,$"Dimension should be all integer but {e.Message}"));
                    }
                })},
            };
        }
        private double GetValue(IEnumerable<int> indices)
        {
            if (indices.Count() != dimension.Count)
            {
                throw logger.Error(new ModelException(this,$"Indices should be 1xn vector"));
            }
            int rowIndex = indices.Take(indices.Count()-1).Zip(indicesList, (i, a) => i*a).Sum();
            return table[rowIndex, indices.Last()];
        }
        private double Interpolate(List<double> value)
        {
            var index = axes.Select((axis, i) => (axis.FindLastIndex(t => t < value[i]), axis.FindIndex(t => t >= value[i]))).ToList();
            for (int i=0; i<index.Count; i++)
            {
                if (index[i].Item1 < 0)
                {
                    index[i] = (0, 1);
                }
                else if (index[i].Item2 < 0)
                {
                    index[i] = (axes[i].Count - 2, axes[i].Count - 1);
                }
            }
            var ratio = index.Select((idx, i) => {
                double left = axes[i][idx.Item1];
                double right = axes[i][idx.Item2];
                return (value[i]-left)/(right-left);
            }).ToList();
            List<double> coeffs = new List<double>();
            for (int i=0; i<Math.Pow(2, ratio.Count); i++)
            {
                double c = 1;
                List<int> idx = new List<int>();
                for (int j=0; j<ratio.Count; j++)
                {
                    if ((i>>j)%2==0)
                    {
                        c *= ratio[j];
                        idx.Add(index[j].Item2);
                    }
                    else
                    {
                        c *= 1-ratio[j];
                        idx.Add(index[j].Item1);
                    }
                }
                double val = GetValue(idx);
                coeffs.Add(c*val);
            }
            return coeffs.Sum();
        }
        protected override List<Matrix<double>> Calculate(List<Matrix<double>> values)
        {
            var val = values[0].Row(0).ToList();
            double interpolationVal = Interpolate(val);
            MatrixBuilder<double> m = Matrix<double>.Build;
            Results[0] = m.Dense(1, 1, interpolationVal);
            return Results;
        }
    }
}