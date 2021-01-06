/* Copyright (C) 2019-2021 Junruoyu Zheng. Home page: https://junruoyu-zheng.gitee.io/ligral

   Distributed under MIT license.
   See file LICENSE for detail or copy at https://opensource.org/licenses/MIT
*/

using MathNet.Numerics.LinearAlgebra;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Ligral.Component
{
    partial class Signal : IEnumerable<double>, IEnumerator<double>, System.IComparable<Signal>
    {
        public string Name
        {
            get 
            {
                if (outPort == null)
                {
                    return null;
                }
                else
                {
                    return outPort.SignalName;
                }
            }
        }
        protected Logger loggerInstance;
        private static Logger signalLogger = new Logger("Signal");
        protected Logger logger
        {
            get
            {
                if (loggerInstance is null)
                {
                    loggerInstance = new Logger(Name);
                }
                return loggerInstance;
            }
        }
        private OutPort outPort;
        private double doubleValue = 0;
        private Matrix<double> matrixValue;
        public bool IsMatrix {get; private set;} = false;
        public bool Packed {get; private set;} = false;
        private int currentRow = 0;
        private int currentCol = -1;
        public double Current 
        {
            get
            {
                if (!Packed)
                {
                    throw logger.Error(new LigralException("Signal is unpacked"));
                }
                if (IsMatrix)
                {
                    return matrixValue[currentRow, currentCol];
                }
                else
                {
                    return doubleValue;
                }
            }
        }

        object IEnumerator.Current => throw new System.NotImplementedException();

        public Signal(OutPort port = null)
        {
            outPort = port;
        }
        public Signal(object val, OutPort port = null) : this(port)
        {
            Pack(val);
        }
        public Signal(Matrix<double> val, OutPort port = null) : this(port)
        {
            Pack(val);
        }
        public Signal(double val, OutPort port = null) : this(port)
        {
            Pack(val);
        }
        public void Pack(object val) 
        {
            matrixValue = val as Matrix<double>;
            IsMatrix = matrixValue != null;
            if (!IsMatrix)
            {
                doubleValue = System.Convert.ToDouble(val);
            }   
            Packed = true;
        }
        public void Pack(Matrix<double> matrix)
        {
            matrixValue = matrix;
            IsMatrix = true;
            Packed = true;
        }
        public void Pack(double value)
        {
            doubleValue = value;
            IsMatrix = false;
            Packed = true;
        }
        public object Unpack() 
        {
            if (IsMatrix)
            {
                return matrixValue;
            }
            else
            {
                return doubleValue;
            }
        }
        public void Clone(Signal signal)
        {
            Pack(signal.Unpack());
        }
        public IEnumerator<double> GetEnumerator()
        {
            return this;
        }
        public bool MoveNext()
        {
            if (IsMatrix)
            {
                currentCol ++;
                if (currentCol >= matrixValue.ColumnCount)
                {
                    currentCol = 0;
                    currentRow ++;
                }
                if (currentRow >= matrixValue.RowCount)
                {
                    Reset();
                    return false;
                }
                return true;
            }
            else
            {
                currentCol++;
                if (currentCol > 0)
                {
                    Reset();
                    return false;
                }
                else
                {
                    return true;
                }
            }
        }
        public void Reset()
        {
            currentCol = -1;
            currentRow = 0;
        }
        public Signal Apply(System.Func<double, double> func)
        {
            if (IsMatrix)
            {
                MatrixBuilder<double> m = Matrix<double>.Build;
                Matrix<double> matrix = matrixValue.Map(func);
                return new Signal(matrix);
            }
            else
            {
                return new Signal(func(doubleValue));
            }
        }
        public Signal ZipApply(Signal signal, System.Func<double, double, double> func)
        {
            if (IsMatrix && signal.IsMatrix)
            {
                Matrix<double> other = signal.Unpack() as Matrix<double>;
                Matrix<double> result = matrixValue.Map2(func, other);
                return new Signal(result);
            }
            else if (IsMatrix && !signal.IsMatrix)
            {
                double other = (double) signal.Unpack();
                Matrix<double> result = matrixValue.Map((item)=>func(item, other));
                return new Signal(result);
            }
            else if (!IsMatrix && signal.IsMatrix)
            {
                return signal.ZipApply(this, (first, second)=>func(second, first));
            }
            else
            {
                double other = (double) signal.Unpack();
                return new Signal(func(doubleValue, other));
            }
        }
        public void ZipApply<TOther>(IEnumerable<TOther> other, System.Action<double, TOther> action)
        {
            Reset();
            if (IsMatrix)
            {
                foreach ((var otherItem, var val) in other.Zip<TOther, double>(this))
                {
                    action(val, otherItem);   
                }             
            }
            else
            {
                // other.ForEach(otherItem => action(doubleValue, otherItem));
                foreach (var otherItem in other)
                {
                    action(doubleValue, otherItem);
                }
            }
        }
        public List<TResult> ZipApply<TOther, TResult>(List<TOther> other, System.Func<double, TOther, TResult> func)
        {
            Reset();
            if (IsMatrix)
            {
                return other.Zip<TOther, double>(this).ToList()
                    .ConvertAll(pair => func(pair.Second, pair.First));                
            }
            else
            {
                return other.ConvertAll(otherItem => func(doubleValue, otherItem));
            }
        }
        public string ToStringInLine()
        {
            if (IsMatrix)
            {
                return $"({matrixValue.RowCount}x{matrixValue.ColumnCount})  " + string.Join(" ", ToList());
            }
            else
            {
                return doubleValue.ToString();
            }
        }
        public List<double> ToList()
        {
            Reset();
            List<double> list = new List<double>(this);
            return list;
        }
        public (int, int) Shape()
        {
            if (IsMatrix)
                return (matrixValue.RowCount, matrixValue.ColumnCount);
            else
                return (0, 0);
        }
        public bool CheckShape(int rowNo, int colNo)
        {
            if (IsMatrix)
                return rowNo == matrixValue.RowCount && colNo == matrixValue.ColumnCount;
            else
                return rowNo == 0 && colNo == 0;
        }
        public int Count()
        {
            if (IsMatrix)
                return matrixValue.RowCount * matrixValue.ColumnCount;
            else
                return 1;
        }
        IEnumerator IEnumerable.GetEnumerator()
        {
            throw new System.NotImplementedException();
        }

        public void Dispose()
        {
            
        }
        private int Compare(double a, double b, double err=1e-5)
        {
            if (a - b > err)
            {
                return 1;
            }
            else if (b - a > err)
            {
                return -1;
            }
            else
            {
                return 0;
            }
        }
        public int CompareTo(Signal signal)
        {
            if (signal.IsMatrix && IsMatrix)
            {
                Matrix<double> matrix = signal.Unpack() as Matrix<double>;
                return Compare(matrixValue.Trace(), matrix.Trace());
            }
            else if (!signal.IsMatrix && !IsMatrix)
            {
                double value = (double) signal.Unpack();
                return Compare(doubleValue, value);
            }
            else
            {
                throw logger.Error(new LigralException("Double cannot be compared with matrix"));
            }
        }
    }
}