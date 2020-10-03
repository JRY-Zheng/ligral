using MathNet.Numerics.LinearAlgebra;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Ligral
{
    class Signal : IEnumerable<double>, IEnumerator<double>
    {
        protected static int id = 0;
        public string Name;
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
                    throw new LigralException("Signal is unpacked");
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

        public Signal()
        {
            id += 1;
            Name = GetType().Name + id.ToString();
        }
        public Signal(object val) : this()
        {
            Pack(val);
        }
        public Signal(Matrix<double> val) : this()
        {
            Pack(val);
        }
        public Signal(double val) : this()
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
                return false;
            }
        }
        public void Reset()
        {
            currentCol = -1;
            currentRow = 0;
        }
        public static Signal operator+(Signal left, Signal right)
        {
            if (left.IsMatrix)
            {
                return (left.Unpack() as Matrix<double>)+right;
            }
            else
            {
                return ((double) left.Unpack())+right;
            }
        }
        public static Signal operator+(Matrix<double> left, Signal right)
        {
            if (right.IsMatrix)
            {
                return new Signal(left+(right.Unpack() as Matrix<double>));
            }
            else
            {
                return new Signal(left+((double) right.Unpack()));
            }
        }
        public static Signal operator+(double left, Signal right)
        {
            if (right.IsMatrix)
            {
                return new Signal(left+(right.Unpack() as Matrix<double>));
            }
            else
            {
                return new Signal(left+((double) right.Unpack()));
            }
        }
        public static Signal operator-(Signal left, Signal right)
        {
            if (left.IsMatrix)
            {
                return (left.Unpack() as Matrix<double>)-right;
            }
            else
            {
                return ((double) left.Unpack())-right;
            }
        }
        public static Signal operator-(Matrix<double> left, Signal right)
        {
            if (right.IsMatrix)
            {
                return new Signal(left-(right.Unpack() as Matrix<double>));
            }
            else
            {
                return new Signal(left-((double) right.Unpack()));
            }
        }
        public static Signal operator-(double left, Signal right)
        {
            if (right.IsMatrix)
            {
                return new Signal(left-(right.Unpack() as Matrix<double>));
            }
            else
            {
                return new Signal(left-((double) right.Unpack()));
            }
        }
        public static Signal operator*(Signal left, Signal right)
        {
            if (left.IsMatrix)
            {
                return (left.Unpack() as Matrix<double>)*right;
            }
            else
            {
                return ((double) left.Unpack())*right;
            }
        }
        public static Signal operator*(Matrix<double> left, Signal right)
        {
            if (right.IsMatrix)
            {
                return new Signal(left*(right.Unpack() as Matrix<double>));
            }
            else
            {
                return new Signal(left*((double) right.Unpack()));
            }
        }
        public static Signal operator*(double left, Signal right)
        {
            if (right.IsMatrix)
            {
                return new Signal(left*(right.Unpack() as Matrix<double>));
            }
            else
            {
                return new Signal(left*((double) right.Unpack()));
            }
        }
        public static Signal operator&(Signal left, Signal right)
        {
            if (left.IsMatrix)
            {
                return (left.Unpack() as Matrix<double>)&right;
            }
            else
            {
                return ((double) left.Unpack())&right;
            }
        }
        public static Signal operator&(Matrix<double> left, Signal right)
        {
            if (right.IsMatrix)
            {
                Matrix<double> rightMatrix = right.Unpack() as Matrix<double>;
                MatrixBuilder<double> m = Matrix<double>.Build;
                Matrix<double> result = m.DenseOfMatrix(rightMatrix);
                left.Map2((l, r)=>l*r, rightMatrix, result);
                return new Signal(result);
            }
            else
            {
                return new Signal(left*((double) right.Unpack()));
            }
        }
        public static Signal operator&(double left, Signal right)
        {
            if (right.IsMatrix)
            {
                return new Signal(left*(right.Unpack() as Matrix<double>));
            }
            else
            {
                return new Signal(left*((double) right.Unpack()));
            }
        }
        public static Signal operator/(Signal left, Signal right)
        {
            if (left.IsMatrix)
            {
                return (left.Unpack() as Matrix<double>)/right;
            }
            else
            {
                return ((double) left.Unpack())/right;
            }
        }
        public static Signal operator/(Matrix<double> left, Signal right)
        {
            if (right.IsMatrix)
            {
                return new Signal(left*(right.Unpack() as Matrix<double>).Inverse());
            }
            else
            {
                return new Signal(left/((double) right.Unpack()));
            }
        }
        public static Signal operator/(double left, Signal right)
        {
            if (right.IsMatrix)
            {
                return new Signal(left/(right.Unpack() as Matrix<double>));
            }
            else
            {
                return new Signal(left/((double) right.Unpack()));
            }
        }
        public static Signal operator^(Signal left, Signal right)
        {
            if (left.IsMatrix)
            {
                return (left.Unpack() as Matrix<double>)^right;
            }
            else
            {
                return ((double) left.Unpack())^right;
            }
        }
        public static Signal operator^(Matrix<double> left, Signal right)
        {
            if (right.IsMatrix)
            {
                throw new LigralException("Matrix power matrix base is not supported.");
            }
            else
            {
                int index = System.Convert.ToInt32(right.Unpack());
                MatrixBuilder<double> m = Matrix<double>.Build;
                Matrix<double> result = m.DenseOfMatrix(left);
                left.Power(index, result);
                return new Signal(result);
            }
        }
        public static Signal operator^(double left, Signal right)
        {
            if (right.IsMatrix)
            {
                Matrix<double> rightMatrix = right.Unpack() as Matrix<double>;
                MatrixBuilder<double> m = Matrix<double>.Build;
                Matrix<double> result = m.DenseOfMatrix(rightMatrix);
                // [TODO] QR decomposition
                return new Signal(left/(right.Unpack() as Matrix<double>));
            }
            else
            {
                double rightValue = (double) right.Unpack();
                return new Signal(System.Math.Pow(left, rightValue));
            }
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

        IEnumerator IEnumerable.GetEnumerator()
        {
            throw new System.NotImplementedException();
        }

        public void Dispose()
        {
            
        }
    }
}