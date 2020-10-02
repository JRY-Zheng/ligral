using System.Collections.Generic;
using System.Linq;
using Dict = System.Collections.Generic.Dictionary<string, object>;
using ParameterDictionary = System.Collections.Generic.Dictionary<string, Ligral.Parameter>;
using System;
using System.Text.RegularExpressions;
using MathNet.Numerics.LinearAlgebra;
using DoubleCsvTable;

namespace Ligral
{
    class Clock : Model
    {
        protected override string DocString
        {
            get
            {
                return "This model outputs the simulation time.";
            }
        }
        protected override void SetUpPorts()
        {
            OutPortList.Add(new OutPort("time", this));
        }
        protected override List<Signal> Calculate(List<Signal> values)
        {
            Results[0].Pack(time);
            return Results;
        }
    }

    class Constant : Model
    {
        protected override string DocString
        {
            get
            {
                return "This model outputs the given constant.";
            }
        }
        protected override void SetUpPorts()
        {
            OutPortList.Add(new OutPort("value", this));
        }
        protected override void SetUpParameters()
        {
            Parameters = new ParameterDictionary()
            {
                {"value", new Parameter(value=>
                {
                    Matrix<double> matrix = value as Matrix<double>;
                    if (matrix!=null)
                    {
                        Results[0].Pack(matrix);
                    }
                    else
                    {
                        Results[0].Pack(Convert.ToDouble(value));
                    }
                })}
            };
        }
        protected override List<Signal> Calculate(List<Signal> values)
        {
            return Results;
        }
    }

    class Node : Model
    {
        protected override string DocString
        {
            get
            {
                return "This model outputs what it receives.";
            }
        }
    }
    class Input : Model
    {
        protected override string DocString
        {
            get
            {
                return "This model is automatically used inside a route. Do not call it manually.";
            }
        }
    }
    class Output : Model
    {
        protected override string DocString
        {
            get
            {
                return "This model is automatically used inside a route. Do not call it manually.";
            }
        }
    }
    class Gain : Model
    {
        private Signal gain;
        private bool leftProduct = false;
        protected override string DocString
        {
            get
            {
                return "This model amplifies the input by a given constant.";
            }
        }
        protected override void SetUpParameters()
        {
            Parameters = new ParameterDictionary()
            {
                {"value", new Parameter(value=>
                {
                    gain = new Signal(value);
                })},
                {"prod", new Parameter(value=>
                {
                    string prod = (string) value;
                    switch (prod)
                    {
                        case "left":
                            leftProduct = true; break;
                        case "right":
                            leftProduct = false; break;
                        default:
                            throw new ModelException(this, $"Invalid enum prod {prod}");
                    }
                }, ()=>{})}
            };
        }
        protected override List<Signal> Calculate(List<Signal> values)
        {
            // Results.Clear();
            Signal inputSignal = values[0];
            Signal outputSignal = Results[0];
            if (leftProduct)
                outputSignal.Clone(gain * inputSignal);
            else
                outputSignal.Clone(inputSignal * gain);
            // Results.Add(signal); // validation of input is done somewhere else
            return Results;
        }
    }

    class Deadzone : Model
    {
        private double left = -1;
        private double right = 1;
        protected override string DocString
        {
            get
            {
                return "This model generates zero output within a specified region [left, right].";
            }
        }
        protected override void SetUpParameters()
        {
            Parameters = new ParameterDictionary()
            {
                {"left", new Parameter(value=>
                {
                    left = (double)value;
                })},
                {"right", new Parameter(value=>
                {
                    right = (double)value;
                })},
            };
        }
        protected override List<Signal> Calculate(List<Signal> values)
        {
            Signal inputSignal = values[0];
            Signal outputSignal = Results[0];
            outputSignal.Clone(inputSignal.Apply((item) =>
            {
                if (item < right && item > left)
                {
                    return 0;
                }
                else if (item <= left)
                {
                    return item - left;
                }
                else //item>=right
                {
                    return item - right;
                }
            }));
            return Results;
        }
    }
    class Saturation : Model
    {
        private double upper = 1;
        private double lower = -1;
        protected override string DocString
        {
            get
            {
                return "This model produces an output signal that is the value of the input signal bounded to the upper and lower.";
            }
        }
        protected override void SetUpParameters()
        {
            Parameters = new ParameterDictionary()
            {
                {"upper", new Parameter(value=>
                {
                    upper = (double)value;
                })},
                {"lower", new Parameter(value=>
                {
                    lower = (double)value;
                })},
            };
        }
        protected override List<Signal> Calculate(List<Signal> values)
        {
            // Results.Clear();
            Signal inputSignal = values[0];
            Signal outputSignal = Results[0];
            outputSignal.Clone(inputSignal.Apply((item) =>
            {
                if (item < upper && item > lower)
                {
                    return item;
                }
                else if (item <= lower)
                {
                    return lower;
                }
                else //item>=upper
                {
                    return upper;
                }
            }));
            return Results;
        }
    }

    class Abs : Model
    {
        protected override string DocString
        {
            get
            {
                return "This model calculates the absolute value.";
            }
        }
        protected override void SetUpPorts()
        {
            InPortList.Add(new InPort("x", this));
            OutPortList.Add(new OutPort("y", this));
        }
        protected override List<Signal> Calculate(List<Signal> values)
        {
            // Results.Clear();
            Signal inputSignal = values[0];
            Signal outputSignal = Results[0];
            outputSignal.Clone(inputSignal.Apply(Math.Abs));
            // Results.Add(Math.Abs(values[0]));
            return Results;
        }
    }

