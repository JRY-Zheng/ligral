/* Copyright (C) 2019-2022 Junruoyu Zheng. Home page: https://junruoyu-zheng.gitee.io/ligral

   Distributed under MIT license.
   See file LICENSE for detail or copy at https://opensource.org/licenses/MIT
*/

using System.Collections.Generic;
using ParameterDictionary = System.Collections.Generic.Dictionary<string, Ligral.Component.Parameter>;
using Ligral.Tools;
using Ligral.Tools.Protocols;
using Ligral.Simulation;
using MathNet.Numerics.LinearAlgebra;
using Ligral.Syntax.CodeASTs;

namespace Ligral.Component.Models
{
    enum MergeOption
    {
        None, Column, Row
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
        private string varName;
        private ObservationHandle handle;
        private int rowNo;
        private int colNo;
        private MergeOption mergeOption = MergeOption.None;
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
                }, ()=> {})},
                {"merge", new Parameter(ParameterType.String , value=>
                {
                    switch ((string)value)
                    {
                    case "column":
                        mergeOption = MergeOption.Column;
                        break;
                    case "row":
                        mergeOption = MergeOption.Row;
                        break;
                    case "none":
                        mergeOption = MergeOption.None;
                        break;
                    default:
                        throw logger.Error(new ModelException(this, $"Merge option must be column, row or none, but got {value}"));
                    }
                }, ()=> {})}
            };
        }
        public override void Prepare()
        {
            varName = Model.GetVarName(varName, this);
        }
        public override void Check()
        {
            rowNo = InPortList[0].RowNo;
            colNo = InPortList[0].ColNo;
            FigureProtocol.FigureConfig figureConfig = new FigureProtocol.FigureConfig()
            {
                FigureId = publisher.Id,
                Title = varName,
                RowsCount = mergeOption == MergeOption.Column ? 1 : rowNo==0 ? 1 : rowNo,
                ColumnsCount = mergeOption == MergeOption.Row ? 1 : colNo==0 ? 1 : colNo
            };
            publisher.Send(FigureProtocol.FigureConfigLabel, figureConfig);
            handle = Observation.CreateObservation(varName, rowNo, colNo);
            if (rowNo > 0 && colNo > 0)
            {
                if (mergeOption == MergeOption.Column) ColumnMerge();
                else if (mergeOption == MergeOption.Row) RowMerge();
                else NoneMerge();
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
        private void NoneMerge()
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
        private void RowMerge()
        {
            for(int i = 0; i < rowNo; i++)
            {
                string observationName = $"{varName}({i})";
                FigureProtocol.PlotConfig plotConfig = new FigureProtocol.PlotConfig()
                {
                    FigureId = publisher.Id,
                    RowNO = i,
                    ColumnNO = 0,
                    XLabel = "time/s",
                    YLabel = observationName
                };
                publisher.Send(FigureProtocol.PlotConfigLabel, plotConfig);
                for (int j = 0; j < colNo; j++)
                {
                    FigureProtocol.Curve curve = new FigureProtocol.Curve()
                    {
                        FigureId = publisher.Id,
                        CurveHandle = i*colNo+j,
                        RowNO = i,
                        ColumnNO = 0
                    };
                    publisher.Send(FigureProtocol.CurveLabel, curve);
                }
            }
        }
        private void ColumnMerge()
        {
            for(int i = 0; i < rowNo; i++)
            {
                for (int j = 0; j < colNo; j++)
                {
                    if ( i == 0 )
                    {
                        string observationName = $"{varName}({j})";
                        FigureProtocol.PlotConfig plotConfig = new FigureProtocol.PlotConfig()
                        {
                            FigureId = publisher.Id,
                            RowNO = 0,
                            ColumnNO = j,
                            XLabel = "time/s",
                            YLabel = observationName
                        };
                        publisher.Send(FigureProtocol.PlotConfigLabel, plotConfig);
                    }
                    FigureProtocol.Curve curve = new FigureProtocol.Curve()
                    {
                        FigureId = publisher.Id,
                        CurveHandle = i*colNo+j,
                        RowNO = 0,
                        ColumnNO = j
                    };
                    publisher.Send(FigureProtocol.CurveLabel, curve);
                }
            }
        }
        protected override List<Matrix<double>> Calculate(List<Matrix<double>> values)
        {
            Matrix<double> inputSignal = values[0];
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
            logger.Debug("Figure show command sent");
        }
        public override List<CodeAST> ConstructConfigurationAST()
        {
            return OutputSink.ConstructConfigurationAST(GlobalName, handle);
        }
    }
}