/* Copyright (C) 2019-2021 Junruoyu Zheng. Home page: https://junruoyu-zheng.gitee.io/ligral

   Distributed under MIT license.
   See file LICENSE for detail or copy at https://opensource.org/licenses/MIT
*/

using System.Collections.Generic;
using System;
using System.Linq;
using Ligral.Tools;
using MathNet.Numerics.LinearAlgebra;
using Ligral.Component;

namespace Ligral.Simulation
{
    public class Handle<T>
    {
        
        public int rowNo {get; private set;}
        public int colNo {get; private set;}
        public List<T> space {get;} = new List<T>();
        private string name;
        private Logger logger;
        public Handle(string name, int rowNo, int colNo, Func<string, T> create)
        {
            this.name = name;
            logger = new Logger(name);
            if (rowNo < 0 || colNo < 0)
            {
                throw logger.Error(new LigralException($"Invalid shape {rowNo}x{colNo} in {name}"));
            }
            this.rowNo = rowNo;
            this.colNo = colNo;
            if ((rowNo == 0 && colNo == 0) || (rowNo == 1 && colNo == 1))
            {
                space.Add(create(name));
            }
            else if (colNo == 1)
            {
                for (int j = 0; j < rowNo; j++)
                {
                    space.Add(create($"{name}({j})"));
                }
            }
            else if (rowNo == 1)
            {
                for (int j = 0; j < colNo; j++)
                {
                    space.Add(create($"{name}({j})"));
                }
            }
            else
            {
                for(int i = 0; i < rowNo; i++)
                {
                    for (int j = 0; j < colNo; j++)
                    {
                        space.Add(create($"{name}({i}-{j})"));
                    }
                }
            }
        }
        public void SetSignal(Signal signal, Action<T, double> setValue)
        {
            if (!signal.CheckShape(rowNo, colNo))
            {
                throw logger.Error(new LigralException($"Inconsistent shape {rowNo}x{colNo} in {name}, {signal.Shape()} expected."));
            }
            signal.ZipApply<T>(space, (value, room) => setValue(room, value));
        }
        public Signal GetSignal(Func<T, double> getValue)
        {
            Signal signal = new Signal();
            if (rowNo==0 && colNo==0)
            {
                signal.Pack(getValue(space[0]));
            }
            else
            {
                IEnumerable<double> row = space.ConvertAll(room => getValue(room));
                MatrixBuilder<double> m = Matrix<double>.Build;
                Matrix<double> mat = m.DenseOfRowMajor(1, colNo, row.Take(colNo));
                for (int i = 1; i < rowNo; i++)
                {
                    row = row.Skip(colNo);
                    Matrix<double> vec = m.DenseOfRowMajor(1, colNo, row.Take(colNo));
                    mat = mat.Stack(vec);
                }
                signal.Pack(mat);
            }
            return signal;
        }
    }
}