    class Sin : Model
    {
        protected override string DocString
        {
            get
            {
                return "This model calculates y=sin(x).";
            }
        }
        protected override void SetUpPorts()
        {
            InPortList.Add(new InPort("x", this));
            OutPortList.Add(new OutPort("y", this));
        }
        protected override List<Signal> Calculate(List<Signal> values)
        {
            // Results.Clear();
            // Results.Add(Math.Sin(values[0]));
            Signal inputSignal = values[0];
            Signal outputSignal = Results[0];
            outputSignal.Clone(inputSignal.Apply(Math.Sin));
            return Results;
        }
    }

    class Cos : Model
    {
        protected override string DocString
        {
            get
            {
                return "This model calculates y=cos(x).";
            }
        }
        protected override void SetUpPorts()
        {
            InPortList.Add(new InPort("x", this));
            OutPortList.Add(new OutPort("y", this));
        }
        protected override List<Signal> Calculate(List<Signal> values)
        {
            // Results.Clear();
            // Results.Add(Math.Cos(values[0]));
            Signal inputSignal = values[0];
            Signal outputSignal = Results[0];
            outputSignal.Clone(inputSignal.Apply(Math.Cos));
            return Results;
        }
    }

    class Tan : Model
    {
        protected override string DocString
        {
            get
            {
                return "This model calculates y=tan(x).";
            }
        }
        protected override void SetUpPorts()
        {
            InPortList.Add(new InPort("x", this));
            OutPortList.Add(new OutPort("y", this));
        }
        protected override List<Signal> Calculate(List<Signal> values)
        {
            // Results.Clear();
            // Results.Add(Math.Tan(values[0]));
            Signal inputSignal = values[0];
            Signal outputSignal = Results[0];
            outputSignal.Clone(inputSignal.Apply(Math.Tan));
            return Results;
        }
    }

    class Sinh : Model
    {
        protected override string DocString
        {
            get
            {
                return "This model calculates y=sinh(x).";
            }
        }
        protected override void SetUpPorts()
        {
            InPortList.Add(new InPort("x", this));
            OutPortList.Add(new OutPort("y", this));
        }
        protected override List<Signal> Calculate(List<Signal> values)
        {
            // Results.Clear();
            // Results.Add(Math.Sinh(values[0]));
            Signal inputSignal = values[0];
            Signal outputSignal = Results[0];
            outputSignal.Clone(inputSignal.Apply(Math.Sinh));
            return Results;
        }
    }

    class Cosh : Model
    {
        protected override string DocString
        {
            get
            {
                return "This model calculates y=cosh(x).";
            }
        }
        protected override void SetUpPorts()
        {
            InPortList.Add(new InPort("x", this));
            OutPortList.Add(new OutPort("y", this));
        }
        protected override List<Signal> Calculate(List<Signal> values)
        {
            // Results.Clear();
            // Results.Add(Math.Cosh(values[0]));
            Signal inputSignal = values[0];
            Signal outputSignal = Results[0];
            outputSignal.Clone(inputSignal.Apply(Math.Cosh));
            return Results;
        }
    }

    class Tanh : Model
    {
        protected override string DocString
        {
            get
            {
                return "This model calculates y=tanh(x).";
            }
        }
        protected override void SetUpPorts()
        {
            InPortList.Add(new InPort("x", this));
            OutPortList.Add(new OutPort("y", this));
        }
        protected override List<Signal> Calculate(List<Signal> values)
        {
            // Results.Clear();
            // Results.Add(Math.Tanh(values[0]));
            Signal inputSignal = values[0];
            Signal outputSignal = Results[0];
            outputSignal.Clone(inputSignal.Apply(Math.Tanh));
            return Results;
        }
    }

    class Asin : Model
    {
        protected override string DocString
        {
            get
            {
                return "This model calculates y=asin(x).";
            }
        }
        protected override void SetUpPorts()
        {
            InPortList.Add(new InPort("x", this));
            OutPortList.Add(new OutPort("y", this));
        }
        protected override List<Signal> Calculate(List<Signal> values)
        {
            // Results.Clear();
            // Results.Add(Math.Asin(values[0]));
            Signal inputSignal = values[0];
            Signal outputSignal = Results[0];
            outputSignal.Clone(inputSignal.Apply(Math.Asin));
            return Results;
        }
    }

    class Acos : Model
    {
        protected override string DocString
        {
            get
            {
                return "This model calculates y=acos(x).";
            }
        }
        protected override void SetUpPorts()
        {
            InPortList.Add(new InPort("x", this));
            OutPortList.Add(new OutPort("y", this));
        }
        protected override List<Signal> Calculate(List<Signal> values)
        {
            // Results.Clear();
            // Results.Add(Math.Acos(values[0]));
            Signal inputSignal = values[0];
            Signal outputSignal = Results[0];
            outputSignal.Clone(inputSignal.Apply(Math.Acos));
            return Results;
        }
    }

