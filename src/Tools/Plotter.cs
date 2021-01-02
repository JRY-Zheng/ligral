using System.Diagnostics;
using System.IO;
using System.Collections.Generic;
using Ligral.Tools.Protocols;
using CurveDictionary = System.Collections.Generic.Dictionary<int, System.Collections.Generic.Dictionary<int, (int, int)>>;

namespace Ligral.Tools
{
    class Plotter : Subscriber
    {
        protected Process PythonProcess;
        protected Dictionary<int, int> AxesSize = new Dictionary<int, int>();
        protected Dictionary<int, bool> Showed = new Dictionary<int, bool>();
        protected CurveDictionary Curves = new CurveDictionary();
        protected Dictionary<string, int> Files = new Dictionary<string, int>();
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
                throw logger.Error(new LigralException("Python is not installed or not callable."));
            }
            string currentDirectory = System.IO.Directory.GetCurrentDirectory();
            string scripts = System.IO.Path.Join(currentDirectory, settings.OutputFolder, "plot.py");
            ScriptsStream = new StreamWriter(scripts);
            Execute(@"# Auto-generated by Ligral (c) 2020
import matplotlib.pyplot as plt
import numpy as np
import pandas as pd
");
        }
        protected void Execute(string command)
        {
            ScriptsStream.WriteLine(command);
            PythonProcess.StandardInput.WriteLine(command);
        }
        public override void Unsubscribe()
        {
            base.Unsubscribe();
            Execute("exit()");
            // PythonProcess.WaitForExit();
            ScriptsStream.Close();
        }
        protected override bool Receive(FigureProtocol.FigureConfig figureConfig)
        {
            Execute($@"
fig{figureConfig.FigureId}, ax{figureConfig.FigureId} = plt.subplots({figureConfig.RowsCount}, {figureConfig.ColumnsCount}, num='{figureConfig.Title}')
fig{figureConfig.FigureId}.suptitle('{figureConfig.Title}')
");
            if (figureConfig.RowsCount == 1 && figureConfig.ColumnsCount == 1)
            {
                AxesSize[figureConfig.FigureId] = 0;
            }
            else if (figureConfig.RowsCount == 1 || figureConfig.ColumnsCount == 1)
            {
                AxesSize[figureConfig.FigureId] = 1;
            }
            else
            {
                AxesSize[figureConfig.FigureId] = 2;
            }
            Showed[figureConfig.FigureId] = false;
            return true;
        }
        protected override bool Receive(FigureProtocol.PlotConfig plotConfig)
        {
            if (AxesSize.ContainsKey(plotConfig.FigureId))
            switch (AxesSize[plotConfig.FigureId])
            {
            case 0:
                Execute($@"
ax{plotConfig.FigureId}.set_xlabel('{plotConfig.XLabel}')
ax{plotConfig.FigureId}.set_ylabel('{plotConfig.YLabel}')
ax{plotConfig.FigureId}.grid()
");
                break;
            case 1:
                int index = plotConfig.RowNO > plotConfig.ColumnNO ? plotConfig.RowNO : plotConfig.ColumnNO;
                Execute($@"
ax = ax{plotConfig.FigureId}[{index}]
ax.set_xlabel({plotConfig.XLabel})
ax.set_ylabel({plotConfig.YLabel})
ax.grid()
");
                break;
            case 2:
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
        protected override bool Receive(FigureProtocol.DataFile dataFile)
        {
            if (!Curves.ContainsKey(dataFile.FigureId)) return true;
            var curve = Curves[dataFile.FigureId];
            if (!curve.ContainsKey(dataFile.CurveHandle)) return true;
            (int rowNO, int colNO) = curve[dataFile.CurveHandle];
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
            if (AxesSize.ContainsKey(dataFile.FigureId))
            switch (AxesSize[dataFile.FigureId])
            {
            case 0:
                Execute($@"
ax{dataFile.FigureId}.plot(x, y)
");
                break;
            case 1:
                int index = rowNO > colNO ? rowNO : colNO;
                Execute($@"
ax{dataFile.FigureId}[{index}].plot(x, y)
");
                break;
            case 2:
                Execute($@"
ax{dataFile.FigureId}[{rowNO}, {colNO}].plot(x, y)
ax.grid()
");
                break;
            }
            Showed[dataFile.FigureId] = true;
            foreach (var value in Showed.Values)
            {
                if (!value) return true;
            }
            Execute($@"
plt.show()
");
            Unsubscribe();
            return true;
        }
        protected override bool Receive(FigureProtocol.Curve curve)
        {
            var curveDictionary = new Dictionary<int, (int, int)>();
            Curves[curve.FigureId] = curveDictionary;
            curveDictionary[curve.CurveHandle] = (curve.RowNO, curve.ColumnNO);
            return true;
        }
    }
}