/* Copyright (C) 2019-2021 Junruoyu Zheng. Home page: https://junruoyu-zheng.gitee.io/ligral

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
    class TrimmingCommand : Command
    {
        public string FileName;
        public string OutputFile;
        public bool? IsJsonFile;
        public override string HelpInfo {get => @"Command: trim
    Position parameter: 
        FileName            required string
            if is file      interpret the file and trim the model.
            or is folder    the folder must be a package.
    Named parameters:
        --json & -j         boolean
            if true         the project is in .lig.json format.
            else            the project is in .lig format.
        --output & -o       string
            if given        output trim point in .lig file with the given name.
            else            print trim point in the screen.
    Examples:
        ligral trim model.lig
                            trim model.lig at the given condition.
        ligral trim model.lig -o tp.lig
                            trim model.lig and output trim point in file tp.lig.
        ligral trim model.lig.json --json
                            trim model.lig.json, the output format is .lig";}

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
                if (OutputFile is string outputFile)
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
                logger.Warn($"Unexpected error in {FileName}, ligral exited with error.");
            }
        }
    }
}