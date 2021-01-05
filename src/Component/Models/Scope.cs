/* Copyright 2019-2020 Junruoyu Zheng. All rights reserved.

   Distributed under MIT license.
   See file LICENSE for detail or copy at https://opensource.org/licenses/MIT
*/

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
        private Publisher publisher = new Publisher();
        protected override void SetUpPorts()
        {
            InPortList.Add(new InPort("input", this));
        }
        protected override void SetUpParameters()
        {
            Parameters = new ParameterDictionary()
            {
                {"name", new Parameter(ParameterType.String , value=>
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
            (int rowNo, int colNo) = inputSignal.Shape();
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
            Settings settings = Settings.GetInstance();
            if (settings.RealTimeSimulation)
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
        }
    }
}