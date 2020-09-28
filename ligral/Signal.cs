using MathNet.Numerics.LinearAlgebra;

namespace Ligral
{
    class Signal
    {
        protected static int id = 0;
        public string Name;
        protected Signal()
        {
            id += 1;
            Name = GetType().Name + id.ToString();
        }
        public virtual object GetValue() 
        {
            return null;
        }
    }

    class DoubleSignal : Signal
    {
        private double value = 0;
        public override object GetValue()
        {
            return value;
        }
    }

    class MatrixSignal : Signal
    {
        private Matrix<double> value;
        public override object GetValue()
        {
            if (value==null)
            {
                throw new LigralException($"Signal {Name} is null while read");
            }
            else
            {
                return value;
            }
        }
    }
}