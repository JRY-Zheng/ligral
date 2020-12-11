using System.Collections.Generic;
using System.IO;
using CommandLine;
using System.Text.Json;
using System.Linq;
using System;
using Ligral.Syntax.ASTs;
using Ligral.Syntax;
using Ligral.Component;
using Ligral.Simulation;
using Ligral.Simulation.Solvers;
using Ligral.Tools;
using Ligral.Tools.Protocols;

namespace Ligral
{
    class Program
    {
        static void Main(string[] args)
        {
            ParserResult<Options> result = CommandLine.Parser.Default.ParseArguments<Options>(args)
                .MapResult<Options, ParserResult<Options>>(
                    opts => DoParse(opts), //in case parser success
                    errs => DoError(errs));
            // helpInfo = HelpText.AutoBuild(result);
        }
        static ParserResult<Options> DoError(IEnumerable<Error> errs)
        {
            errs.ToList().ForEach(err=>System.Console.WriteLine(err.Tag));
            return null;
        }
        static ParserResult<Options> DoParse(Options options)
        {
            Settings settings = Settings.GetInstance();
            settings.GetDefaultSettings();
            if (options.RequireDoc!=null)
            {
                if (ModelManager.ModelTypePool.Keys.Contains(options.RequireDoc))
                {
                    Console.WriteLine(ModelManager.Create(options.RequireDoc).GetDoc());
                }
                else
                {
                    Console.WriteLine($"No model named {options.RequireDoc}");
                }
            }
            else if (options.RequireDocs)
            {
                foreach (string modelName in ModelManager.ModelTypePool.Keys)
                {
                    Console.WriteLine(ModelManager.Create(modelName).GetDoc());
                }
            }
            else if (options.RequireModelJSON)
            {
                if (options.OutputFolder!=null)
                {
                    settings.OutputFolder = options.OutputFolder;
                }
                else if (settings.OutputFolder==null)
                {
                    settings.OutputFolder = Path.GetFileNameWithoutExtension(options.InputFile);
                }
                settings.NeedOutput = true;
                foreach (string modelName in ModelManager.ModelTypePool.Keys)
                {
                    if (modelName.Contains('<')) continue;
                    ModelDocument modelDocument = ModelManager.Create(modelName).GetDocStruct();
                    string modelJson = JsonSerializer.Serialize<ModelDocument>(
                        modelDocument, new JsonSerializerOptions() {WriteIndented = true}
                    );
                    File.WriteAllText(Path.Join(settings.OutputFolder, $"{modelDocument.Type}.mdl.json"), modelJson);
                }
            }
            else if (options.RequireExamples)
            {
                Console.WriteLine("Ligral is developed by J. Zheng in 2020.");
                Console.WriteLine("Use --help to see full set of parameters");
                Console.WriteLine("Examples:");
                Console.WriteLine("\t ligral test.lig\t\t\t# parse and simulate test.lig");
                Console.WriteLine("\t ligral test.lig -o output\t\t# redirect simulation results to folder output");
                Console.WriteLine("\t ligral test.lig -s 0.01 -t 100\t\t# set simulation configuration");
                Console.WriteLine("\t ligral -d Integrator\t\t\t# see the document of model Integrator");
                Console.WriteLine("\t ligral -D\t\t\t\t# see the full set of documents");
            }
            else if (options.InputFile!=null)
            {
                string text = File.ReadAllText(options.InputFile);
                Syntax.Parser parser = new Syntax.Parser();
                parser.Load(text);
                ProgramAST p = parser.Parse();
                Interpreter interpreter = Interpreter.GetInstance(Path.GetDirectoryName(options.InputFile));
                interpreter.Interpret(p);
                if (options.OutputFolder!=null)
                {
                    settings.OutputFolder = options.OutputFolder;
                }
                else if (settings.OutputFolder==null)
                {
                    settings.OutputFolder = Path.GetFileNameWithoutExtension(options.InputFile);
                }
                if (options.StepSize!=null)
                {
                    settings.StepSize = (double) options.StepSize;
                }
                if (options.StopTime!=null)
                {
                    settings.StopTime = (double) options.StopTime;
                }
                Inspector inspector = new Inspector();
                List<Model> routine = inspector.Inspect(ModelManager.ModelPool);
                Problem problem = new Problem(routine);
                Solver solver = new EulerSolver();
                solver.Solve(problem);
            }
            else
            {
                Publisher publisher = new Publisher();
                FigureProtocol.FigureConfig figureConfig = new FigureProtocol.FigureConfig()
                {
                    FigureId = 1,
                    Title = "my title",
                    RowsCount = 1,
                    ColumnsCount = 1
                };
                publisher.Send(FigureProtocol.FigureConfigLabel, figureConfig);
                FigureProtocol.PlotConfig plotConfig = new FigureProtocol.PlotConfig()
                {
                    FigureId = 1,
                    RowNO = 0,
                    ColumnNO = 0,
                    XLabel = "time/s",
                    YLabel = "angle/deg"
                };
                publisher.Send(FigureProtocol.PlotConfigLabel, plotConfig);
                FigureProtocol.Curve curve = new FigureProtocol.Curve()
                {
                    FigureId = 1,
                    CurveHandle = 0,
                    RowNO = 0,
                    ColumnNO = 0
                };
                publisher.Send(FigureProtocol.CurveLabel, curve);
                FigureProtocol.ShowCommand showCommand = new FigureProtocol.ShowCommand()
                {
                    FigureId = 1
                };
                publisher.Send(FigureProtocol.ShowCommandLabel, showCommand);
                FigureProtocol.Data data = new FigureProtocol.Data()
                {
                    FigureId = 1, 
                    CurveHandle = 0, 
                    XValue = 0, 
                    YValue = 2
                };
                publisher.Send(FigureProtocol.DataLabel, data);
                System.Threading.Thread.Sleep(1000);
                data.XValue = 1;
                data.YValue = 1;
                publisher.Send(FigureProtocol.DataLabel, data);
            }
            return null;
        }
    }
}
