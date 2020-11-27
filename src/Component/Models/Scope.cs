using System.Collections.Generic;
using ParameterDictionary = System.Collections.Generic.Dictionary<string, Ligral.Component.Parameter>;
using Ligral.Tools;
using Ligral.Tools.Protocols;
using Ligral.Simulation;

namespace Ligral.Component.Models
{
    class Scope : Model
    {
        protected override string DocString
        {
            get
            {
                return "This model displays time domain signals and stores data to the output folder.";
            }
        }
        private string varName;
        private List<Observation> observations = new List<Observation>();
        private int rowNo = -1;
        private int colNo = -1;
        private Publisher publisher = new Publisher();
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
                    varName = (string)value;
                }, ()=> {})}
            };
        }
        protected override List<Signal> DefaultCalculate(List<Signal> values)
        {
            Signal inputSignal = values[0];
            if (varName == null)
            {
                varName = GivenName ?? inputSignal.Name ?? Name;
                if (Scope != null)
                {
                    if (GivenName != null || inputSignal.Name == null)
                    {
                        varName = Scope + "." + varName;
                    }
                }
            }
            (rowNo, colNo) = inputSignal.Shape();
            FigureProtocol.FigureConfig figureConfig = new FigureProtocol.FigureConfig()
            {
                FigureId = publisher.Id,
                Title = varName,
                RowsCount = rowNo==0 ? 1 : rowNo,
                ColumnsCount = colNo==0 ? 1 : colNo
            };
            publisher.Send(FigureProtocol.FigureConfigLabel, figureConfig);
            if (inputSignal.IsMatrix)
            {
                for(int i = 0; i < rowNo; i++)
                {
                    for (int j = 0; j < colNo; j++)
                    {
                        string observationName = $"{varName}({i}-{j})";
                        observations.Add(Observation.CreateObservation(observationName));
                        FigureProtocol.PlotConfig plotConfig = new FigureProtocol.PlotConfig()
                        {
                            FigureId = publisher.Id,
                            RowNO = i,
                            ColumnNO = j,
                            XLabel = "time/s",
                            YLabel = observationName
                        };
                        publisher.Send(FigureProtocol.PlotConfigLabel, plotConfig);
                        FigureProtocol.Curve curve = new FigureProtocol.Curve()
                        {
                            FigureId = publisher.Id,
                            CurveHandle = i*colNo+j,
                            RowNO = i,
                            ColumnNO = j
                        };
                        publisher.Send(FigureProtocol.CurveLabel, curve);
                    }
                }
            }
            else
            {
                observations.Add(Observation.CreateObservation(varName));
                FigureProtocol.PlotConfig plotConfig = new FigureProtocol.PlotConfig()
                {
                    FigureId = publisher.Id,
                    RowNO = 0,
                    ColumnNO = 0,
                    XLabel = "time/s",
                    YLabel = varName
                };
                publisher.Send(FigureProtocol.PlotConfigLabel, plotConfig);
                FigureProtocol.Curve curve = new FigureProtocol.Curve()
                {
                    FigureId = publisher.Id,
                    CurveHandle = 0,
                    RowNO = 0,
                    ColumnNO = 0
                };
                publisher.Send(FigureProtocol.CurveLabel, curve); 
            }
            FigureProtocol.ShowCommand showCommand = new FigureProtocol.ShowCommand()
            {
                FigureId = publisher.Id
            };
            publisher.Send(FigureProtocol.ShowCommandLabel, showCommand);
            Calculate = PostCalculate;
            return Calculate(values);
        }
        private List<Signal> PostCalculate(List<Signal> values)
        {
            Signal inputSignal = values[0];
            inputSignal.ZipApply<Observation>(observations, (value, observation) => observation.Cache(value));
            return Results;
        }
        public override void Refresh()
        {
            for (int i=0; i<observations.Count; i++)
            {
                Observation observation = observations[i];
                FigureProtocol.Data data = new FigureProtocol.Data()
                {
                    FigureId = publisher.Id, 
                    CurveHandle = i, 
                    XValue = Solver.Time, 
                    YValue = observation.OutputVariable
                };
                publisher.Send(FigureProtocol.DataLabel, data);
            };
        }
        public override void Release()
        {
            for (int i=0; i<observations.Count; i++)
            {
                Observation observation = observations[i];
                FigureProtocol.DataFile dataFile = new FigureProtocol.DataFile()
                {
                    FigureId = publisher.Id, 
                    CurveHandle = i, 
                    FileName = Observation.DataFile,
                    XColumn = "Time",
                    YColumn = observation.Name
                };
                publisher.Send(FigureProtocol.DataFileLabel, dataFile);
            };
            // Settings settings = Settings.GetInstance();
            // settings.NeedOutput = true;
            // 
            // string dataFile = ;
            // table.DumpFile(dataFile, true);
            // List<string> scripts = new List<string>();
            // scripts.Add("# Auto-generated by Ligral(c)");
            // scripts.Add("import numpy as np");
            // scripts.Add("import pandas as pd");
            // scripts.Add("import matplotlib.pyplot as plt");
            // scripts.Add($"frame = pd.read_csv(r'{dataFile}')");
            // scripts.Add($"data = frame.values");
            // scripts.Add($"plt.figure(num='{varName}')");
            // scripts.Add("time = data.T[0]");
            // if (rowNo ==0 && colNo == 0)
            // {
            //     scripts.Add("plt.plot(time, data.T[1])");
            //     scripts.Add("plt.xlabel('time (s)')");
            //     scripts.Add("plt.ylabel('Data')");
            //     scripts.Add("plt.grid()");
            // }
            // else
            // {
            //     scripts.Add("for i, col in enumerate(data.T[1:]):");
            //     scripts.Add($"    plt.subplot({rowNo}, {colNo}, i+1)");
            //     scripts.Add("    plt.plot(time, col)");
            //     scripts.Add("    plt.xlabel('time (s)')");
            //     scripts.Add("    plt.ylabel(frame.columns[i+1])");
            //     scripts.Add("    plt.grid()");
            // }
            // scripts.Add($"plt.suptitle('{varName}')");
            // scripts.Add("plt.tight_layout()");
            // scripts.Add("plt.show()");
            // string scriptsFile = System.IO.Path.Join(currentDirectory, settings.OutputFolder, varName + ".py");
            // System.IO.File.WriteAllLines(scriptsFile, scripts);
            // System.Diagnostics.Process.Start("python", $"\"{scriptsFile}\"");
        }
    }
}