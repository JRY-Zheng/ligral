/* Copyright (C) 2019-2022 Junruoyu Zheng. Home page: https://junruoyu-zheng.gitee.io/ligral

   Distributed under MIT license.
   See file LICENSE for detail or copy at https://opensource.org/licenses/MIT
*/

using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using Ligral.Tools.Protocols;

namespace Ligral.Tools
{
    public class RTPlotter : Plotter
    {
        protected double Interval = 2;
        public RTPlotter() : base()
        {
            
        }
        protected override void Execute(string command)
        {
            PythonProcess.Execute(command);
            // Console.WriteLine(command);
            logger.Debug($"Executing command: {command}");
        }
        protected override bool Receive(FigureProtocol.Curve curve)
        {
            if (!base.Receive(curve)) return false;
            Figure figure = Figures[curve.FigureId];
            string curveID = $"{curve.FigureId}_{curve.CurveHandle}";
            switch (figure.AxesShape)
            {
            case AxesShapeType.Scalar:
                Execute($@"
cv{curveID}, = ax{curve.FigureId}.plot([], [])
xdata{curveID} = cv{curveID}.get_xdata()
ydata{curveID} = cv{curveID}.get_ydata()
ax{curve.FigureId}.grid()
");
                break;
            case AxesShapeType.Vector:
                int index = curve.RowNO > curve.ColumnNO ? curve.RowNO : curve.ColumnNO;
                Execute($@"
cv{curveID}, = ax{curve.FigureId}[{index}].plot([], [])
xdata{curveID} = cv{curveID}.get_xdata()
ydata{curveID} = cv{curveID}.get_ydata()
ax{curve.FigureId}[{index}].grid()
");
                break;
            case AxesShapeType.Matrix:
                Execute($@"
cv{curveID}, = ax{curve.FigureId}[{curve.RowNO}, {curve.ColumnNO}].plot([], [])
xdata{curveID} = cv{curveID}.get_xdata()
ydata{curveID} = cv{curveID}.get_ydata()
ax{curve.FigureId}[{curve.RowNO}, {curve.ColumnNO}].grid()
");
                break;
            }
            return true;
        }

        protected override bool Receive(FigureProtocol.ShowCommand showCommand)
        {
            if (!Figures.ContainsKey(showCommand.FigureId)) return false;
            Figure fig = Figures[showCommand.FigureId];
            fig.Showed = true;
            foreach (Figure figure in Figures.Values)
            {
                if (!figure.Showed) return true;
            }
            Execute(@"
plt.ion()
");
            foreach (Figure figure in Figures.Values)
            {
                figure.Showed = false;
            }
            foreach (Curve curve in fig.Curves.Values)
            {
                curve.CachedTime = DateTime.Now;
            }
            return true;
        }
        protected override bool Receive(FigureProtocol.Data data)
        {
            if (!Figures.ContainsKey(data.FigureId)) return false;
            Figure figure = Figures[data.FigureId];
            if (!figure.Curves.ContainsKey(data.CurveHandle)) return false;
            Curve curve = figure.Curves[data.CurveHandle];
            curve.CachedX.Add(data.XValue);
            curve.CachedY.Add(data.YValue);
            if ((DateTime.Now - curve.CachedTime).TotalSeconds >= Interval)
            {
                Flush(curve, data.FigureId, data.CurveHandle);
            }
            return true;
        }
        protected virtual void Flush(Curve curve, int figureId, int curveHandle)
        {
            Figure figure = Figures[figureId];
            string curveID = $"{figureId}_{curveHandle}";
            switch (figure.AxesShape)
            {
            case AxesShapeType.Scalar:
                Execute($@"
xdata{curveID} = np.append(xdata{curveID}, [{string.Join(',', curve.CachedX)}])
ydata{curveID} = np.append(ydata{curveID}, [{string.Join(',', curve.CachedY)}])
cv{curveID}.set_data(xdata{curveID}, ydata{curveID})
ax{figureId}.set_xlim((min(xdata{curveID}), max(xdata{curveID})))
ax{figureId}.set_ylim((min(ydata{curveID}), max(ydata{curveID})))
");
                break;
                case AxesShapeType.Vector:
                int index = curve.RowNO > curve.ColNO ? curve.RowNO : curve.ColNO;
                Execute($@"
xdata{curveID} = np.append(xdata{curveID}, [{string.Join(',', curve.CachedX)}])
ydata{curveID} = np.append(ydata{curveID}, [{string.Join(',', curve.CachedY)}])
cv{curveID}.set_data(xdata{curveID}, ydata{curveID})
ax{figureId}[{index}].set_xlim((min(xdata{curveID}), max(xdata{curveID})))
ax{figureId}[{index}].set_ylim((min(ydata{curveID}), max(ydata{curveID})))
");
                break;
            case AxesShapeType.Matrix:
                Execute($@"
xdata{curveID} = np.append(xdata{curveID}, [{string.Join(',', curve.CachedX)}])
ydata{curveID} = np.append(ydata{curveID}, [{string.Join(',', curve.CachedY)}])
cv{curveID}.set_data(xdata{curveID}, ydata{curveID})
ax{figureId}[{curve.RowNO},{curve.ColNO}].set_xlim((min(xdata{curveID}), max(xdata{curveID})))
ax{figureId}[{curve.RowNO},{curve.ColNO}].set_ylim((min(ydata{curveID}), max(ydata{curveID})))
");
                break;
            }
            curve.CachedX.Clear();
            curve.CachedY.Clear();
            curve.CachedTime = DateTime.Now;
            curve.Paused = true;
            if (Figures.All(fig => fig.Value.Paused))
            {
                Execute(@"
plt.pause(0.05)
");
                foreach (Figure fig in Figures.Values)
                {
                    fig.Paused = false;
                }
            }
        }
        protected override bool Receive(FigureProtocol.DataFile dataFile)
        {
            if (!Figures.ContainsKey(dataFile.FigureId)) return false;
            Figure figure = Figures[dataFile.FigureId];
            if (!figure.Curves.ContainsKey(dataFile.CurveHandle)) return false;
            Curve curve = figure.Curves[dataFile.CurveHandle];
            Flush(curve, dataFile.FigureId, dataFile.CurveHandle);
            curve.Showed = true;
            if (SaveFigure && figure.Showed) 
            {
                string fileName = Path.GetFullPath(dataFile.FileName);
                string figName = figure.Name;
                foreach (char invalidChar in Path.GetInvalidFileNameChars())
                {
                    figName = figName.Replace(invalidChar, '_');
                }
                Execute($@"
fig{dataFile.FigureId}.savefig(r'{Path.GetDirectoryName(fileName)}\{figName}.png')
");
            }
            foreach (Figure fig in Figures.Values)
            {
                if (!figure.Showed) return true;
            }
            Execute(@"
plt.ioff()
plt.show()
");
            Unsubscribe();
            return true;
        }
        public override void Configure(Dictionary<string, object> dict)
        {
            foreach (string item in dict.Keys)
            {
                object val = dict[item];
                try
                {
                    switch (item)
                    {
                    case "output_script":
                    case "output_scripts":
                        throw logger.Error(new SettingException(item, val, "Realtime plotter cannot output scripts"));
                    case "frequency":
                    case "freq":
                        dict.Remove(item);
                        Interval = 1/((double) val); 
                        break;
                    case "interval":
                        dict.Remove(item);
                        Interval = (double) val; 
                        break;
                    }
                }
                catch (System.InvalidCastException)
                {
                    throw logger.Error(new SettingException(item, val, $"Invalid type {val.GetType()} in plotter"));
                }
            }
            base.Configure(dict);
        }
    }
}