    class Atan : Model
    {
        protected override string DocString
        {
            get
            {
                return "This model calculates y=atan(x).";
            }
        }
        protected override void SetUpPorts()
        {
            InPortList.Add(new InPort("x", this));
            OutPortList.Add(new OutPort("y", this));
        }
        protected override List<Signal> Calculate(List<Signal> values)
        {
            // Results.Clear();
            // Results.Add(Math.Atan(values[0]));
            Signal inputSignal = values[0];
            Signal outputSignal = Results[0];
            outputSignal.Clone(inputSignal.Apply(Math.Atan));
            return Results;
        }
    }

    class Atan2 : Model
    {
        protected override string DocString
        {
            get
            {
                return "This model calculates y=atan(sin/cos).";
            }
        }
        protected override void SetUpPorts()
        {
            InPortList.Add(new InPort("sin", this));
            InPortList.Add(new InPort("cos", this));
            OutPortList.Add(new OutPort("y", this));
        }
        protected override List<Signal> Calculate(List<Signal> values)
        {
            // Results.Clear();
            // Results.Add(Math.Atan2(values[0], values[1]));
            Signal ySignal = values[0];
            Signal xSignal = values[1];
            Signal outputSignal = Results[0];
            outputSignal.Clone(ySignal.ZipApply(xSignal, Math.Atan2));
            return Results;
        }
    }

    class Asinh : Model
    {
        protected override string DocString
        {
            get
            {
                return "This model calculates y=asinh(x).";
            }
        }
        protected override void SetUpPorts()
        {
            InPortList.Add(new InPort("x", this));
            OutPortList.Add(new OutPort("y", this));
        }
        protected override List<Signal> Calculate(List<Signal> values)
        {
            // Results.Clear();
            // Results.Add(Math.Asinh(values[0]));
            Signal inputSignal = values[0];
            Signal outputSignal = Results[0];
            outputSignal.Clone(inputSignal.Apply(Math.Asinh));
            return Results;
        }
    }

    class Acosh : Model
    {
        protected override string DocString
        {
            get
            {
                return "This model calculates y=acosh(x).";
            }
        }
        protected override void SetUpPorts()
        {
            InPortList.Add(new InPort("x", this));
            OutPortList.Add(new OutPort("y", this));
        }
        protected override List<Signal> Calculate(List<Signal> values)
        {
            // Results.Clear();
            // Results.Add(Math.Acosh(values[0]));
            Signal inputSignal = values[0];
            Signal outputSignal = Results[0];
            outputSignal.Clone(inputSignal.Apply(Math.Acosh));
            return Results;
        }
    }

    class Atanh : Model
    {
        protected override string DocString
        {
            get
            {
                return "This model calculates y=atanh(x).";
            }
        }
        protected override void SetUpPorts()
        {
            InPortList.Add(new InPort("x", this));
            OutPortList.Add(new OutPort("y", this));
        }
        protected override List<Signal> Calculate(List<Signal> values)
        {
            // Results.Clear();
            // Results.Add(Math.Atanh(values[0]));
            Signal inputSignal = values[0];
            Signal outputSignal = Results[0];
            outputSignal.Clone(inputSignal.Apply(Math.Atanh));
            return Results;
        }
    }

    class Exp : Model
    {
        protected override string DocString
        {
            get
            {
                return "This model calculates y=exp(x).";
            }
        }
        protected override void SetUpPorts()
        {
            InPortList.Add(new InPort("x", this));
            OutPortList.Add(new OutPort("y", this));
        }
        protected override List<Signal> Calculate(List<Signal> values)
        {
            // Results.Clear();
            // Results.Add(Math.Exp(values[0]));
            Signal inputSignal = values[0];
            Signal outputSignal = Results[0];
            outputSignal.Clone(inputSignal.Apply(Math.Exp));
            return Results;
        }
    }

    class Pow : Model
    {
        protected override string DocString
        {
            get
            {
                return "This model calculates y=x^power.";
            }
        }
        private double power;
        protected override void SetUpPorts()
        {
            InPortList.Add(new InPort("x", this));
            OutPortList.Add(new OutPort("y", this));
        }
        protected override void SetUpParameters()
        {
            Parameters = new ParameterDictionary()
            {
                {"power", new Parameter(value=>
                {
                    power = (double)value;
                })}
            };
        }
        protected override List<Signal> Calculate(List<Signal> values)
        {
            // Results.Clear();
            // Results.Add(Math.Pow(values[0], power));
            Signal inputSignal = values[0];
            Signal outputSignal = Results[0];
            outputSignal.Clone(inputSignal.Apply((item) => Math.Pow(item, power)));
            return Results;
        }
    }
    class Sqrt : Model
    {
        protected override string DocString
        {
            get
            {
                return "This model calculates y=sqrt(x).";
            }
        }
        protected override void SetUpPorts()
        {
            InPortList.Add(new InPort("x", this));
            OutPortList.Add(new OutPort("y", this));
        }
        protected override List<Signal> Calculate(List<Signal> values)
        {
            // Results.Clear();
            // Results.Add(Math.Sqrt(values[0]));
            Signal inputSignal = values[0];
            Signal outputSignal = Results[0];
            outputSignal.Clone(inputSignal.Apply(Math.Sqrt));
            return Results;
        }
    }

