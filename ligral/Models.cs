using System.Collections.Generic;
using System.Linq;
using Dict=System.Collections.Generic.Dictionary<string,object>;
using ParameterDictionary=System.Collections.Generic.Dictionary<string,Ligral.Parameter>;
using System;
using System.Text.RegularExpressions;

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
        protected override List<double> Calculate(List<double> values)
        {
            Results.Clear();
            Results.Add(time);
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
                    Results.Add(Convert.ToDouble(value));
                })}
            };
        }
        protected override List<double> Calculate(List<double> values)
        {
            return Results;
        }
        protected override void ConfigureAction(Dict dictionary)
        {
            //Results.Clear(); Validation of configure is done somewhere else
            Results.Add((double)ObtainKeyValue(dictionary, "value"));
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
        private double gain = 1;
        protected override string DocString 
        {
            get
            {
                return "This model amplifies the input by a given constant.";
            }
        }
        private List<double> result = new List<double>();
        protected override void SetUpParameters()
        {
            Parameters = new ParameterDictionary()
            {
                {"value", new Parameter(value=>
                {
                    gain = System.Convert.ToDouble(value);
                })}
            };
        }
        protected override List<double> Calculate(List<double> values)
        {
            result.Clear();
            result.Add(values[0]*gain); // validation of input is done somewhere else
            return result;
        }
        protected override void ConfigureAction(Dictionary<string, object> dictionary)
        {
            gain = System.Convert.ToDouble(ObtainKeyValue(dictionary, "value"));
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
        protected override List<double> Calculate(List<double> values)
        {   
            Results.Clear();
            if (values[0]<right && values[0]>left)
            {
                Results.Add(0);
            }
            else if (values[0]<=left)
            {
                Results.Add(values[0]-left);
            }
            else //values[0]>=right
            {
                Results.Add(values[0]-right);
            }
            return Results;
        }
        protected override void ConfigureAction(Dictionary<string, object> dictionary)
        {
            left = (double)ObtainKeyValue(dictionary, "left");
            right = (double)ObtainKeyValue(dictionary, "right");
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
        protected override List<double> Calculate(List<double> values)
        {   
            Results.Clear();
            if (values[0]<upper && values[0]>lower)
            {
                Results.Add(values[0]);
            }
            else if (values[0]<=lower)
            {
                Results.Add(lower);
            }
            else //values[0]>=upper
            {
                Results.Add(upper);
            }
            return Results;
        }
        protected override void ConfigureAction(Dictionary<string, object> dictionary)
        {
            upper = (double)ObtainKeyValue(dictionary, "upper");
            lower = (double)ObtainKeyValue(dictionary, "lower");
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
        protected override List<double> Calculate(List<double> values)
        {
            Results.Clear();
            Results.Add(Math.Abs(values[0]));
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
        protected override List<double> Calculate(List<double> values)
        {
            Results.Clear();
            Results.Add(Math.Sin(values[0]));
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
        protected override List<double> Calculate(List<double> values)
        {
            Results.Clear();
            Results.Add(Math.Cos(values[0]));
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
        protected override List<double> Calculate(List<double> values)
        {
            Results.Clear();
            Results.Add(Math.Tan(values[0]));
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
        protected override List<double> Calculate(List<double> values)
        {
            Results.Clear();
            Results.Add(Math.Sinh(values[0]));
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
        protected override List<double> Calculate(List<double> values)
        {
            Results.Clear();
            Results.Add(Math.Cosh(values[0]));
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
        protected override List<double> Calculate(List<double> values)
        {
            Results.Clear();
            Results.Add(Math.Tanh(values[0]));
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
        protected override List<double> Calculate(List<double> values)
        {
            Results.Clear();
            Results.Add(Math.Asin(values[0]));
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
        protected override List<double> Calculate(List<double> values)
        {
            Results.Clear();
            Results.Add(Math.Acos(values[0]));
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
        protected override List<double> Calculate(List<double> values)
        {
            Results.Clear();
            Results.Add(Math.Atan(values[0]));
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
        protected override List<double> Calculate(List<double> values)
        {
            Results.Clear();
            Results.Add(Math.Atan2(values[0], values[1]));
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
        protected override List<double> Calculate(List<double> values)
        {
            Results.Clear();
            Results.Add(Math.Asinh(values[0]));
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
        protected override List<double> Calculate(List<double> values)
        {
            Results.Clear();
            Results.Add(Math.Acosh(values[0]));
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
        protected override List<double> Calculate(List<double> values)
        {
            Results.Clear();
            Results.Add(Math.Atanh(values[0]));
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
        protected override List<double> Calculate(List<double> values)
        {
            Results.Clear();
            Results.Add(Math.Exp(values[0]));
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
        protected override List<double> Calculate(List<double> values)
        {
            Results.Clear();
            Results.Add(Math.Pow(values[0], power));
            return Results;
        }
        protected override void ConfigureAction(Dictionary<string, object> dictionary)
        {
            power = (double) ObtainKeyValue(dictionary, "power");
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
        protected override List<double> Calculate(List<double> values)
        {
            Results.Clear();
            Results.Add(Math.Sqrt(values[0]));
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
        protected override List<double> Calculate(List<double> values)
        {
            Results.Clear();
            Results.Add(Math.Sign(values[0]));
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
        protected override List<double> Calculate(List<double> values)
        {
            Results.Clear();
            Results.Add(Math.Pow(values[0], values[1]));
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
                {"base", new Parameter(false, value=>
                {
                    newBase = (double)value;
                }, ()=>
                {
                    newBase = Math.E;
                })}
            };
        }
        protected override List<double> Calculate(List<double> values)
        {
            Results.Clear();
            Results.Add(Math.Log(values[0], newBase));
            return Results;
        }
        protected override void ConfigureAction(Dict dictionary)
        {
            if (dictionary.ContainsKey("base"))
            {
                newBase = (double) dictionary["base"];
            }
            else
            {
                newBase = Math.E;
            }
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
        protected override List<double> Calculate(List<double> values)
        {
            Results.Clear();
            Results.Add(Math.Log(values[0], values[1]));
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
                {"ampl", new Parameter(false, value=>
                {
                    ampl = (double)value;
                }, ()=>
                {
                    ampl = 1;
                })},
                {"omega", new Parameter(false, value=>
                {
                    omega = (double)value;
                }, ()=>
                {
                    omega = 1;
                })},
                {"phi", new Parameter(false, value=>
                {
                    phi = (double)value;
                }, ()=>
                {
                    phi = 0;
                })},
            };
        }
        protected override List<double> Calculate(List<double> values)
        {
            Results.Clear();
            Results.Add(ampl*Math.Sin(omega*time+phi));
            return Results;
        }
        protected override void ConfigureAction(Dictionary<string, object> dictionary)
        {
            if (dictionary.ContainsKey("ampl"))
            {
                ampl = (double)dictionary["ampl"];
            }
            if (dictionary.ContainsKey("omega"))
            {
                omega = (double)dictionary["omega"];
            }
            if (dictionary.ContainsKey("phi"))
            {
                phi = (double)dictionary["phi"];
            }
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
                {"start", new Parameter(false, value=>
                {
                    start = (double)value;
                }, ()=>
                {
                    start = 0;
                })},
                {"level", new Parameter(false, value=>
                {
                    level = (double)value;
                }, ()=>
                {
                    level = 1;
                })},
            };
        }
        protected override List<double> Calculate(List<double> values)
        {
            Results.Clear();
            if (time>=start)
            {
                Results.Add(level);
            }
            else
            {
                Results.Add(0);
            }
            return Results;
        }
        protected override void ConfigureAction(Dictionary<string, object> dictionary)
        {
            if (dictionary.ContainsKey("start"))
            {
                start = (double)dictionary["start"];
            }
            if (dictionary.ContainsKey("level"))
            {
                level = (double)dictionary["level"];
            }
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
        private class DataPoint
        {
            public double Time;
            public double Data;
            public DataPoint(double time, double data)
            {
                Time = time;
                Data = data;
            }
        }
        private List<DataPoint> vector;
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
                    string fileName = (string) value;
                    List<string> lines;
                    try
                    {
                        lines = System.IO.File.ReadAllLines(fileName).ToList();
                    }
                    catch
                    {
                        throw new LigralException($"Error occurs while reading {fileName}");
                    }
                    Regex headerRegex = new Regex(@"^\s*([Tt]ime)\s*,\s*([Dd]*ata?)\s*$");
                    if (!headerRegex.Match(lines[0]).Success)
                    {
                        throw new LigralException($"Invalid playback file {fileName}");
                    }
                    Regex dataRegex = new Regex(@"^\s*([-+]*\d+(.\d*)?)\s*,\s*([-+]*\d+(.\d*)?)\s*$");
                    vector = lines.Skip(1).Select((line, index)=>
                    {
                        Match match = dataRegex.Match(line);
                        if (match.Success)
                        {
                            return new DataPoint(double.Parse(match.Groups[1].Value), double.Parse(match.Groups[3].Value));
                        }
                        else
                        {
                            throw new LigralException($"Invalid data format in file {fileName} line {index+2}: {line}");
                        }
                    }).ToList();
                    vector.Sort((a,b)=>a.Time.CompareTo(b.Time));
                })}
            };
        }
        protected override List<double> Calculate(List<double> values)
        {
            Results.Clear();
            DataPoint before = vector.FindLast(dataPoint=>dataPoint.Time<time);
            DataPoint after = vector.Find(dataPoint=>dataPoint.Time>time);
            DataPoint current = vector.Find(dataPoint=>dataPoint.Time==time);
            if (current!=null)
            {
                Results.Add(current.Data);
            }
            else if (before!=null && after != null)
            {
                Results.Add(before.Data+(after.Data-before.Data)/(after.Time-before.Time)*(time-before.Time));
            }
            else if (before==null && after!=null)
            {
                Results.Add(after.Data);
            }
            else if (before!=null && after==null)
            {
                Results.Add(before.Data);
            }
            else
            {
                throw new LigralException($"Invalid playback input at time {time}");
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
                {"name", new Parameter(false, value=>
                {
                    varName = (string) value;
                }, ()=>
                {
                    varName = Name;
                })}
            };
        }
        protected override List<double> Calculate(List<double> values)
        {
            // Results.Clear();
            System.Console.WriteLine(string.Format("Time: {0,-6:0.00} {1,10} = {2:0.00}", time, varName, values[0]));
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
        private double initial = 0;
        private int delay = 1;
        private List<double> stack = new List<double>();
        protected override void SetUpPorts()
        {
            base.SetUpPorts();
            Initializeable = true;
        }
        protected override void SetUpParameters()
        {
            Parameters = new ParameterDictionary()
            {
                {"initial", new Parameter(false, value=>
                {
                    initial = (double)value;
                }, ()=>
                {
                    initial = 0;
                })},
                {"delay", new Parameter(false, value=>
                {
                    delay = Convert.ToInt32(value);
                }, ()=>
                {
                    delay = 0;
                })},
            };
        }
        public override void Initialize()
        {
            base.Initialize();
            delay -= 1;
        }
        protected override List<double> Calculate(List<double> values)
        {
            Results.Clear();
            stack.Add(values[0]);
            if (stack.Count>delay)
            {
                Results.Add(stack[0]);
                stack.RemoveAt(0);
            }
            else
            {
                Results.Add(initial);
            }
            return Results;
        }
        protected override void ConfigureAction(Dictionary<string, object> dictionary)
        {
            if (dictionary.ContainsKey("initial"))
            {
                initial = (double)dictionary["initial"];
            }
            if (dictionary.ContainsKey("delay"))
            {
                delay = (int)dictionary["delay"];
            }
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
        private double lastTime = 0;
        protected override void SetUpPorts()
        {
            base.SetUpPorts();
            Initializeable = true;
            // Results.Add(0);
        }
        protected override void SetUpParameters()
        {
            Parameters = new ParameterDictionary()
            {
                {"initial", new Parameter(false, value=>
                {
                    Results.Add((double)value);
                }, ()=>
                {
                    Results.Add(0);
                })},
            };
        }
        protected override List<double> Calculate(List<double> values)
        {
            Results[0] += values[0]*(time-lastTime);
            lastTime = time;
            return Results;
        }
        protected override void ConfigureAction(Dictionary<string, object> dictionary)
        {
            if (dictionary.ContainsKey("initial"))
            {
                Results[0] = (double)dictionary["initial"];
            }
        }
    }

    class BoundedIntegrator : Model
    {
        protected override string DocString 
        {
            get
            {
                return "This model outputs the value of the integral of its input signal with respect to time, which is limited by bounds.";
            }
        }
        private double lastTime = 0;
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
            Parameters = new ParameterDictionary()
            {
                {"initial", new Parameter(false, value=>
                {
                    Results.Add((double)value);
                }, ()=>
                {
                    Results.Add(0);
                })},
                {"upper", new Parameter(value=>
                {
                    upper=(double)value;
                })},
                {"lower", new Parameter(value=>
                {
                    lower = (double)value;
                })},
            };
        }
        protected override List<double> Calculate(List<double> values)
        {
            if ((Results[0]<upper && Results[0]>lower) ||
                (Results[0]>=upper && values[0]<0) ||
                (Results[0]<=lower && values[0]>0))
            {
                Results[0] += values[0]*(time-lastTime);
            }
            lastTime = time;
            return Results;
        }
        protected override void ConfigureAction(Dictionary<string, object> dictionary)
        {
            if (dictionary.ContainsKey("initial"))
            {
                Results[0] = (double)dictionary["initial"];
            }
        }
    }

    class Calculator : Model
    {
        protected override string DocString 
        {
            get
            {
                return "This model outputs the result of the algebraic operation defined by parameter type.";
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
                    if (operType=='+'||operType=='-'||operType=='*'||operType=='/'||operType=='^')
                    {
                        type = operType;
                    }
                    else
                    {
                        throw new LigralException("Invalid calculation operator " + operType);
                    }
                })}
            };
        }
        protected override List<double> Calculate(List<double> values)
        {
            Results.Clear();
            if (type=='+')
            {
                Results.Add(values[0]+values[1]);
            }
            else if (type=='-')
            {
                Results.Add(values[0]-values[1]);
            }else if (type=='*')
            {
                Results.Add(values[0]*values[1]);
            }else if (type=='/')
            {
                Results.Add(values[0]/values[1]);
            }else if (type=='^')
            {
                Results.Add(Math.Pow(values[0], values[1]));
            }// validation of operator is done in configure
            return Results;
        }
        protected override void ConfigureAction(Dictionary<string, object> dictionary)
        {
            char operType = System.Convert.ToChar(ObtainKeyValue(dictionary, "type"));
            if (operType=='+'||operType=='-'||operType=='*'||operType=='/')
            {
                type = operType;
            }
            else
            {
                throw new LigralException("Invalid calculation operator " + operType);
            }
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
        protected override List<double> Calculate(List<double> values)
        {
            Results.Clear();
            Results.Add(values.Max());
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
        protected override List<double> Calculate(List<double> values)
        {
            Results.Clear();
            Results.Add(values.Min());
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
        private List<double> dataList = new List<double>();
        private List<double> timeList = new List<double>();
        protected override void SetUpPorts()
        {
            InPortList.Add(new InPort("input", this));
        }
        protected override void SetUpParameters()
        {
            Parameters = new ParameterDictionary()
            {
                {"name", new Parameter(false, value=>
                {
                    fileName = (string)value;
                }, ()=>
                {
                    fileName = Name;
                })}
            };
        }
        protected override List<double> Calculate(List<double> values)
        {
            dataList.Add(values[0]);
            timeList.Add(time);
            return Results;
        }
        protected override void ConfigureAction(Dictionary<string, object> dictionary)
        {
            if (dictionary.ContainsKey("name"))
            {
                fileName = (string) dictionary["name"];
            }
            else
            {
                fileName = Name;
            }
        }
        public override void Release()
        {
            Settings settings = Settings.GetInstance();
            settings.NeedOutput = true;
            List<string> dataLines = timeList.Zip(dataList).ToList().ConvertAll<string>(
                pair=>string.Format("{0},{1}",pair.First, pair.Second));
            dataLines.Insert(0, "time,data");
            string currentDirectory = System.IO.Directory.GetCurrentDirectory();
            string dataFile = System.IO.Path.Join(currentDirectory, settings.OutputFolder, fileName+".csv");
            System.IO.File.WriteAllLines(dataFile, dataLines);
            List<string> scripts = new List<string>();
            scripts.Add("# Auto-generated by Ligral(c)");
            scripts.Add("import numpy as np");
            scripts.Add("import pandas as pd");
            scripts.Add("import matplotlib.pyplot as plt");
            scripts.Add($"data = pd.read_csv(r'{dataFile}').values");
            scripts.Add($"plt.figure(num='{fileName}')");
            scripts.Add("plt.plot(*data.T)");
            scripts.Add("plt.xlabel('time (s)')");
            scripts.Add($"plt.title('{fileName}')");
            scripts.Add("plt.grid()");
            scripts.Add("plt.show()");
            string scriptsFile = System.IO.Path.Join(currentDirectory, settings.OutputFolder, fileName+".py");
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
        private List<double> xDataList = new List<double>();
        private List<double> yDataList = new List<double>();
        protected override void SetUpPorts()
        {
            InPortList.Add(new InPort("x", this));
            InPortList.Add(new InPort("y", this));
        }
        protected override void SetUpParameters()
        {
            Parameters = new ParameterDictionary()
            {
                {"name", new Parameter(false, value=>
                {
                    fileName = (string)value;
                }, ()=>
                {
                    fileName = Name;
                })}
            };
        }
        protected override List<double> Calculate(List<double> values)
        {
            xDataList.Add(values[0]);
            yDataList.Add(values[1]);
            return Results;
        }
        protected override void ConfigureAction(Dictionary<string, object> dictionary)
        {
            if (dictionary.ContainsKey("name"))
            {
                fileName = (string) dictionary["name"];
            }
            else
            {
                fileName = Name;
            }
        }
        public override void Release()
        {
            Settings settings = Settings.GetInstance();
            settings.NeedOutput = true;
            List<string> dataLines = xDataList.Zip(yDataList).ToList().ConvertAll<string>(
                pair=>string.Format("{0},{1}",pair.First, pair.Second));
            dataLines.Insert(0, "x,y");
            string currentDirectory = System.IO.Directory.GetCurrentDirectory();
            string dataFile = System.IO.Path.Join(currentDirectory, settings.OutputFolder, fileName+".csv");
            System.IO.File.WriteAllLines(dataFile, dataLines);
            List<string> scripts = new List<string>();
            scripts.Add("# Auto-generated by Ligral(c)");
            scripts.Add("import numpy as np");
            scripts.Add("import pandas as pd");
            scripts.Add("import matplotlib.pyplot as plt");
            scripts.Add($"data = pd.read_csv(r'{dataFile}').values");
            scripts.Add($"plt.figure(num='{fileName}')");
            scripts.Add("plt.plot(*data.T)");
            scripts.Add("plt.xlabel('x')");
            scripts.Add("plt.ylabel('y')");
            scripts.Add($"plt.title('{fileName}')");
            scripts.Add("plt.grid()");
            scripts.Add("plt.show()");
            string scriptsFile = System.IO.Path.Join(currentDirectory, settings.OutputFolder, fileName+".py");
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
        protected override List<double> Calculate(List<double> values)
        {
            Results.Clear();
            if (values[0]!=0)
            {
                Results.Add(values[1]);
            }
            else
            {
                Results.Add(values[2]);
            }
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
                {"threshold", new Parameter(false, value=>
                {
                    threshold = (double)value;
                }, ()=>
                {
                    threshold = 0;
                })}
            };
        }
        protected override List<double> Calculate(List<double> values)
        {
            Results.Clear();
            if (values[0]>=threshold)
            {
                Results.Add(values[1]);
            }
            else
            {
                Results.Add(values[2]);
            }
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
                {"seed", new Parameter(false, value=>
                {
                    seed = Convert.ToInt32(value);
                    random = new Random(seed);
                }, ()=>
                {
                    seed = System.DateTime.Now.Millisecond;
                    random = new Random(seed);
                })},
                {"upper", new Parameter(false, value=>
                {
                    upper = (double)value;
                }, ()=>
                {
                    upper = 1;
                })},
                {"lower", new Parameter(false, value=>
                {
                    lower = (double)value;
                }, ()=>
                {
                    lower = 0;
                })},
            };
        }
        protected override List<double> Calculate(List<double> values)
        {
            Results.Clear();
            Results.Add(random.NextDouble()*(upper-lower)+lower);
            return Results;
        }
    }
}