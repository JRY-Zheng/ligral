using System.Collections.Generic;
using ParameterDictionary = System.Collections.Generic.Dictionary<string, Ligral.Component.Parameter>;
using DoubleCsvTable;
using Ligral.Component;
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
        private string fileName;
        private CsvTable table;
        private int rowNo = -1;
        private int colNo = -1;
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
                    fileName = (string)value;
                }, ()=>
                {
                    fileName = Name;
                })}
            };
        }
        protected override List<Signal> DefaultCalculate(List<Signal> values)
        {
            Signal inputSignal = values[0];
            if (rowNo < 0 || colNo < 0)
            {
                (rowNo, colNo) = inputSignal.Shape();
                List<string> columns = new List<string>() {"Time"};
                if (inputSignal.IsMatrix)
                {
                    for(int i = 0; i < rowNo; i++)
                    {
                        for (int j = 0; j < colNo; j++)
                        {
                            columns.Add($"Data({i}-{j})");
                        }
                    }
                }
                else
                {
                    columns.Add("Data");
                }
                table = new CsvTable(columns, new List<List<double>>());
            }
            Calculate = PostCalculate;
            return Calculate(values);
        }
        private List<Signal> PostCalculate(List<Signal> values)
        {
            Signal inputSignal = values[0];
            List<double> row = inputSignal.ToList();
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
            scripts.Add($"data = frame.values");
            scripts.Add($"plt.figure(num='{fileName}')");
            scripts.Add("time = data.T[0]");
            if (rowNo ==0 && colNo == 0)
            {
                scripts.Add("plt.plot(time, data.T[1])");
                scripts.Add("plt.xlabel('time (s)')");
                scripts.Add("plt.ylabel('Data')");
                scripts.Add("plt.grid()");
            }
            else
            {
                scripts.Add("for i, col in enumerate(data.T[1:]):");
                scripts.Add($"    plt.subplot({rowNo}, {colNo}, i+1)");
                scripts.Add("    plt.plot(time, col)");
                scripts.Add("    plt.xlabel('time (s)')");
                scripts.Add("    plt.ylabel(frame.columns[i+1])");
                scripts.Add("    plt.grid()");
            }
            scripts.Add($"plt.suptitle('{fileName}')");
            scripts.Add("plt.tight_layout()");
            scripts.Add("plt.show()");
            string scriptsFile = System.IO.Path.Join(currentDirectory, settings.OutputFolder, fileName + ".py");
            System.IO.File.WriteAllLines(scriptsFile, scripts);
            System.Diagnostics.Process.Start("python", $"\"{scriptsFile}\"");
        }
    }
}