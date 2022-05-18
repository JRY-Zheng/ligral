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
    class LinearizationCommand : Command
    {
        public string FileName;
        public string OutputFile;
        public bool? IsJsonFile;
        public override string HelpInfo {get => @"Command: lin & linearize & linearise
    Position parameter: 
        FileName            required string
            if is file      interpret the file and linearize the model.
            or is folder    the folder must be a package.
    Named parameters:
        --json & -j         boolean
            if true         the project is in .lig.json format.
            else            the project is in .lig format.
        --output & -o       string
            if given        output state space matrices in .lig file with the given name.
            else            print state space matrices in the screen.
    Examples:
        ligral lin model.lig
                            linearize model.lig at the given point.
        ligral lin model.lig -o ss.lig
                            linearize model.lig and output state space matrices in file ss.lig.
        ligral lin model.lig.json --json
                            linearize model.lig.json, the output format is .lig";}

        public override void Run()
        {
            try
            {
                ControlInput.IsOpenLoop = true;
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
                Inspector inspector = new Inspector();
                List<Model> routine = inspector.Inspect(ModelManager.ModelPool);
                string problemName = Path.GetFileNameWithoutExtension(FileName);
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
                if (OutputFile is string outputFile)
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
                logger.Warn($"Unexpected error in {FileName}, ligral exited with error.");
            }
        }
    }
}