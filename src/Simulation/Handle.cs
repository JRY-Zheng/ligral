/* Copyright (C) 2019-2022 Junruoyu Zheng. Home page: https://junruoyu-zheng.gitee.io/ligral

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
        protected Logger logger;
        public Handle(string name, int rowNo, int colNo, Func<string, T> create)
        {
            this.name = name;
            logger = new Logger(name);
            if (rowNo <= 0 || colNo <= 0)
            {
                throw logger.Error(new LigralException($"Invalid shape {rowNo}x{colNo} in {name}"));
            }
            this.rowNo = rowNo;
            this.colNo = colNo;
            if (rowNo == 1 && colNo == 1)
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
        public void SetSignal(Matrix<double> matrix, Action<T, double> setValue)
        {
            if (!matrix.CheckShape(rowNo, colNo))
            {
                throw logger.Error(new LigralException($"Inconsistent shape {matrix.ShapeString()} in {name}, ({rowNo}, {colNo}) expected."));
            }
            matrix.Apply2<T>(space, (value, room) => setValue(room, value));
        }
        public Matrix<double> GetSignal(Func<T, double> getValue)
        {
            IEnumerable<double> row = space.ConvertAll(room => getValue(room));
            MatrixBuilder<double> m = Matrix<double>.Build;
            Matrix<double> matrix = m.Dense(colNo, rowNo, row.ToArray()).Transpose();
            return matrix;
        }
    }
}