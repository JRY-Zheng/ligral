/* Copyright (C) 2019-2022 Junruoyu Zheng. Home page: https://junruoyu-zheng.gitee.io/ligral

   Distributed under MIT license.
   See file LICENSE for detail or copy at https://opensource.org/licenses/MIT
*/

using System.Collections.Generic;
using System.IO;
using System;
using Ligral.Syntax;
using Ligral.Component;
using Ligral.Simulation;
using Ligral.Extension;

namespace Ligral.Commands
{
    class SimulationCommand : Command
    {
        public string FileName;
        public string OutputFolder;
        public double? StepSize;
        public double? StopTime;
        public bool? RealTimeSimulation;
        public bool? ToCompile;
        public bool? IsJsonFile;
        public override string HelpInfo {get => "";}

        public override void Run()
        {
            try
            {
                Settings settings = Settings.GetInstance();
                settings.GetDefaultSettings();
                logger.Info($"Ligral (R) Simulation Engine version {Program.Version}.\nCopyright (C) Ligral Tech. All rights reserved.");
                PluginLoader pluginLoader = new PluginLoader();
                pluginLoader.Load();
                if (IsJsonFile is bool isJsonFile && isJsonFile)
                {
                    JsonLoader jsonLoader = new JsonLoader();
                    jsonLoader.Load(FileName);
                }
                else
                {
                    Interpreter interpreter = Interpreter.GetInstance(FileName);
                    interpreter.Interpret();
                }
                if (OutputFolder is string outputFolder)
                {
                    settings.OutputFolder = outputFolder;
                }
                if (StepSize is double stepSize)
                {
                    settings.StepSize = stepSize;
                }
                if (StopTime is double stopTime)
                {
                    settings.StopTime = (double) stopTime;
                }
                settings.ApplySetting();
                Inspector inspector = new Inspector();
                List<Model> routine = inspector.Inspect(ModelManager.ModelPool);
                if (ToCompile??false)
                {
                    Compiler compiler = new Compiler();
                    compiler.Compile(routine);
                }
                else
                {
                    string problemName = Path.GetFileNameWithoutExtension(FileName);
                    Problem problem = new Problem(problemName, routine);
                    Solver solver = Solver.GetSolver(settings.SolverName.ToLower());
                    try
                    {
                        solver.Solve(problem);
                    }
                    catch (LigralException e)
                    {
                        Solver.OnStopped();
                        throw logger.Error(new LigralException("Solving problem failed, solver exited."));
                    }
                }
            }
            catch (LigralException)
            {
                logger.Throw();
                logger.Warn($"Unexpected error in {FileName}, ligral exited with error.");
            }
        }
    }
}