    class Sign : Model
    {
        protected override string DocString
        {
            get
            {
                return "This model calculates y = 1 if x>0;-1 if x<0; 0 if x=0.";
            }
        }
        protected override void SetUpPorts()
        {
            InPortList.Add(new InPort("x", this));
            OutPortList.Add(new OutPort("y", this));
        }
        protected override List<Signal> Calculate(List<Signal> values)
        {
            // Results.Clear();
            // Results.Add(Math.Sign(values[0]));
            Signal inputSignal = values[0];
            Signal outputSignal = Results[0];
            outputSignal.Clone(inputSignal.Apply((item) => Convert.ToDouble(Math.Sign(item))));
            return Results;
        }
    }

    class Pow2 : Model
    {
        protected override string DocString
        {
            get
            {
                return "This model calculates y=base^index.";
            }
        }
        protected override void SetUpPorts()
        {
            InPortList.Add(new InPort("base", this));
            InPortList.Add(new InPort("index", this));
            OutPortList.Add(new OutPort("y", this));
        }
        protected override List<Signal> Calculate(List<Signal> values)
        {
            // Results.Clear();
            // Results.Add(Math.Pow(values[0], values[1]));
            Signal baseSignal = values[0];
            Signal indexSignal = values[1];
            Signal outputSignal = Results[0];
            outputSignal.Clone(baseSignal.ZipApply(indexSignal, Math.Pow));
            return Results;
        }
    }

    class Log : Model
    {
        protected override string DocString
        {
            get
            {
                return "This model calculates y=log(x)/log(base), where default base is e.";
            }
        }
        private double newBase;
        protected override void SetUpPorts()
        {
            InPortList.Add(new InPort("x", this));
            OutPortList.Add(new OutPort("y", this));
        }
        protected override void SetUpParameters()
        {
            Parameters = new ParameterDictionary()
            {
                {"base", new Parameter(value=>
                {
                    newBase = (double)value;
                }, ()=>
                {
                    newBase = Math.E;
                })}
            };
        }
        protected override List<Signal> Calculate(List<Signal> values)
        {
            // Results.Clear();
            // Results.Add(Math.Log(values[0], newBase));
            Signal inputSignal = values[0];
            Signal outputSignal = Results[0];
            outputSignal.Clone(inputSignal.Apply((item) => Math.Log(item, newBase)));
            return Results;
        }
    }

    class Log2 : Model
    {
        protected override string DocString
        {
            get
            {
                return "This model calculates y=log(x)/log(base).";
            }
        }
        protected override void SetUpPorts()
        {
            InPortList.Add(new InPort("x", this));
            InPortList.Add(new InPort("base", this));
            OutPortList.Add(new OutPort("y", this));
        }
        protected override List<Signal> Calculate(List<Signal> values)
        {
            // Results.Clear();
            // Results.Add(Math.Log(values[0], values[1]));
            Signal xSignal = values[0];
            Signal baseSignal = values[1];
            Signal outputSignal = Results[0];
            outputSignal.Clone(xSignal.ZipApply(baseSignal, Math.Log));
            return Results;
        }
    }

    class SineWave : Model
    {
        protected override string DocString
        {
            get
            {
                return "This model outputs ampl*sin(omega*time+phi).";
            }
        }
        private double ampl = 1;
        private double omega = 1;
        private double phi = 0;
        protected override void SetUpPorts()
        {
            OutPortList.Add(new OutPort("source", this));
        }
        protected override void SetUpParameters()
        {
            Parameters = new ParameterDictionary()
            {
                {"ampl", new Parameter(value=>
                {
                    ampl = (double)value;
                }, ()=>
                {
                    ampl = 1;
                })},
                {"omega", new Parameter(value=>
                {
                    omega = (double)value;
                }, ()=>
                {
                    omega = 1;
                })},
                {"phi", new Parameter(value=>
                {
                    phi = (double)value;
                }, ()=>
                {
                    phi = 0;
                })},
            };
        }
        protected override List<Signal> Calculate(List<Signal> values)
        {
            // Results.Clear();
            // Results.Add(ampl*Math.Sin(omega*time+phi));
            Signal outputSignal = Results[0];
            outputSignal.Pack(ampl * Math.Sin(omega * time + phi));
            return Results;
        }
    }

    class Step : Model
    {
        protected override string DocString
        {
            get
            {
                return "This model provides a step between 0 and a definable level at a specified time.";
            }
        }
        private double start = 0;
        private double level = 1;
        protected override void SetUpPorts()
        {
            OutPortList.Add(new OutPort("source", this));
        }
        protected override void SetUpParameters()
        {
            Parameters = new ParameterDictionary()
            {
                {"start", new Parameter(value=>
                {
                    start = (double)value;
                }, ()=>
                {
                    start = 0;
                })},
                {"level", new Parameter(value=>
                {
                    level = (double)value;
                }, ()=>
                {
                    level = 1;
                })},
            };
        }
        protected override List<Signal> Calculate(List<Signal> values)
        {
            // Results.Clear();
            Signal outputSignal = Results[0];
            if (time >= start)
            {
                outputSignal.Pack(level);
            }
            else
            {
                outputSignal.Pack(0);
            }
            return Results;
        }
    }

