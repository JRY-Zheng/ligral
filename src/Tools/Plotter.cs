using System.Diagnostics;
using System.IO;
using System;
using System.Collections.Generic;
using Ligral.Tools.Protocols;
using Ligral.Simulation;

namespace Ligral.Tools
{
    enum AxesShapeType
    {
        Scalar, Vector, Matrix
    }
    class Figure
    {
        public bool Showed 
        {
            get
            {
                foreach (int handle in Curves.Keys)
                {
                    if (!Curves[handle].Showed) return false;
                }
                return true;
            }
            set
            {
                foreach (int handle in Curves.Keys)
                {
                    Curves[handle].Showed = value;
                }
            }
        }
        public bool Paused 
        {
            get
            {
                foreach (int handle in Curves.Keys)
                {
                    if (!Curves[handle].Paused) return false;
                }
                return true;
            }
            set
            {
                foreach (int handle in Curves.Keys)
                {
                    Curves[handle].Paused = value;
                }
            }
        }
        public string Name;
        public AxesShapeType AxesShape;
        public Dictionary<int, Curve> Curves = new Dictionary<int, Curve>();
    }
    class Curve
    {
        public List<double> CachedX = new List<double>();
        public List<double> CachedY = new List<double>();
        public bool Paused = false;
        public bool Showed = false;
        public int RowNO;
        public int ColNO;
        public DateTime CachedTime;
    }
    class Plotter : Subscriber, IConfigure
    {
        protected Process PythonProcess;
        protected Dictionary<int, Figure> Figures = new Dictionary<int, Figure>();
        protected Dictionary<string, int> Files = new Dictionary<string, int>();
        protected bool OutputScript = false;
        protected bool SaveFigure = false;
        protected StreamWriter ScriptsStream;
        public Plotter()
        {
            Settings settings = Settings.GetInstance();
            PythonProcess = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = settings.PythonExecutable,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    RedirectStandardInput = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true
                }
            };
            try
            {
                PythonProcess.Start();
            }
            catch (System.ComponentModel.Win32Exception)
            {
                throw logger.Error(new LigralException($"{settings.PythonExecutable} is not installed or not callable."));
            }
        }
        protected virtual void Execute(string command)
        {
            if (OutputScript) ScriptsStream.Write(command);
            PythonProcess.StandardInput.Write(command);
        }
        public override void Unsubscribe()
        {
            if (Publisher.ContainsHooks(this)) Publisher.RemoveHooks(this);
            base.Unsubscribe();
            Execute("exit()");
            ScriptsStream.Close();
        }
        protected override bool Receive(FigureProtocol.FigureConfig figureConfig)
        {
            Execute($@"
fig{figureConfig.FigureId}, ax{figureConfig.FigureId} = plt.subplots({figureConfig.RowsCount}, {figureConfig.ColumnsCount}, num='{figureConfig.Title}')
fig{figureConfig.FigureId}.suptitle('{figureConfig.Title}')
");
            Figure figure = new Figure();
            if (figureConfig.RowsCount == 1 && figureConfig.ColumnsCount == 1)
            {
                figure.AxesShape = AxesShapeType.Scalar;
            }
            else if (figureConfig.RowsCount == 1 || figureConfig.ColumnsCount == 1)
            {
                figure.AxesShape = AxesShapeType.Vector;
            }
            else
            {
                figure.AxesShape = AxesShapeType.Matrix;
            }
            figure.Name = figureConfig.Title;
            Figures[figureConfig.FigureId] = figure;
            return true;
        }
        protected override bool Receive(FigureProtocol.PlotConfig plotConfig)
        {
            if (Figures.ContainsKey(plotConfig.FigureId))
            switch (Figures[plotConfig.FigureId].AxesShape)
            {
            case AxesShapeType.Scalar:
                Execute($@"
ax{plotConfig.FigureId}.set_xlabel('{plotConfig.XLabel}')
ax{plotConfig.FigureId}.set_ylabel('{plotConfig.YLabel}')
ax{plotConfig.FigureId}.grid()
");
                break;
            case AxesShapeType.Vector:
                int index = plotConfig.RowNO > plotConfig.ColumnNO ? plotConfig.RowNO : plotConfig.ColumnNO;
                Execute($@"
ax = ax{plotConfig.FigureId}[{index}]
ax.set_xlabel({plotConfig.XLabel})
ax.set_ylabel({plotConfig.YLabel})
ax.grid()
");
                break;
            case AxesShapeType.Matrix:
                Execute($@"
ax = ax{plotConfig.FigureId}[{plotConfig.RowNO}, {plotConfig.ColumnNO}]
ax.set_xlabel({plotConfig.XLabel})
ax.set_ylabel({plotConfig.YLabel})
ax.grid()
");
                break;
            }
            return true;
        }
        protected override bool Receive(FigureProtocol.Curve curve)
        {
            if (!Figures.ContainsKey(curve.FigureId)) return false;
            Figure figure = Figures[curve.FigureId];
            Curve cv = new Curve();
            figure.Curves[curve.CurveHandle] = cv;
            cv.ColNO = curve.ColumnNO;
            cv.RowNO = curve.RowNO;
            return true;
        }

        protected override bool Receive(FigureProtocol.ShowCommand showCommand)
        {
            return true;
        }
        protected override bool Receive(FigureProtocol.Data data)
        {
            return true;
        }
        protected override bool Receive(FigureProtocol.DataFile dataFile)
        {
            if (!Figures.ContainsKey(dataFile.FigureId)) return false;
            Figure figure = Figures[dataFile.FigureId];
            if (!figure.Curves.ContainsKey(dataFile.CurveHandle)) return false;
            Curve curve = figure.Curves[dataFile.CurveHandle];
            string fileName = Path.GetFullPath(dataFile.FileName);
            if (!Files.ContainsKey(fileName))
            {
                Execute($@"
df{Files.Count} = pd.read_csv(r'{fileName}', skipinitialspace=True)
");
                Files[fileName] = Files.Count;
            }
            Execute($@"
x = df{Files[fileName]}['{dataFile.XColumn}'].values
y = df{Files[fileName]}['{dataFile.YColumn}'].values
");
            switch (figure.AxesShape)
            {
            case AxesShapeType.Scalar:
                Execute($@"
ax{dataFile.FigureId}.plot(x, y)
");
                break;
            case AxesShapeType.Vector:
                int index = curve.RowNO > curve.ColNO ? curve.RowNO : curve.ColNO;
                Execute($@"
ax{dataFile.FigureId}[{index}].plot(x, y)
");
                break;
            case AxesShapeType.Matrix:
                Execute($@"
ax{dataFile.FigureId}[{curve.RowNO}, {curve.ColNO}].plot(x, y)
ax.grid()
");
                break;
            }
            curve.Showed = true;
            if (SaveFigure && figure.Showed) 
            {
                string figName = figure.Name;
                foreach (char invalidChar in Path.GetInvalidFileNameChars())
                {
                    figName = figName.Replace(invalidChar, '_');
                }
                Execute($@"
plt.savefig(r'{Path.GetDirectoryName(fileName)}\{figName}.png')
");
            }
            foreach (var fig in Figures.Values)
            {
                if (!fig.Showed) return true;
            }
            Execute($@"
plt.show()
");
            Unsubscribe();
            return true;
        }
        public virtual void Configure(Dictionary<string, object> dict)
        {
            foreach (string item in dict.Keys)
            {
                object val = dict[item];
                try
                {
                    switch (item)
                    {
                    case "enable":
                        if ((bool) dict[item])
                        {
                            if (!Tools.Publisher.ContainsHooks(this))
                            {
                                Tools.Publisher.AddHooks(this);
                            }
                        }
                        else
                        {
                            if (Tools.Publisher.ContainsHooks(this))
                            {
                                Tools.Publisher.RemoveHooks(this);
                            }
                        }
                        break;
                    case "output_script":
                    case "output_scripts":
                        OutputScript = (bool) dict[item]; break;
                    case "save_figure":
                    case "save_figures":
                        SaveFigure = (bool) dict[item]; break;
                    default:
                        throw new SettingException(item, dict[item], "Unsupported setting in plotter.");
                    }
                }
                catch (System.InvalidCastException)
                {
                    throw logger.Error(new SettingException(item, val, $"Invalid type {val.GetType()} in plotter"));
                }
            }
            Settings settings = Settings.GetInstance();
            string currentDirectory = System.IO.Directory.GetCurrentDirectory();
            string scripts = System.IO.Path.Join(currentDirectory, settings.OutputFolder, "plot.py");
            if (OutputScript) ScriptsStream = new StreamWriter(scripts);
            Execute(@"# Auto-generated by Ligral (c) 2020
import matplotlib.pyplot as plt
import numpy as np
import pandas as pd
");
        }
    }
}