/* Copyright (C) 2019-2022 Junruoyu Zheng. Home page: https://junruoyu-zheng.gitee.io/ligral

   Distributed under MIT license.
   See file LICENSE for detail or copy at https://opensource.org/licenses/MIT
*/

using System.Collections.Generic;
using System.Linq;
using ParameterDictionary = System.Collections.Generic.Dictionary<string, Ligral.Component.Parameter>;
using Ligral.Tools;
using Ligral.Simulation;
using Ligral.Tools.Protocols;
using MathNet.Numerics.LinearAlgebra;
using Ligral.Syntax.CodeASTs;

namespace Ligral.Component.Models
{
    class PhaseDiagram : Model
    {
        protected override string DocString
        {
            get
            {
                return "This model displays phase diagram and stores data to the output folder.";
            }
        }
        private string varName;
        private string xName;
        private string yName;
        private ObservationHandle xHandle;
        private ObservationHandle yHandle;
        private Publisher publisher = new Publisher();
        private delegate void SendPacketHandler();
        private SendPacketHandler SendData;
        private SendPacketHandler SendFile;
        protected override void SetUpPorts()
        {
            InPortList.Add(new InPort("x", this));
            InPortList.Add(new InPort("y", this));
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
            string xSignalName = InPortList[0].Source.SignalName;
            string ySignalName = InPortList[1].Source.SignalName;
            if (varName == null)
            {
                if (GivenName != null)
                {
                    if (Scope != null)
                    {
                        varName = Scope + "." + GivenName;
                    }
                    else
                    {
                        varName = GivenName;
                    }
                }
                else if (xSignalName != null && ySignalName != null)
                {
                    varName = $"{xSignalName}/{ySignalName}";
                    xName = xSignalName;
                    yName = ySignalName;
                }
                else
                {
                    if (Scope != null)
                    {
                        varName = Scope + "." + Name;
                    }
                    else
                    {
                        varName = Name;
                    }
                }
            }
            xName = xName ?? $"{varName}:x";
            yName = yName ?? $"{varName}:y";
        }
        public override void Check()
        {
            int xr = InPortList[0].RowNo;
            int xc = InPortList[0].ColNo;
            int yr = InPortList[1].RowNo;
            int yc = InPortList[1].ColNo;
            xHandle = Observation.CreateObservation(xName, xr, xc);
            yHandle = Observation.CreateObservation(yName, yr, yc);
            if (xr == 0 && xc == 0 && yr == 0 && yc == 0)
            {
                FigureConfigDoubleScalar(xName, yName);
                SendData = SendDataDoubleScalar;
                SendFile = SendFileDoubleScalar;
            }
            else if (xr == 0 && yr == 0)
            {
                FigureConfigScalarMatrix(yc, yr, xName, yName);
                SendData = SendDataScalarMatrix;
                SendFile = SendFileScalarMatrix;
            }
            else if (yr == 0 && yc == 0)
            {
                FigureConfigMatrixScalar(xc, xr, xName, yName);
                SendData = SendDataMatrixScalar;
                SendFile = SendFileMatrixScalar;
            }
            else if (xr == 1 && yc == 1)
            {
                FigureConfigHVectorVVector(xc, yr, xName, yName);
                SendData = SendDataHVectorVVector;
                SendFile = SendFileHVectorVVector;
            }
            else if (xc == 1 && yr == 1)
            {
                FigureConfigVVectorHVector(xr, yc, xName, yName);
                SendData = SendDataVVectorHVector;
                SendFile = SendFileVVectorHVector;
            }
            else
            {
                throw logger.Error(new ModelException(this, "PhaseDiagram only accepts [scalar, (m*n)] or [(1*m), (n*1)] or vice versa."));
            }
        }
        protected override List<Matrix<double>> Calculate(List<Matrix<double>> values)
        {
            Matrix<double> xSignal = values[0];
            Matrix<double> ySignal = values[1];
            xHandle.Cache(xSignal);
            yHandle.Cache(ySignal);
            return Results;
        }
        public override void Refresh()
        {
            Settings settings = Settings.GetInstance();
            if (settings.RealTimeSimulation && SendData != null)
            {
                SendData();
            }
        }
        public override void Release()
        {
            if (SendFile != null)
            {
                SendFile();
            }
        }
        #region double-scalar
        private void FigureConfigDoubleScalar(string xName, string yName)
        {
            FigureProtocol.FigureConfig figureConfig = new FigureProtocol.FigureConfig()
            {
                FigureId = publisher.Id,
                Title = varName,
                RowsCount = 1,
                ColumnsCount = 1
            };
            publisher.Send(FigureProtocol.FigureConfigLabel, figureConfig);
            FigureProtocol.PlotConfig plotConfig = new FigureProtocol.PlotConfig()
            {
                FigureId = publisher.Id,
                RowNO = 0,
                ColumnNO = 0,
                XLabel = xName,
                YLabel = yName
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
        private void SendDataDoubleScalar()
        {
            FigureProtocol.Data data = new FigureProtocol.Data()
            {
                FigureId = publisher.Id, 
                CurveHandle = 0, 
                XValue = xHandle.space[0].OutputVariable, 
                YValue = yHandle.space[0].OutputVariable
            };
            publisher.Send(FigureProtocol.DataLabel, data);
        }
        private void SendFileDoubleScalar()
        {
            FigureProtocol.DataFile dataFile = new FigureProtocol.DataFile()
            {
                FigureId = publisher.Id, 
                CurveHandle = 0, 
                FileName = Observation.DataFile,
                XColumn = xHandle.space[0].Name,
                YColumn = yHandle.space[0].Name
            };
            publisher.Send(FigureProtocol.DataFileLabel, dataFile);
        }
        #endregion
        #region y-matrix
        private void FigureConfigScalarMatrix(int yc, int yr, string xName, string yName)
        {
            FigureProtocol.FigureConfig figureConfig = new FigureProtocol.FigureConfig()
            {
                FigureId = publisher.Id,
                Title = varName,
                RowsCount = yc,
                ColumnsCount = yr
            };
            publisher.Send(FigureProtocol.FigureConfigLabel, figureConfig);
            for(int i = 0; i < yr; i++)
            {
                for (int j = 0; j < yc; j++)
                {
                    string observationName = $"{yName}({i}-{j})";
                    FigureProtocol.PlotConfig plotConfig = new FigureProtocol.PlotConfig()
                    {
                        FigureId = publisher.Id,
                        RowNO = i,
                        ColumnNO = j,
                        XLabel = xName,
                        YLabel = observationName
                    };
                    publisher.Send(FigureProtocol.PlotConfigLabel, plotConfig);
                    FigureProtocol.Curve curve = new FigureProtocol.Curve()
                    {
                        FigureId = publisher.Id,
                        CurveHandle = i*yc+j,
                        RowNO = i,
                        ColumnNO = j
                    };
                    publisher.Send(FigureProtocol.CurveLabel, curve);
                }
            }
        }
        private void SendDataScalarMatrix()
        {
            for (int i=0; i<yHandle.space.Count; i++)
            {
                FigureProtocol.Data data = new FigureProtocol.Data()
                {
                    FigureId = publisher.Id, 
                    CurveHandle = i, 
                    XValue = xHandle.space[0].OutputVariable, 
                    YValue = yHandle.space[i].OutputVariable
                };
                publisher.Send(FigureProtocol.DataLabel, data);
            }
        }
        private void SendFileScalarMatrix()
        {
            for (int i=0; i<yHandle.space.Count; i++)
            {
                FigureProtocol.DataFile dataFile = new FigureProtocol.DataFile()
                {
                    FigureId = publisher.Id, 
                    CurveHandle = i, 
                    FileName = Observation.DataFile,
                    XColumn = xHandle.space[0].Name,
                    YColumn = yHandle.space[i].Name
                };
                publisher.Send(FigureProtocol.DataFileLabel, dataFile);
            }
        }
        #endregion
        #region x-matrix
        private void FigureConfigMatrixScalar(int xc, int xr, string xName, string yName)
        {
            FigureProtocol.FigureConfig figureConfig = new FigureProtocol.FigureConfig()
            {
                FigureId = publisher.Id,
                Title = varName,
                RowsCount = xc,
                ColumnsCount = xr
            };
            publisher.Send(FigureProtocol.FigureConfigLabel, figureConfig);
            for(int i = 0; i < xr; i++)
            {
                for (int j = 0; j < xc; j++)
                {
                    string observationName = $"{xName}({i}-{j})";
                    FigureProtocol.PlotConfig plotConfig = new FigureProtocol.PlotConfig()
                    {
                        FigureId = publisher.Id,
                        RowNO = i,
                        ColumnNO = j,
                        XLabel = observationName,
                        YLabel = yName
                    };
                    publisher.Send(FigureProtocol.PlotConfigLabel, plotConfig);
                    FigureProtocol.Curve curve = new FigureProtocol.Curve()
                    {
                        FigureId = publisher.Id,
                        CurveHandle = i*xc+j,
                        RowNO = i,
                        ColumnNO = j
                    };
                    publisher.Send(FigureProtocol.CurveLabel, curve);
                }
            }
        }
        private void SendDataMatrixScalar()
        {
            for (int i=0; i<xHandle.space.Count; i++)
            {
                FigureProtocol.Data data = new FigureProtocol.Data()
                {
                    FigureId = publisher.Id, 
                    CurveHandle = i, 
                    XValue = xHandle.space[i].OutputVariable, 
                    YValue = yHandle.space[0].OutputVariable
                };
                publisher.Send(FigureProtocol.DataLabel, data);
            }
        }
        private void SendFileMatrixScalar()
        {
            for (int i=0; i<xHandle.space.Count; i++)
            {
                FigureProtocol.DataFile dataFile = new FigureProtocol.DataFile()
                {
                    FigureId = publisher.Id, 
                    CurveHandle = i, 
                    FileName = Observation.DataFile,
                    XColumn = xHandle.space[i].Name,
                    YColumn = yHandle.space[0].Name
                };
                publisher.Send(FigureProtocol.DataFileLabel, dataFile);
            }
        }
        #endregion
        #region vector-hor-ver
        private void FigureConfigHVectorVVector(int xc, int yr, string xName, string yName)
        {
            FigureProtocol.FigureConfig figureConfig = new FigureProtocol.FigureConfig()
            {
                FigureId = publisher.Id,
                Title = varName,
                RowsCount = xc,
                ColumnsCount = yr
            };
            publisher.Send(FigureProtocol.FigureConfigLabel, figureConfig);
            for (int i = 0; i < xc; i++)
            {
                string observationName = $"{xName}({i})";
            }
            for (int i = 0; i < yr; i++)
            {
                string observationName = $"{yName}({i})";
                for (int j = 0; j < xc; j++)
                {
                    FigureProtocol.PlotConfig plotConfig = new FigureProtocol.PlotConfig()
                    {
                        FigureId = publisher.Id,
                        RowNO = i,
                        ColumnNO = j,
                        XLabel = xHandle.space[j].Name,
                        YLabel = observationName
                    };
                    publisher.Send(FigureProtocol.PlotConfigLabel, plotConfig);
                    FigureProtocol.Curve curve = new FigureProtocol.Curve()
                    {
                        FigureId = publisher.Id,
                        CurveHandle = i*xc+j,
                        RowNO = i,
                        ColumnNO = j
                    };
                    publisher.Send(FigureProtocol.CurveLabel, curve);
                }
            }
        }
        private void SendDataHVectorVVector()
        {
            for (int j = 0; j < yHandle.space.Count; j++)
            {
                for (int i = 0; i< xHandle.space.Count; i++)
                {
                    FigureProtocol.Data data = new FigureProtocol.Data()
                    {
                        FigureId = publisher.Id, 
                        CurveHandle = j * xHandle.space.Count + i, 
                        XValue = xHandle.space[i].OutputVariable, 
                        YValue = yHandle.space[j].OutputVariable
                    };
                    publisher.Send(FigureProtocol.DataLabel, data);
                }
            }
        }
        private void SendFileHVectorVVector()
        {
            for (int j = 0; j < yHandle.space.Count; j++)
            {
                for (int i = 0; i< xHandle.space.Count; i++)
                {
                    FigureProtocol.DataFile dataFile = new FigureProtocol.DataFile()
                    {
                        FigureId = publisher.Id, 
                        CurveHandle = j * xHandle.space.Count + i, 
                        FileName = Observation.DataFile,
                        XColumn = xHandle.space[i].Name, 
                        YColumn = yHandle.space[j].Name
                    };
                    publisher.Send(FigureProtocol.DataFileLabel, dataFile);
                }
            }
        }
        #endregion
        #region vector-ver-hor
        private void FigureConfigVVectorHVector(int xr, int yc, string xName, string yName)
        {
            FigureProtocol.FigureConfig figureConfig = new FigureProtocol.FigureConfig()
            {
                FigureId = publisher.Id,
                Title = varName,
                RowsCount = yc,
                ColumnsCount = xr
            };
            publisher.Send(FigureProtocol.FigureConfigLabel, figureConfig);
            for (int i = 0; i < yc; i++)
            {
                string observationName = $"{yName}({i})";
            }
            for (int i = 0; i < xr; i++)
            {
                string observationName = $"{xName}({i})";
                for (int j = 0; j < yc; j++)
                {
                    FigureProtocol.PlotConfig plotConfig = new FigureProtocol.PlotConfig()
                    {
                        FigureId = publisher.Id,
                        RowNO = i,
                        ColumnNO = j,
                        XLabel = observationName,
                        YLabel = yHandle.space[j].Name
                    };
                    publisher.Send(FigureProtocol.PlotConfigLabel, plotConfig);
                    FigureProtocol.Curve curve = new FigureProtocol.Curve()
                    {
                        FigureId = publisher.Id,
                        CurveHandle = i*yc+j,
                        RowNO = i,
                        ColumnNO = j
                    };
                    publisher.Send(FigureProtocol.CurveLabel, curve);
                }
            }
        }
        private void SendDataVVectorHVector()
        {
            for (int i = 0; i< xHandle.space.Count; i++)
            {
                for (int j = 0; j < yHandle.space.Count; j++)
                {
                    FigureProtocol.Data data = new FigureProtocol.Data()
                    {
                        FigureId = publisher.Id, 
                        CurveHandle = i * yHandle.space.Count + j, 
                        XValue = xHandle.space[i].OutputVariable, 
                        YValue = yHandle.space[j].OutputVariable
                    };
                    publisher.Send(FigureProtocol.DataLabel, data);
                }
            }
        }
        private void SendFileVVectorHVector()
        {
            for (int i = 0; i< xHandle.space.Count; i++)
            {
                for (int j = 0; j < yHandle.space.Count; j++)
                {
                    FigureProtocol.DataFile dataFile = new FigureProtocol.DataFile()
                    {
                        FigureId = publisher.Id, 
                        CurveHandle = i * yHandle.space.Count + j, 
                        FileName = Observation.DataFile,
                        XColumn = xHandle.space[i].Name, 
                        YColumn = yHandle.space[j].Name
                    };
                    publisher.Send(FigureProtocol.DataFileLabel, dataFile);
                }
            }
        }
        #endregion
        public override List<CodeAST> ConstructConfigurationAST()
        {
            return OutputSink.ConstructConfigurationAST(GlobalName, xHandle);
        }
    }
}