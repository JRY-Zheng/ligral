using System.Collections.Generic;
using ParameterDictionary = System.Collections.Generic.Dictionary<string, Ligral.Component.Parameter>;
using DoubleCsvTable;
using Ligral.Component;
using Ligral.Simulation;

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
        private string fileName;
        private CsvTable table;
        private int rowNo = -1;
        private int colNo = -1;
        private enum Mode {Ymn, Xmn, X1mYn1, Xm1Y1n, X1Y1};
        private Mode mode;
        protected override void SetUpPorts()
        {
            InPortList.Add(new InPort("x", this));
            InPortList.Add(new InPort("y", this));
        }
        protected override void SetUpParameters()
        {
            Parameters = new ParameterDictionary()
            {
                {"name", new Parameter(value=>
                {
                    fileName = (string)value;
                }, ()=>
                {
                    fileName = Name;
                })}
            };
        }
        protected override List<Signal> DefaultCalculate(List<Signal> values)
        {
            Signal xSignal = values[0];
            Signal ySignal = values[1];
            (int xr, int xc) = xSignal.Shape();
            (int yr, int yc) = xSignal.Shape();
            List<string> columns = new List<string>() {"Time"};
            if (!xSignal.IsMatrix && !ySignal.IsMatrix)
            {
                colNo = 1;
                rowNo = 1;
                mode = Mode.X1Y1;
                columns.Add("Data:x");
                columns.Add("Data:y");
            }
            else if (!xSignal.IsMatrix)
            {
                colNo = yc;
                rowNo = yr;
                mode = Mode.Ymn;
                columns.Add("Data:x");
                for(int i = 0; i < rowNo; i++)
                {
                    for (int j = 0; j < colNo; j++)
                    {
                        columns.Add($"Data:y({i}-{j})");
                    }
                }
            }
            else if (!ySignal.IsMatrix)
            {
                colNo = xc;
                rowNo = xr;
                mode = Mode.Xmn;
                for(int i = 0; i < rowNo; i++)
                {
                    for (int j = 0; j < colNo; j++)
                    {
                        columns.Add($"Data:x({i}-{j})");
                    }
                }
                columns.Add("Data:y");
            }
            else if (xr == 1 && yc == 1)
            {
                colNo = xc;
                rowNo = yr;
                mode = Mode.X1mYn1;
                for (int j = 0; j < colNo; j++)
                {
                    columns.Add($"Data:x({j})");
                }
                for(int i = 0; i < rowNo; i++)
                {
                    columns.Add($"Data:y({i})");
                }
            }
            else if (xc == 1 && yr == 1)
            {
                colNo = yc;
                rowNo = xr;
                mode = Mode.Xm1Y1n;
                for(int i = 0; i < rowNo; i++)
                {
                    columns.Add($"Data:x({i})");
                }
                for (int j = 0; j < colNo; j++)
                {
                    columns.Add($"Data:y({j})");
                }
            }
            else
            {
                throw new ModelException(this, "PhaseDiagram only accepts [scalar, (m*n)] or [(1*m), (n*1)] or vice versa.");
            }
            table = new CsvTable(columns, new List<List<double>>());
            Calculate = PostCalculate;
            return Calculate(values);
        }
        private List<Signal> PostCalculate(List<Signal> values)
        {
            Signal xSignal = values[0];
            Signal ySignal = values[1];
            List<double> row = xSignal.ToList();
            row.AddRange(ySignal.ToList());
            row.Insert(0, Solver.Time);
            table.AddRow(row);
            return Results;
        }
        public override void Release()
        {
            Settings settings = Settings.GetInstance();
            settings.NeedOutput = true;
            string currentDirectory = System.IO.Directory.GetCurrentDirectory();
            string dataFile = System.IO.Path.Join(currentDirectory, settings.OutputFolder, fileName + ".csv");
            table.DumpFile(dataFile, true);
            List<string> scripts = new List<string>();
            scripts.Add("# Auto-generated by Ligral(c)");
            scripts.Add("import numpy as np");
            scripts.Add("import pandas as pd");
            scripts.Add("import matplotlib.pyplot as plt");
            scripts.Add($"frame = pd.read_csv(r'{dataFile}')");
            scripts.Add("data = frame.values");
            scripts.Add($"plt.figure(num='{fileName}')");
            scripts.Add("time = data.T[0]");
            switch (mode)
            {
                case Mode.X1Y1:
                    scripts.Add("x = data.T[1]");
                    scripts.Add("y = data.T[2]");
                    scripts.Add("plt.plot(x, y)");
                    scripts.Add("plt.xlabel('x')");
                    scripts.Add("plt.ylabel('y')");
                    scripts.Add("plt.grid()");
                    break;
                case Mode.Ymn:
                    scripts.Add("x = data.T[1]");
                    scripts.Add("y = data.T[2:]");
                    scripts.Add("for i, col in enumerate(y):");
                    scripts.Add($"    plt.subplot({rowNo}, {colNo}, i+1)");
                    scripts.Add("    plt.plot(x, col)");
                    scripts.Add("    plt.xlabel('x')");
                    scripts.Add("    plt.ylabel(f'y{i}')");
                    scripts.Add("    plt.grid()");
                    break;
                case Mode.Xmn:
                    scripts.Add("x = data.T[1:-1]");
                    scripts.Add("y = data.T[-1]");
                    scripts.Add("for i, col in enumerate(x):");
                    scripts.Add($"    plt.subplot({rowNo}, {colNo}, i+1)");
                    scripts.Add("    plt.plot(col, y)");
                    scripts.Add("    plt.xlabel(f'x{i}')");
                    scripts.Add("    plt.ylabel('y')");
                    scripts.Add("    plt.grid()");
                    break;
                case Mode.X1mYn1:
                    scripts.Add($"x = data.T[1:{colNo+1}]");
                    scripts.Add($"y = data.T[{colNo+1}:]");
                    scripts.Add("for i, col in enumerate(y):");
                    scripts.Add("    for j, row in enumerate(x):");
                    scripts.Add($"        plt.subplot({rowNo}, {colNo}, i*{rowNo}+j+1)");
                    scripts.Add("        plt.plot(row, col)");
                    scripts.Add("        plt.xlabel(f'x{j}')");
                    scripts.Add("        plt.ylabel(f'y{i}')");
                    scripts.Add("        plt.grid()");
                    break;
                case Mode.Xm1Y1n:
                    scripts.Add($"x = data.T[1:{rowNo+1}]");
                    scripts.Add($"y = data.T[{rowNo+1}:]");
                    scripts.Add("for i, col in enumerate(x):");
                    scripts.Add("    for j, row in enumerate(y):");
                    scripts.Add($"        plt.subplot({rowNo}, {colNo}, i*{rowNo}+j+1)");
                    scripts.Add("        plt.plot(col, row)");
                    scripts.Add("        plt.xlabel(f'x{i}')");
                    scripts.Add("        plt.ylabel(f'y{j}')");
                    scripts.Add("        plt.grid()");
                    break;
            }
            scripts.Add($"plt.suptitle('{fileName}')");
            scripts.Add("plt.show()");
            string scriptsFile = System.IO.Path.Join(currentDirectory, settings.OutputFolder, fileName + ".py");
            System.IO.File.WriteAllLines(scriptsFile, scripts);
            System.Diagnostics.Process.Start("python", $"\"{scriptsFile}\"");
        }
    }
}