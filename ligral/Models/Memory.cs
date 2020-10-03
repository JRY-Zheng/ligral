using System.Collections.Generic;
using ParameterDictionary = System.Collections.Generic.Dictionary<string, Ligral.Parameter>;
using System;
using MathNet.Numerics.LinearAlgebra;

namespace Ligral.Models
{
    class Memory : Model
    {
        protected override string DocString
        {
            get
            {
                return "This model outputs its input from the previous time step.";
            }
        }
        private Signal initial = new Signal();
        private List<Signal> stack = new List<Signal>();
        private int colNo = 0;
        private int rowNo = 0;
        protected override void SetUpPorts()
        {
            base.SetUpPorts();
            Initializeable = true;
        }
        protected override void SetUpParameters()
        {
            Parameters = new ParameterDictionary()
            {
                {"initial", new Parameter(value=>
                {
                    initial.Pack(value);
                }, ()=>{})},
                {"delay", new Parameter(value=>
                {
                    int delayedFrame = Convert.ToInt32(value);
                    if (delayedFrame < 1)
                    {
                        throw new ModelException(this, "Delay should be greater than 1");
                    }
                    for (int i = 0; i <= delayedFrame; i++)
                    {
                        stack.Add(new Signal());
                    }
                }, ()=>
                {
                    stack.Add(new Signal());
                    stack.Add(new Signal());
                })},
                {"col", new Parameter(value=>
                {
                    colNo = Convert.ToInt32(value);
                }, ()=>{})},
                {"row", new Parameter(value=>
                {
                    rowNo = Convert.ToInt32(value);
                }, ()=>{})}
            };
        }
        protected override void AfterConfigured()
        {
            if (colNo > 0 && rowNo > 0)
            {
                if (initial.Packed && !initial.IsMatrix)
                {
                    throw new ModelException(this, $"Inconsistency between initial value and shape");
                }
                Matrix<double> initialMatrix = initial.Unpack() as Matrix<double>;
                if (initialMatrix == null) // unpacked
                {
                    MatrixBuilder<double> m = Matrix<double>.Build;
                    initial.Pack(m.Dense(rowNo, colNo, 0));
                }
                else
                {
                    if (colNo != initialMatrix.ColumnCount || rowNo != initialMatrix.RowCount)
                    {
                        throw new ModelException(this, $"Inconsistency between initial value and shape");
                    }
                }
            }
            else if (colNo == 0 && rowNo == 0)
            {
                if (!initial.Packed)
                {
                    initial.Pack(0);
                }
            }
            else
            {
                throw new ModelException(this, $"Matrix row and col should be positive non-zero: {colNo}x{rowNo}");
            }
            stack.ForEach(signal => signal.Clone(initial));
        }
        public override void Initialize()
        {
            base.Initialize();
            stack.RemoveAt(0);
        }
        protected override List<Signal> Calculate(List<Signal> values)
        {
            // Results.Clear();
            Signal inputSignal = values[0];
            Signal outputSignal = Results[0];
            Signal stackTop = stack[0];
            stack.Remove(stackTop);
            stackTop.Clone(inputSignal);
            stack.Add(stackTop);
            Signal newStackTop = stack[0];
            outputSignal.Clone(newStackTop);
            return Results;
        }
    }
}