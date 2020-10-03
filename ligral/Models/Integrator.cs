using System.Collections.Generic;
using ParameterDictionary = System.Collections.Generic.Dictionary<string, Ligral.Parameter>;
using System;
using MathNet.Numerics.LinearAlgebra;

namespace Ligral.Models
{
    class Integrator : Model
    {
        protected override string DocString
        {
            get
            {
                return "This model outputs the value of the integral of its input signal with respect to time.";
            }
        }
        // private double lastTime = 0;
        protected List<State> states = new List<State>();
        protected Signal initial = new Signal();
        protected bool isMatrix {get {return initial.IsMatrix;}}
        protected int colNo = 0;
        protected int rowNo = 0;
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
                    Results[0].Clone(initial);
                }, ()=>{})},
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
                    initialMatrix = m.Dense(rowNo, colNo, 0);
                    initial.Pack(initialMatrix);
                    Results[0].Clone(initial);
                }
                else
                {
                    if (colNo != initialMatrix.ColumnCount || rowNo != initialMatrix.RowCount)
                    {
                        throw new ModelException(this, $"Inconsistency between initial value and shape");
                    }
                }
                foreach (double initialValue in initialMatrix.Transpose().ToArray())
                {
                    State state = State.CreateState(initialValue, $"{Name}{states.Count+1}");
                    state.Config(1e-5, 10);
                    state.DerivativeReceived += s=>{};
                    states.Add(state);
                }
            }
            else if (colNo == 0 && rowNo == 0)
            {
                if (!initial.Packed)
                {
                    initial.Pack(0);
                    Results[0].Clone(initial);
                }
                else if (initial.IsMatrix)
                {
                    Matrix<double> matrix = initial.Unpack() as Matrix<double>;
                    colNo = matrix.ColumnCount;
                    rowNo = matrix.RowCount;
                    AfterConfigured();
                    return;
                }
                State state = State.CreateState((double) initial.Unpack(), Name);
                state.Config(1e-5, 10);
                state.DerivativeReceived += s=>{};
                states.Add(state);
            }
            else
            {
                throw new ModelException(this, $"Matrix row and col should be positive non-zero: {colNo}x{rowNo}");
            }
        }
        // protected override void AfterConfigured()
        // {
        //     state = State.CreateState(initial);
        //     state.Config(1e-5, 10);
        //     state.DerivativeReceived += (state) => { };
        // }
        protected override List<Signal> Calculate(List<Signal> values)
        {
            // Results[0] += values[0]*(time-lastTime);
            // lastTime = time;
            Signal inputSignal = values[0];
            Signal outputSignal = Results[0];
            if (isMatrix && inputSignal.IsMatrix)
            {
                inputSignal.ZipApply<State, int>(states, (deriv, state) => {
                    StateCalculate(state, deriv);
                    return 0;
                });
                MatrixBuilder<double> m = Matrix<double>.Build;
                Matrix<double> matrix = m.Dense(colNo, rowNo, states.ConvertAll(state => state.StateVariable).ToArray()).Transpose();
                outputSignal.Pack(matrix);
            }
            else if (!isMatrix && !inputSignal.IsMatrix)
            {
                State state = states[0];
                StateCalculate(state, (double) inputSignal.Unpack());
                outputSignal.Pack(state.StateVariable);
            }
            else
            {
                throw new ModelException(this, "Type conflict");
            }
            return Results;
        }

        protected virtual void StateCalculate(State state, double deriv)
        {
            state.SetDerivative(deriv, time);
            state.EulerPropagate();
        }
    }
}