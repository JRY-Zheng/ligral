using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Linq;
using System;
using Ligral.Syntax.ASTs;
using Ligral.Syntax;
using Ligral.Component;
using Ligral.Simulation;
using Ligral.Simulation.Solvers;

namespace Ligral
{
    class Program
    {
        private static Logger logger = new Logger("Inspector");
        static void Main(string[] args)
        {
            Options options = new Options(args);
            switch (options.GetCommand())
            {
            case SimulationProject simulationProject:
                Run(simulationProject);
                break;
            case Document document:
                Run(document);
                break;
            case Help help:
                Console.WriteLine(@"Help on ligral:
Root:
    Posotion parameter: 
        FileName            required string
            if is file      interpret the file and run simulation.
            or is folder    the folder must be a package.
    Named parameters:
        --step-size & -s    double
            if given        override the step size of a fixed-step solver.
            else            use step size from `conf` or default 0.01.
        --stop-time & -t    double
            if given        override the stop time of all solvers.
            else            use stop time from `conf` or default 10.
        --output & -o       string
            if given        output CSV file in the given folder.
            else            output CSV file in the startup folder.
    Examples:
        ligral main.lig     run main.lig with default settings.
        ligral main.lig -o out
                            run main.lig and output CSV file into `out` folder.
        ligral main.lig -s 0.1 -t 100
                            run main.lig with step 0.1s and stop it at 100s.
Command: doc & document
    Position parameter:
        ModelName           optional string
            if exist        return the document of this specific model.
            else            return documents of all models.
    Named parameters:
        --json & -j         boolean
            if true         output the definition(s) in JSON format.
            else            print document(s) on the screen.
        --output & -o       string
            if given        output JSON file in the given folder.
            else            output JSON file in the startup folder.
    Examples:
        ligral doc          print all documents on the screen
        ligral doc --json -o def
                            output all JSON definitions to the `def` folder.
        ligral doc Sin      print the document of the model `Sin`.
Command: help & --help & -h:
    No parameters       print helps on the screen.
Parameter: --version & -v:
    No values           print the current version of ligral.");
                break;
            case Version version:
                Console.WriteLine("Ligral "+System.Reflection.Assembly.GetExecutingAssembly().GetName().Version);
                break;
            case Main main:
                Console.WriteLine(@"Copyright (c) Ligral Tech. All rights reserved. 
                    __    _                  __
                   / /   (_)___ __________ _/ /
                  / /   / / __ `/ ___/ __ `/ / 
                 / /___/ / /_/ / /  / /_/ / /  
                /_____/_/\__, /_/   \__,_/__/   
                        /____/                 
----------------------------------------------------------------
Hi, Ligral is a textual language for simulation.

Usage:
    ligral main.lig         to parse and simulate main.lig project.
    ligral doc Node         to view the document of Node model.
    ligral doc              to view documents of all the models.
    
Learn more:
    Visit https://junruoyu-zheng.gitee.io/ligral
    also available at https://JRY-Zheng.github.io/ligral");
                break;
            }
        }
        private static void Run(SimulationProject simulationProject)
        {
            Settings settings = Settings.GetInstance();
            settings.GetDefaultSettings();
            Interpreter interpreter = Interpreter.GetInstance(simulationProject.FileName);
            interpreter.Interpret();
            if (simulationProject.OutputFolder is string jsonOutputFolder)
            {
                settings.OutputFolder = jsonOutputFolder;
            }
            if (simulationProject.StepSize is double stepSize)
            {
                settings.StepSize = stepSize;
            }
            if (simulationProject.StopTime is double stopTime)
            {
                settings.StopTime = (double) stopTime;
            }
            Inspector inspector = new Inspector();
            List<Model> routine = inspector.Inspect(ModelManager.ModelPool);
            Problem problem = new Problem(routine);
            Solver solver = new FixedStepRK4Solver();
            solver.Solve(problem);
        }
        private static void Run(Document document)
        {
            Settings settings = Settings.GetInstance();
            settings.GetDefaultSettings();
            List<Model> models = new List<Model>();
            if (document.ModelName is string modelName)
            {
                if (ModelManager.ModelTypePool.Keys.Contains(modelName))
                {
                    models.Add(ModelManager.Create(modelName));
                }
                else
                {
                    throw logger.Error(new OptionException(modelName, $"No model named {modelName}"));
                }
            }
            else
            {
                foreach (string modelType in ModelManager.ModelTypePool.Keys)
                {
                    if (modelType.Contains('<')) continue;
                    models.Add(ModelManager.Create(modelType));
                }
            }
            if (document.ToJson is bool toJson && toJson)
            {
                if (document.OutputFolder is string outputFolder)
                {
                    settings.OutputFolder = outputFolder;
                }
                settings.NeedOutput = true;
                foreach (Model model in models)
                {
                    ModelDocument modelDocument = model.GetDocStruct();
                    string modelJson = JsonSerializer.Serialize<ModelDocument>(
                        modelDocument, new JsonSerializerOptions() {WriteIndented = true}
                    );
                    File.WriteAllText(Path.Join(settings.OutputFolder, $"{modelDocument.Type}.mdl.json"), modelJson);
                }
            }
            else
            {
                if (!(document.OutputFolder is null))
                {
                    throw logger.Error(new OptionException("Output folder is only needed when mdl.json is requested."));
                }
                foreach (Model model in models)
                {
                    Console.WriteLine(model.GetDoc());
                }
            }
        }
    }
}