    class Playback : Model
    {
        protected override string DocString
        {
            get
            {
                return "This model generates a playback provided by a time-data csv file.";
            }
        }
        private CsvTable table;
        private int rowNo = 0;
        private int colNo = 0;
        protected override void SetUpPorts()
        {
            OutPortList.Add(new OutPort("source", this));
        }
        protected override void SetUpParameters()
        {
            Parameters = new ParameterDictionary()
            {
                {"file", new Parameter(value=>
                {
                    table = new CsvTable((string)value, true);
                    if (table.Columns.Count < 2 || table.GetColumnName(0) != "Time" || table.GetColumnName(0) != "time")
                    {
                        throw new ModelException(this,"Invalid playback file");
                    }
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
        private List<double> Interpolate()
        {
            List<double> before = table.Data.FindLast(row => row[0] < time);
            List<double> after = table.Data.Find(row => row[0] > time);
            List<double> current = table.Data.Find(row => row[0] == time);
            if (current != null)
            {
                return current;
            }
            else if (before != null && after != null)
            {
                // Results.Add(before.Data+(after.Data-before.Data)/(after.Time-before.Time)*(time-before.Time));
                double tb = before[0];
                double ta = after[0];
                return before.Zip(after, (b, a) => b + (a - b) / (ta - tb) * (time - tb)).ToList();
            }
            else if (before == null && after != null)
            {
                return after.ToList();
            }
            else if (before != null && after == null)
            {
                return before.ToList();
            }
            else
            {
                throw new ModelException(this, $"Invalid playback input at time {time}");
            }
        }
        protected override List<Signal> Calculate(List<Signal> values)
        {
            // Results.Clear();
            Signal outputSignal = Results[0];
            List<double> playback = Interpolate();
            if (colNo == 0 && rowNo == 0 && playback.Count == 2)
            {
                outputSignal.Pack(playback[1]);
            }
            else if (colNo * rowNo == playback.Count - 1)
            {
                MatrixBuilder<double> m = Matrix<double>.Build;
                Matrix<double> matrix = m.Dense(colNo, rowNo, playback.Skip(1).ToArray()).Transpose();
                outputSignal.Pack(matrix);
            }
            else
            {
                throw new ModelException(this, $"Inconsistency of row, col and playback");
            }
            return Results;
        }
    }

    class Print : Model
    {
        protected override string DocString
        {
            get
            {
                return "This model prints the data to the console.";
            }
        }
        private string varName;
        protected override void SetUpPorts()
        {
            InPortList.Add(new InPort("input", this));
        }
        protected override void SetUpParameters()
        {
            Parameters = new ParameterDictionary()
            {
                {"name", new Parameter(value=>
                {
                    varName = (string) value;
                }, ()=>
                {
                    varName = Name;
                })}
            };
        }
        protected override List<Signal> Calculate(List<Signal> values)
        {
            // Results.Clear();
            Signal inputSignal = values[0];
            System.Console.WriteLine(string.Format("Time: {0,-6:0.00} {1,10} = {2:0.00}", time, varName, inputSignal.ToStringInLine()));
            return Results;
        }
    }

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
                    double delayedFrame = Convert.ToInt32(value);
                    if (delayedFrame < 1)
                    {
                        throw new ModelException(this, "Delay should be greater than 1");
                    }
                    for (int i = 0; i < delayedFrame; i++)
                    {
                        stack.Add(new Signal());
                    }
                }, ()=>
                {
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

    class BoundedIntegrator : Integrator
    {
        protected override string DocString
        {
            get
            {
                return "This model outputs the value of the integral of its input signal with respect to time, which is limited by bounds.";
            }
        }
        private double upper;
        private double lower;
        protected override void SetUpPorts()
        {
            base.SetUpPorts();
            Initializeable = true;
            // Results.Add(0);
        }
        protected override void SetUpParameters()
        {
            base.SetUpParameters();
            Parameters["upper"] = new Parameter(value=>
                {
                    upper=(double)value;
                });
            Parameters["lower"] = new Parameter(value=>
                {
                    lower = (double)value;
                });
        }
        protected override void StateCalculate(State state, double deriv)
        {
            if ((state.StateVariable < upper && state.StateVariable > lower) ||
                (state.StateVariable >= upper && deriv < 0) ||
                (state.StateVariable <= lower && deriv > 0))
            {
                base.StateCalculate(state, deriv);
            }
            else
            {
                base.StateCalculate(state, 0);
            }
        }
        // protected override List<Signal> Calculate(List<Signal> values)
        // {
        //     if ((Results[0] < upper && Results[0] > lower) ||
        //         (Results[0] >= upper && values[0] < 0) ||
        //         (Results[0] <= lower && values[0] > 0))
        //     {
        //         Results[0] += values[0] * (time - lastTime);
        //     }
        //     lastTime = time;
        //     return Results;
        // }
    }

    class Calculator : Model
    {
        protected override string DocString
        {
            get
            {
                return "This model outputs the result of the algebraic operation defined by parameter type (+-*/^@).";
            }
        }
        private char type;
        protected override void SetUpPorts()
        {
            InPortList.Add(new InPort("left", this));
            InPortList.Add(new InPort("right", this));
            OutPortList.Add(new OutPort("result", this));
        }
        protected override void SetUpParameters()
        {
            Parameters = new ParameterDictionary()
            {
                {"type", new Parameter(value=>
                {
                    char operType = System.Convert.ToChar(value);
                    if (operType=='+'||operType=='-'||operType=='*'||operType=='/'||operType=='^'||operType=='@')
                    {
                        type = operType;
                    }
                    else
                    {
                        throw new ModelException(this,"Invalid calculation operator " + operType);
                    }
                })}
            };
        }
        protected override List<Signal> Calculate(List<Signal> values)
        {
            // Results.Clear();
            Signal leftSignal = values[0];
            Signal rightSignal = values[1];
            Signal outputSignal = Results[0];
            if (type == '+')
            {
                outputSignal.Clone(leftSignal + rightSignal);
            }
            else if (type == '-')
            {
                outputSignal.Clone(leftSignal - rightSignal);
            }
            else if (type == '*')
            {
                outputSignal.Clone(leftSignal * rightSignal);
            }
            else if (type == '/')
            {
                outputSignal.Clone(leftSignal / rightSignal);
            }
            else if (type == '^')
            {
                outputSignal.Clone(leftSignal ^ rightSignal);
            }
            else if (type == '@')
            {
                outputSignal.Clone(leftSignal & rightSignal);
            }// validation of operator is done in configure
            return Results;
        }
    }

    class Max : Model
    {
        protected override string DocString
        {
            get
            {
                return "This model returns maximal value among inputs";
            }
        }
        protected override void SetUpPorts()
        {
            InPortList.Add(new InPort("first", this));
            InPortList.Add(new InPort("second", this));
            OutPortList.Add(new OutPort("max", this));
        }
        protected override List<Signal> Calculate(List<Signal> values)
        {
            // Results.Clear();
            Results[0].Clone(values.Max());
            return Results;
        }
    }

    class Min : Model
    {
        protected override string DocString
        {
            get
            {
                return "This model returns minimal value among inputs";
            }
        }
        protected override void SetUpPorts()
        {
            InPortList.Add(new InPort("first", this));
            InPortList.Add(new InPort("second", this));
            OutPortList.Add(new OutPort("min", this));
        }
        protected override List<Signal> Calculate(List<Signal> values)
        {
            // Results.Clear();
            Results[0].Clone(values.Min());
            return Results;
        }
    }

    class Scope : Model
    {
        protected override string DocString
        {
            get
            {
                return "This model displays time domain signals and stores data to the output folder.";
            }
        }
        private string fileName;
        private CsvTable table;
        private int rowNo = -1;
        private int colNo = -1;
        protected override void SetUpPorts()
        {
            InPortList.Add(new InPort("input", this));
        }
        protected override void SetUpParameters()
        {
            Parameters = new ParameterDictionary()
            {
                {"name", new Parameter(value=>
                {
                    fileName = (string)value;
                }, ()=>
                {
                    fileName = Name;
                })}
            };
        }
        protected override List<Signal> Calculate(List<Signal> values)
        {
            Signal inputSignal = values[0];
            if (rowNo < 0 || colNo < 0)
            {
                (rowNo, colNo) = inputSignal.Shape();
                List<string> columns = new List<string>() {"Time"};
                if (inputSignal.IsMatrix)
                {
                    for(int i = 0; i < rowNo; i++)
                    {
                        for (int j = 0; j < colNo; j++)
                        {
                            columns.Add($"Data({i}-{j})");
                        }
                    }
                }
                else
                {
                    columns.Add("Data");
                }
                table = new CsvTable(columns, new List<List<double>>());
            }
            List<double> row = inputSignal.ToList();
            row.Insert(0, time);
            table.AddRow(row);
            return Results;
        }
        public override void Release()
        {
            Settings settings = Settings.GetInstance();
            settings.NeedOutput = true;
            string currentDirectory = System.IO.Directory.GetCurrentDirectory();
            string dataFile = System.IO.Path.Join(currentDirectory, settings.OutputFolder, fileName + ".csv");
            table.DumpFile(dataFile);
            List<string> scripts = new List<string>();
            scripts.Add("# Auto-generated by Ligral(c)");
            scripts.Add("import numpy as np");
            scripts.Add("import pandas as pd");
            scripts.Add("import matplotlib.pyplot as plt");
            scripts.Add($"frame = pd.read_csv(r'{dataFile}')");
            scripts.Add($"data = frame.values");
            scripts.Add($"plt.figure(num='{fileName}')");
            scripts.Add("time = data.T[0]");
            if (rowNo ==0 && colNo == 0)
            {
                scripts.Add("plt.plot(time, data.T[1])");
                scripts.Add("plt.xlabel('time (s)')");
                scripts.Add("plt.ylabel('Data')");
                scripts.Add("plt.grid()");
            }
            else
            {
                scripts.Add("for i, col in enumerate(data.T[1:]):");
                scripts.Add($"    plt.subplot({rowNo}, {colNo}, i+1)");
                scripts.Add("    plt.plot(time, col)");
                scripts.Add("    plt.xlabel('time (s)')");
                scripts.Add("    plt.ylabel(frame.columns[i+1])");
                scripts.Add("    plt.grid()");
            }
            scripts.Add($"plt.suptitle('{fileName}')");
            scripts.Add("plt.tight_layout()");
            scripts.Add("plt.show()");
            string scriptsFile = System.IO.Path.Join(currentDirectory, settings.OutputFolder, fileName + ".py");
            System.IO.File.WriteAllLines(scriptsFile, scripts);
            System.Diagnostics.Process.Start("python", $"\"{scriptsFile}\"");
        }
    }
    class PhaseDiagram : Model
    {
        protected override string DocString
        {
            get
            {
                return "This model displays phase diagram and stores data to the output folder.";
            }
        }
        private string fileName;
        private CsvTable table;
        private int rowNo = -1;
        private int colNo = -1;
        private enum Mode {Ymn, Xmn, X1mYn1, Xm1Y1n, X1Y1};
        private Mode mode;
        protected override void SetUpPorts()
        {
            InPortList.Add(new InPort("x", this));
            InPortList.Add(new InPort("y", this));
        }
        protected override void SetUpParameters()
        {
            Parameters = new ParameterDictionary()
            {
                {"name", new Parameter(value=>
                {
                    fileName = (string)value;
                }, ()=>
                {
                    fileName = Name;
                })}
            };
        }
        protected override List<Signal> Calculate(List<Signal> values)
        {
            Signal xSignal = values[0];
            Signal ySignal = values[1];
            if (rowNo < 0 || colNo < 0)
            {
                (int xr, int xc) = xSignal.Shape();
                (int yr, int yc) = xSignal.Shape();
                List<string> columns = new List<string>() {"Time"};
                if (!xSignal.IsMatrix && !ySignal.IsMatrix)
                {
                    colNo = 1;
                    rowNo = 1;
                    mode = Mode.X1Y1;
                    columns.Add("Data:x");
                    columns.Add("Data:y");
                }
                else if (!xSignal.IsMatrix)
                {
                    colNo = yc;
                    rowNo = yr;
                    mode = Mode.Ymn;
                    columns.Add("Data:x");
                    for(int i = 0; i < rowNo; i++)
                    {
                        for (int j = 0; j < colNo; j++)
                        {
                            columns.Add($"Data:y({i}-{j})");
                        }
                    }
                }
                else if (!ySignal.IsMatrix)
                {
                    colNo = xc;
                    rowNo = xr;
                    mode = Mode.Xmn;
                    for(int i = 0; i < rowNo; i++)
                    {
                        for (int j = 0; j < colNo; j++)
                        {
                            columns.Add($"Data:x({i}-{j})");
                        }
                    }
                    columns.Add("Data:y");
                }
                else if (xr == 1 && yc == 1)
                {
                    colNo = xc;
                    rowNo = yr;
                    mode = Mode.X1mYn1;
                    for (int j = 0; j < colNo; j++)
                    {
                        columns.Add($"Data:x({j})");
                    }
                    for(int i = 0; i < rowNo; i++)
                    {
                        columns.Add($"Data:y({i})");
                    }
                }
                else if (xc == 1 && yr == 1)
                {
                    colNo = yc;
                    rowNo = xr;
                    mode = Mode.Xm1Y1n;
                    for(int i = 0; i < rowNo; i++)
                    {
                        columns.Add($"Data:x({i})");
                    }
                    for (int j = 0; j < colNo; j++)
                    {
                        columns.Add($"Data:y({j})");
                    }
                }
                else
                {
                    throw new ModelException(this, "PhaseDiagram only accepts [scalar, (m*n)] or [(1*m), (n*1)] or vice versa.");
                }
                table = new CsvTable(columns, new List<List<double>>());
            }
            List<double> row = xSignal.ToList();
            row.AddRange(ySignal.ToList());
            row.Insert(0, time);
            table.AddRow(row);
            return Results;
        }
        public override void Release()
        {
            Settings settings = Settings.GetInstance();
            settings.NeedOutput = true;
            string currentDirectory = System.IO.Directory.GetCurrentDirectory();
            string dataFile = System.IO.Path.Join(currentDirectory, settings.OutputFolder, fileName + ".csv");
            table.DumpFile(dataFile);
            List<string> scripts = new List<string>();
            scripts.Add("# Auto-generated by Ligral(c)");
            scripts.Add("import numpy as np");
            scripts.Add("import pandas as pd");
            scripts.Add("import matplotlib.pyplot as plt");
            scripts.Add($"frame = pd.read_csv(r'{dataFile}')");
            scripts.Add("data = frame.values");
            scripts.Add($"plt.figure(num='{fileName}')");
            scripts.Add("time = data.T[0]");
            switch (mode)
            {
                case Mode.X1Y1:
                    scripts.Add("x = data.T[1]");
                    scripts.Add("y = data.T[2]");
                    scripts.Add("plt.plot(x, y)");
                    scripts.Add("plt.xlabel('x')");
                    scripts.Add("plt.ylabel('y')");
                    scripts.Add("plt.grid()");
                    break;
                case Mode.Ymn:
                    scripts.Add("x = data.T[1]");
                    scripts.Add("y = data.T[2:]");
                    scripts.Add("for i, col in enumerate(y):");
                    scripts.Add($"    plt.subplot({rowNo}, {colNo}, i+1)");
                    scripts.Add("    plt.plot(x, col)");
                    scripts.Add("    plt.xlabel('x')");
                    scripts.Add("    plt.ylabel(f'y{i}')");
                    scripts.Add("    plt.grid()");
                    break;
                case Mode.Xmn:
                    scripts.Add("x = data.T[1:-1]");
                    scripts.Add("y = data.T[-1]");
                    scripts.Add("for i, col in enumerate(x):");
                    scripts.Add($"    plt.subplot({rowNo}, {colNo}, i+1)");
                    scripts.Add("    plt.plot(col, y)");
                    scripts.Add("    plt.xlabel(f'x{i}')");
                    scripts.Add("    plt.ylabel('y')");
                    scripts.Add("    plt.grid()");
                    break;
                case Mode.X1mYn1:
                    scripts.Add($"x = data.T[1:{colNo+1}]");
                    scripts.Add($"y = data.T[{colNo+1}:]");
                    scripts.Add("for i, col in enumerate(y):");
                    scripts.Add("    for j, row in enumerate(x):");
                    scripts.Add($"        plt.subplot({rowNo}, {colNo}, i*{rowNo}+j+1)");
                    scripts.Add("        plt.plot(row, col)");
                    scripts.Add("        plt.xlabel(f'x{j}')");
                    scripts.Add("        plt.ylabel(f'y{i}')");
                    scripts.Add("        plt.grid()");
                    break;
                case Mode.Xm1Y1n:
                    scripts.Add($"x = data.T[1:{rowNo+1}]");
                    scripts.Add($"y = data.T[{rowNo+1}:]");
                    scripts.Add("for i, col in enumerate(x):");
                    scripts.Add("    for j, row in enumerate(y):");
                    scripts.Add($"        plt.subplot({rowNo}, {colNo}, i*{rowNo}+j+1)");
                    scripts.Add("        plt.plot(col, row)");
                    scripts.Add("        plt.xlabel(f'x{i}')");
                    scripts.Add("        plt.ylabel(f'y{j}')");
                    scripts.Add("        plt.grid()");
                    break;
            }
            scripts.Add($"plt.suptitle('{fileName}')");
            scripts.Add("plt.show()");
            string scriptsFile = System.IO.Path.Join(currentDirectory, settings.OutputFolder, fileName + ".py");
            System.IO.File.WriteAllLines(scriptsFile, scripts);
            System.Diagnostics.Process.Start("python", $"\"{scriptsFile}\"");
        }
    }

    class LogicSwitch : Model
    {
        protected override string DocString
        {
            get
            {
                return "This model accepts one condition and two values, returns first if the condition is met (non-zero) otherwise second.";
            }
        }
        protected override void SetUpPorts()
        {
            InPortList.Add(new InPort("condition", this));
            InPortList.Add(new InPort("first", this));
            InPortList.Add(new InPort("second", this));
            OutPortList.Add(new OutPort("result", this));
        }
        protected override List<Signal> Calculate(List<Signal> values)
        {
            // Results.Clear();
            Signal conditionSignal = values[0].Apply(item => item == 0 ? 0 : 1);
            Signal firstSignal = values[1];
            Signal secondSignal = values[2];
            Signal resultSignal = Results[0];
            resultSignal.Clone(conditionSignal & firstSignal + (1 - conditionSignal) & secondSignal);
            return Results;
        }
    }

    class ThresholdSwitch : Model
    {
        protected override string DocString
        {
            get
            {
                return "This model accepts one condition and two values, returns first if the condition reaches threshold otherwise second.";
            }
        }
        private double threshold;
        protected override void SetUpPorts()
        {
            InPortList.Add(new InPort("condition", this));
            InPortList.Add(new InPort("first", this));
            InPortList.Add(new InPort("second", this));
            OutPortList.Add(new OutPort("result", this));
        }
        protected override void SetUpParameters()
        {
            Parameters = new ParameterDictionary()
            {
                {"threshold", new Parameter(value=>
                {
                    threshold = (double)value;
                }, ()=>
                {
                    threshold = 0;
                })}
            };
        }
        protected override List<Signal> Calculate(List<Signal> values)
        {
            // Results.Clear();
            Signal conditionSignal = values[0].Apply(item => item >= threshold ? 1 : 0);
            Signal firstSignal = values[1];
            Signal secondSignal = values[2];
            Signal resultSignal = Results[0];
            resultSignal.Clone(conditionSignal & firstSignal + (1 - conditionSignal) & secondSignal);
            return Results;
        }
    }

    class Rand : Model
    {
        protected override string DocString
        {
            get
            {
                return "This model generates random value in given range.";
            }
        }
        private Random random;
        private int seed;
        private double upper;
        private double lower;
        protected override void SetUpPorts()
        {
            OutPortList.Add(new OutPort("output", this));
        }
        protected override void SetUpParameters()
        {
            Parameters = new ParameterDictionary()
            {
                {"seed", new Parameter(value=>
                {
                    seed = Convert.ToInt32(value);
                    random = new Random(seed);
                }, ()=>
                {
                    seed = System.DateTime.Now.Millisecond;
                    random = new Random(seed);
                })},
                {"upper", new Parameter(value=>
                {
                    upper = (double)value;
                }, ()=>
                {
                    upper = 1;
                })},
                {"lower", new Parameter(value=>
                {
                    lower = (double)value;
                }, ()=>
                {
                    lower = 0;
                })},
            };
        }
        protected override List<Signal> Calculate(List<Signal> values)
        {
            // Results.Clear();
            Signal outputSignal = Results[0];
            outputSignal.Pack(random.NextDouble() * (upper - lower) + lower);
            return Results;
        }
    }
}