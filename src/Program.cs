﻿/* Copyright (C) 2019-2021 Junruoyu Zheng. Home page: https://junruoyu-zheng.gitee.io/ligral

   Distributed under MIT license.
   See file LICENSE for detail or copy at https://opensource.org/licenses/MIT
*/

using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Linq;
using System;
using Ligral.Syntax.ASTs;
using Ligral.Syntax;
using Ligral.Component;
using Ligral.Simulation;
using Ligral.Extension;
using Ligral.Network;
using Ligral.Commands;

namespace Ligral
{
    public class Program
    {
        private static Logger logger = new Logger("Main");
        private static string version = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();
        static void Main(string[] args)
        {
            try
            {
                Options options = new Options(args);
                Command command = options.GetCommand();
                if (command.RequestHelp is bool requestHelp && requestHelp)
                {
                    Console.WriteLine(command.HelpInfo);
                    return;
                }
                switch (command)
                {
                case SimulationCommand simulationProject:
                    Run(simulationProject);
                    break;
                case LinearizationCommand linearization:
                    Run(linearization);
                    break;
                case TrimmingCommand trimming:
                    Run(trimming);
                    break;
                case DocumentCommand document:
                    Run(document);
                    break;
                case ExampleCommand example:
                    Run(example);
                    break;
                case HelpCommand help:
                    Console.WriteLine($@"Help on ligral:\n{help.HelpInfo}");
                    break;
                case VersionCommand version:
                    Console.WriteLine("Ligral "+Program.version);
                    break;
                case MainCommand main:
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
    ligral exm --all        to download example projects.
    ligral main.lig         to parse and simulate main.lig project.
    ligral doc              to view documents of all the models.
    
Learn more:
    Visit https://junruoyu-zheng.gitee.io/ligral
    Clone the source at https://gitee.com/junruoyu-zheng/ligral");
                    break;
                }
            }
            catch (LigralException)
            {
                logger.Throw();
                logger.Warn($"Unexpected error occurred, ligral exited with error.");
            }
            catch (Exception e)
            {
                if (Logger.LogFile is null)
                {
                    Logger.LogFile = "ligral.log";
                }
                logger.Fatal(e);
            }
        }
        private static void Run(SimulationCommand simulationProject)
        {
            try
            {
                Settings settings = Settings.GetInstance();
                settings.GetDefaultSettings();
                logger.Info($"Ligral (R) Simulation Engine version {version}.\nCopyright (C) Ligral Tech. All rights reserved.");
                PluginLoader pluginLoader = new PluginLoader();
                pluginLoader.Load();
                if (simulationProject.IsJsonFile is bool isJsonFile && isJsonFile)
                {
                    JsonLoader jsonLoader = new JsonLoader();
                    jsonLoader.Load(simulationProject.FileName);
                }
                else
                {
                    Interpreter interpreter = Interpreter.GetInstance(simulationProject.FileName);
                    interpreter.Interpret();
                }
                if (simulationProject.OutputFolder is string outputFolder)
                {
                    settings.OutputFolder = outputFolder;
                }
                if (simulationProject.StepSize is double stepSize)
                {
                    settings.StepSize = stepSize;
                }
                if (simulationProject.StopTime is double stopTime)
                {
                    settings.StopTime = (double) stopTime;
                }
                settings.ApplySetting();
                Inspector inspector = new Inspector();
                List<Model> routine = inspector.Inspect(ModelManager.ModelPool);
                string problemName = Path.GetFileNameWithoutExtension(simulationProject.FileName);
                Problem problem = new Problem(problemName, routine);
                Solver solver = Solver.GetSolver(settings.SolverName.ToLower());
                solver.Solve(problem);
            }
            catch (LigralException)
            {
                logger.Throw();
                logger.Warn($"Unexpected error in {simulationProject.FileName}, ligral exited with error.");
            }
        }
        private static void Run(LinearizationCommand linearization)
        {
            try
            {
                ControlInput.IsOpenLoop = true;
                Settings settings = Settings.GetInstance();
                settings.GetDefaultSettings();
                logger.Info($"Ligral (R) Simulation Engine version {version}.\nCopyright (C) Ligral Tech. All rights reserved.");
                PluginLoader pluginLoader = new PluginLoader();
                pluginLoader.Load();
                if (linearization.IsJsonFile is bool isJsonFile && isJsonFile)
                {
                    JsonLoader jsonLoader = new JsonLoader();
                    jsonLoader.Load(linearization.FileName);
                }
                else
                {
                    Interpreter interpreter = Interpreter.GetInstance(linearization.FileName);
                    interpreter.Interpret();
                }
                Inspector inspector = new Inspector();
                List<Model> routine = inspector.Inspect(ModelManager.ModelPool);
                string problemName = Path.GetFileNameWithoutExtension(linearization.FileName);
                Problem problem = new Problem(problemName, routine);
                Linearizer linearizer = new Linearizer();
                if (settings.LinearizerConfiguration != null)
                {
                    linearizer.Configure(settings.LinearizerConfiguration);
                }
                else
                {
                    logger.Warn("No linearization configuration is set. The model will be linearized at zero.");
                }
                settings.ApplySetting();
                linearizer.Linearize(problem);
                if (linearization.OutputFile is string outputFile)
                {
                    try
                    {
                        File.WriteAllText(outputFile, linearizer.ToString());
                    }
                    catch (Exception e)
                    {
                        logger.Prompt(linearizer.ToString());
                        throw logger.Error(new LigralException($"Cannot write to {outputFile}, got error: {e.Message}"));
                    }
                }
                else
                {
                    logger.Prompt(linearizer.ToString());
                }
            }
            catch (LigralException)
            {
                logger.Throw();
                logger.Warn($"Unexpected error in {linearization.FileName}, ligral exited with error.");
            }
        }
        private static void Run(TrimmingCommand trimming)
        {
            try
            {
                ControlInput.IsOpenLoop = true;
                Settings settings = Settings.GetInstance();
                settings.GetDefaultSettings();
                logger.Info($"Ligral (R) Simulation Engine version {version}.\nCopyright (C) Ligral Tech. All rights reserved.");
                PluginLoader pluginLoader = new PluginLoader();
                pluginLoader.Load();
                if (trimming.IsJsonFile is bool isJsonFile && isJsonFile)
                {
                    JsonLoader jsonLoader = new JsonLoader();
                    jsonLoader.Load(trimming.FileName);
                }
                else
                {
                    Interpreter interpreter = Interpreter.GetInstance(trimming.FileName);
                    interpreter.Interpret();
                }
                Inspector inspector = new Inspector();
                List<Model> routine = inspector.Inspect(ModelManager.ModelPool);
                string problemName = Path.GetFileNameWithoutExtension(trimming.FileName);
                Problem problem = new Problem(problemName, routine);
                Trimmer trimmer = new Trimmer();
                if (settings.TrimmerConfiguration != null)
                {
                    trimmer.Configure(settings.TrimmerConfiguration);
                }
                else
                {
                    logger.Warn("No trimming configuration is set. The model will be trimmed at zero.");
                }
                settings.ApplySetting();
                trimmer.GetCondition();
                trimmer.Trim(problem);
                if (trimming.OutputFile is string outputFile)
                {
                    try
                    {
                        File.WriteAllText(outputFile, trimmer.ToString());
                    }
                    catch (Exception e)
                    {
                        logger.Prompt(trimmer.ToString());
                        throw logger.Error(new LigralException($"Cannot write to {outputFile}, got error: {e.Message}"));
                    }
                }
                else
                {
                    logger.Prompt(trimmer.ToString());
                }
            }
            catch (LigralException)
            {
                logger.Throw();
                logger.Warn($"Unexpected error in {trimming.FileName}, ligral exited with error.");
            }
        }
        private static void Run(ExampleCommand example)
        {
            ExampleManager manager = new ExampleManager();
            if (example.DownloadAll is bool downloadAll && downloadAll)
            {
                manager.DownloadAllProjects();
            }
            else if (example.ExampleName is string exampleName)
            {
                manager.DownloadProject(exampleName);
            }
            else
            {
                manager.ShowExampleList();
            }
        }
        private static void Run(DocumentCommand document)
        {
            Settings settings = Settings.GetInstance();
            try
            {
                settings.GetDefaultSettings();
            }
            catch (LigralException)
            {
                logger.Error(new LigralException("Default settings is not valid, ligral exited with errors."));
                logger.Throw();
                return;
            }
            PluginLoader pluginLoader = new PluginLoader();
            pluginLoader.Load();
            List<Model> models = new List<Model>();
            if (document.ModelName is string modelName)
            {
                if (ModelManager.ModelTypePool.Keys.Contains(modelName))
                {
                    models.Add(ModelManager.Create(modelName));
                }
                else
                {
                    logger.Error(new OptionException(modelName, $"No model named {modelName}"));
                    logger.Throw();
                    return;
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
                if (!Directory.Exists(settings.OutputFolder))
                {
                    Directory.CreateDirectory(settings.OutputFolder);
                }
                foreach (Model model in models)
                {
                    ModelDocument modelDocument = model.GetDocStruct();
                    string modelJson = JsonSerializer.Serialize<ModelDocument>(
                        modelDocument, new JsonSerializerOptions() {WriteIndented = true}
                    );
                    string modelJsonFileName = Path.Join(settings.OutputFolder, $"{modelDocument.Type}.mdl.json");
                    try
                    {
                        File.WriteAllText(modelJsonFileName, modelJson);
                    }
                    catch (Exception e)
                    {
                        logger.Error(new LigralException($"Cannot write to {modelJsonFileName}: {e.Message}"));
                        logger.Throw();
                        return;
                    }
                }
            }
            else
            {
                if (!(document.OutputFolder is null))
                {
                    logger.Error(new OptionException("Output folder is only needed when mdl.json is requested."));
                    logger.Throw();
                    return;
                }
                foreach (Model model in models)
                {
                    Console.WriteLine(model.GetDoc());
                }
            }
        }
    }
}
