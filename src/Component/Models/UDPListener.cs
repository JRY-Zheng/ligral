/* Copyright (C) 2019-2021 Junruoyu Zheng. Home page: https://junruoyu-zheng.gitee.io/ligral

   Distributed under MIT license.
   See file LICENSE for detail or copy at https://opensource.org/licenses/MIT
*/

using System.Collections.Generic;
using System.Linq;
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
        private List<string> names;
        private List<ControlInputHandle> handles;
        protected override void SetUpParameters()
        {
            Parameters = new ParameterDictionary()
            {
                {"size", new Parameter(ParameterType.Signal , value=>
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
                {"name", new Parameter(ParameterType.String , value=>
                {
                    names = new List<string>(value.ToString().Split(";"));
                    names.ForEach(s => s.Trim());
                }, ()=>{})}
            };
        }
        public override void Check()
        {
            if (names is null)
            {
                names = OutPortList.Select((_, i) => Name+i.ToString()).ToList();
            }
            else if (names.Count != OutPortCount())
            {
                throw logger.Error(new ModelException(this, $"cannot match name info {names.Count} to out port count {OutPortCount()}"));
            }
            if (size is null)
            {
                for (int i=0; i<size.Count; i++)
                {
                    var handle = ControlInput.CreateInput(names[i], 1, 1);
                    handles.Add(handle);
                    OutPortList[i].SetShape(1, 1);
                }
            }
            else if (size.Count != OutPortCount())
            {
                throw logger.Error(new ModelException(this, $"cannot match size info {size.Count} to out port count {OutPortCount()}"));
            }
            else
            {
                for (int i=0; i<size.Count; i++)
                {
                    var handle = ControlInput.CreateInput(names[i], size[i].Item1, size[i].Item2);
                    handles.Add(handle);
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