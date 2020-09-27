using System.Collections.Generic;
using System.IO;
using CommandLine;
using System.Text;
using System.Linq;
using System;

namespace Ligral
{
    class Program
    {
        static void Main(string[] args)
        {
            ParserResult<Options> result = CommandLine.Parser.Default.ParseArguments<Options>(args).MapResult<Options, ParserResult<Options>>((opts) => DoParse(opts), //in case parser sucess
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
                try
                {
                    string text = File.ReadAllText(options.InputFile);
                    Parser parser = new Parser();
                    parser.Load(text);
                    ProgramAST p = parser.Parse();
                    Interpreter interpreter = new Interpreter(Path.GetDirectoryName(options.InputFile));
                    interpreter.Interpret(p);
                    Settings settings = Settings.GetInstance();
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
                    Wanderer wanderer = new Wanderer();
                    wanderer.Wander(routine);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
            }
            else
            {
                Console.WriteLine("Ligral is a Literal and Graphical Simulation Language.");
                Console.WriteLine("\tUse --help option to get help infomation.");
                Console.WriteLine("*******************************************************");
                Console.WriteLine("Type your ligral script below, and use 'run' command to execute.");
                string lineInput = "";
                Parser parser = new Parser();
                Interpreter interpreter = new Interpreter(".");
                while (lineInput.Trim()!="run")
                {
                    parser.Load(lineInput);
                    try
                    {
                        ProgramAST p = parser.Parse();
                        interpreter.AppendInterpret(p);
                    }
                    catch(Exception e)
                    {
                        Console.WriteLine(e.ToString());
                    }
                    Console.Write(">>>");
                    lineInput = Console.ReadLine();
                }
                Inspector inspector = new Inspector();
                List<Model> routine = inspector.Inspect(ModelManager.ModelPool);
                Wanderer wanderer = new Wanderer();
                wanderer.Wander(routine);
            }
            return null;
        }
    }
}
