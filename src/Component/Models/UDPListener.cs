/* Copyright (C) 2019-2021 Junruoyu Zheng. Home page: https://junruoyu-zheng.gitee.io/ligral

   Distributed under MIT license.
   See file LICENSE for detail or copy at https://opensource.org/licenses/MIT
*/

using System.Collections.Generic;
using MathNet.Numerics.LinearAlgebra;
using ParameterDictionary = System.Collections.Generic.Dictionary<string, Ligral.Component.Parameter>;
using Ligral.Simulation;

namespace Ligral.Component.Models
{
    class UDPListener : OutPortVariableModel
    {
        protected override string DocString
        {
            get
            {
                return "This model listens udp data as input";
            }
        }
        private List<(int, int)> size;
        private List<ControlInputHandle> handles;
        protected override void SetUpParameters()
        {
            Parameters = new ParameterDictionary()
            {
                {"size", new Parameter(ParameterType.String , value=>
                {
                    size = new List<(int, int)>();
                    Matrix<double> sizeMat;
                    try
                    {
                        sizeMat = value.ToMatrix();
                        if (sizeMat.ColumnCount != 2)
                        {
                            throw logger.Error(new ModelException(this, $"size must be a nx2 int matrix, but column number is {sizeMat.ColumnCount}"));
                        }
                        foreach (var row in sizeMat.ToRowArrays())
                        {
                            size.Add((row[0].ToInt(), row[1].ToInt()));
                        }
                    }
                    catch (System.ArgumentException e)
                    {
                        throw logger.Error(new ModelException(this, $"size must be a nx2 int matrix, but {e.Message}"));
                    }
                }, ()=>{})},
            };
        }
        public override void Check()
        {
            if (size is null)
            {
                foreach (OutPort outPort in OutPortList)
                {
                    outPort.SetShape(1, 1);
                }
            }
            else if (size.Count != OutPortList.Count)
            {
                throw logger.Error(new ModelException(this, $"cannot match size info {size.Count} to out port count {OutPortList.Count}"));
            }
            else
            {
                for (int i=0; i<size.Count; i++)
                {
                    OutPortList[i].SetShape(size[i].Item1, size[i].Item2);
                }
            }
        }
        protected override List<Matrix<double>> Calculate(List<Matrix<double>> values)
        {
            throw new System.NotImplementedException();
        }
    }
}