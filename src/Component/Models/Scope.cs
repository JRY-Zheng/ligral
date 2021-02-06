/* Copyright (C) 2019-2021 Junruoyu Zheng. Home page: https://junruoyu-zheng.gitee.io/ligral

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
        private ObservationHandle handle;
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
        public override void Prepare()
        {
            string inputSignalName = InPortList[0].Source.SignalName;
            if (varName == null)
            {
                varName = GivenName ?? inputSignalName ?? Name;
                if (Scope != null)
                {
                    if (GivenName != null || inputSignalName == null)
                    {
                        varName = Scope + "." + varName;
                    }
                }
            }
        }
        public override void Check()
        {
            int rowNo = InPortList[0].RowNo;
            int colNo = InPortList[0].ColNo;
            FigureProtocol.FigureConfig figureConfig = new FigureProtocol.FigureConfig()
            {
                FigureId = publisher.Id,
                Title = varName,
                RowsCount = rowNo==0 ? 1 : rowNo,
                ColumnsCount = colNo==0 ? 1 : colNo
            };
            publisher.Send(FigureProtocol.FigureConfigLabel, figureConfig);
            handle = Observation.CreateObservation(varName, rowNo, colNo);
            if (rowNo > 0 && colNo > 1)
            {
                for(int i = 0; i < rowNo; i++)
                {
                    for (int j = 0; j < colNo; j++)
                    {
                        string observationName = $"{varName}({i}-{j})";
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
        }
        protected override List<Signal> DefaultCalculate(List<Signal> values)
        {
            Calculate = PostCalculate;
            return Calculate(values);
        }
        private List<Signal> PostCalculate(List<Signal> values)
        {
            Signal inputSignal = values[0];
            handle.Cache(inputSignal);
            return Results;
        }
        public override void Refresh()
        {
            Settings settings = Settings.GetInstance();
            if (settings.RealTimeSimulation)
            {
                for (int i=0; i<handle.space.Count; i++)
                {
                    Observation observation = handle.space[i];
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
            for (int i=0; i<handle.space.Count; i++)
            {
                Observation observation = handle.space[i];
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