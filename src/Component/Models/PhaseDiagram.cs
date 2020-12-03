using System.Collections.Generic;
using System.Linq;
using ParameterDictionary = System.Collections.Generic.Dictionary<string, Ligral.Component.Parameter>;
using Ligral.Tools;
using Ligral.Simulation;
using Ligral.Tools.Protocols;

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
        private List<Observation> xObservations = new List<Observation>();
        private List<Observation> yObservations = new List<Observation>();
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
        protected override List<Signal> DefaultCalculate(List<Signal> values)
        {
            Signal xSignal = values[0];
            Signal ySignal = values[1];
            string xName = null, yName = null;
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
                else if (xSignal.Name != null && ySignal.Name != null)
                {
                    varName = $"{xSignal.Name}/{ySignal.Name}";
                    xName = xSignal.Name;
                    yName = ySignal.Name;
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
            (int xr, int xc) = xSignal.Shape();
            (int yr, int yc) = ySignal.Shape();
            if (!xSignal.IsMatrix && !ySignal.IsMatrix)
            {
                FigureConfigDoubleScalar(xName, yName);
                SendData = SendDataDoubleScalar;
                SendFile = SendFileDoubleScalar;
            }
            else if (!xSignal.IsMatrix)
            {
                FigureConfigScalarMatrix(yc, yr, xName, yName);
                SendData = SendDataScalarMatrix;
                SendFile = SendFileScalarMatrix;
            }
            else if (!ySignal.IsMatrix)
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
                throw new ModelException(this, "PhaseDiagram only accepts [scalar, (m*n)] or [(1*m), (n*1)] or vice versa.");
            }
            Calculate = PostCalculate;
            return Calculate(values);
        }
        public override void Refresh()
        {
            Settings settings = Settings.GetInstance();
            if (settings.RealTimeDrawing && SendData != null)
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
        private List<Signal> PostCalculate(List<Signal> values)
        {
            Signal xSignal = values[0];
            Signal ySignal = values[1];
            (int xr, int xc) = xSignal.Shape();
            xSignal.ZipApply<Observation>(xObservations, (val, obs) => obs.Cache(val));
            ySignal.ZipApply<Observation>(yObservations, (val, obs) => obs.Cache(val));
            return Results;
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
            xObservations.Add(Observation.CreateObservation(xName));
            yObservations.Add(Observation.CreateObservation(yName));
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
                XValue = xObservations[0].OutputVariable, 
                YValue = yObservations[0].OutputVariable
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
                XColumn = xObservations[0].Name,
                YColumn = yObservations[0].Name
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
            xObservations.Add(Observation.CreateObservation(xName));
            for(int i = 0; i < yr; i++)
            {
                for (int j = 0; j < yc; j++)
                {
                    string observationName = $"{yName}({i}-{j})";
                    yObservations.Add(Observation.CreateObservation(observationName));
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
            for (int i=0; i<yObservations.Count; i++)
            {
                FigureProtocol.Data data = new FigureProtocol.Data()
                {
                    FigureId = publisher.Id, 
                    CurveHandle = i, 
                    XValue = xObservations[0].OutputVariable, 
                    YValue = yObservations[i].OutputVariable
                };
                publisher.Send(FigureProtocol.DataLabel, data);
            }
        }
        private void SendFileScalarMatrix()
        {
            for (int i=0; i<yObservations.Count; i++)
            {
                FigureProtocol.DataFile dataFile = new FigureProtocol.DataFile()
                {
                    FigureId = publisher.Id, 
                    CurveHandle = i, 
                    FileName = Observation.DataFile,
                    XColumn = xObservations[0].Name,
                    YColumn = yObservations[i].Name
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
                    xObservations.Add(Observation.CreateObservation(observationName));
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
            yObservations.Add(Observation.CreateObservation(yName));
        }
        private void SendDataMatrixScalar()
        {
            for (int i=0; i<xObservations.Count; i++)
            {
                FigureProtocol.Data data = new FigureProtocol.Data()
                {
                    FigureId = publisher.Id, 
                    CurveHandle = i, 
                    XValue = xObservations[i].OutputVariable, 
                    YValue = yObservations[0].OutputVariable
                };
                publisher.Send(FigureProtocol.DataLabel, data);
            }
        }
        private void SendFileMatrixScalar()
        {
            for (int i=0; i<xObservations.Count; i++)
            {
                FigureProtocol.DataFile dataFile = new FigureProtocol.DataFile()
                {
                    FigureId = publisher.Id, 
                    CurveHandle = i, 
                    FileName = Observation.DataFile,
                    XColumn = xObservations[i].Name,
                    YColumn = yObservations[0].Name
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
                xObservations.Add(Observation.CreateObservation(observationName));
            }
            for (int i = 0; i < yr; i++)
            {
                string observationName = $"{yName}({i})";
                yObservations.Add(Observation.CreateObservation(observationName));
                for (int j = 0; j < xc; j++)
                {
                    FigureProtocol.PlotConfig plotConfig = new FigureProtocol.PlotConfig()
                    {
                        FigureId = publisher.Id,
                        RowNO = i,
                        ColumnNO = j,
                        XLabel = xObservations[j].Name,
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
            for (int j = 0; j < yObservations.Count; j++)
            {
                for (int i = 0; i< xObservations.Count; i++)
                {
                    FigureProtocol.Data data = new FigureProtocol.Data()
                    {
                        FigureId = publisher.Id, 
                        CurveHandle = j * xObservations.Count + i, 
                        XValue = xObservations[i].OutputVariable, 
                        YValue = yObservations[j].OutputVariable
                    };
                    publisher.Send(FigureProtocol.DataLabel, data);
                }
            }
        }
        private void SendFileHVectorVVector()
        {
            for (int j = 0; j < yObservations.Count; j++)
            {
                for (int i = 0; i< xObservations.Count; i++)
                {
                    FigureProtocol.DataFile dataFile = new FigureProtocol.DataFile()
                    {
                        FigureId = publisher.Id, 
                        CurveHandle = j * xObservations.Count + i, 
                        FileName = Observation.DataFile,
                        XColumn = xObservations[i].Name, 
                        YColumn = yObservations[j].Name
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
                yObservations.Add(Observation.CreateObservation(observationName));
            }
            for (int i = 0; i < xr; i++)
            {
                string observationName = $"{xName}({i})";
                xObservations.Add(Observation.CreateObservation(observationName));
                for (int j = 0; j < yc; j++)
                {
                    FigureProtocol.PlotConfig plotConfig = new FigureProtocol.PlotConfig()
                    {
                        FigureId = publisher.Id,
                        RowNO = i,
                        ColumnNO = j,
                        XLabel = observationName,
                        YLabel = yObservations[j].Name
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
            for (int i = 0; i< xObservations.Count; i++)
            {
                for (int j = 0; j < yObservations.Count; j++)
                {
                    FigureProtocol.Data data = new FigureProtocol.Data()
                    {
                        FigureId = publisher.Id, 
                        CurveHandle = i * yObservations.Count + j, 
                        XValue = xObservations[i].OutputVariable, 
                        YValue = yObservations[j].OutputVariable
                    };
                    publisher.Send(FigureProtocol.DataLabel, data);
                }
            }
        }
        private void SendFileVVectorHVector()
        {
            for (int i = 0; i< xObservations.Count; i++)
            {
                for (int j = 0; j < yObservations.Count; j++)
                {
                    FigureProtocol.DataFile dataFile = new FigureProtocol.DataFile()
                    {
                        FigureId = publisher.Id, 
                        CurveHandle = i * yObservations.Count + j, 
                        FileName = Observation.DataFile,
                        XColumn = xObservations[i].Name, 
                        YColumn = yObservations[j].Name
                    };
                    publisher.Send(FigureProtocol.DataFileLabel, dataFile);
                }
            }
        }
        #endregion
    }
}