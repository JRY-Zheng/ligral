/* Copyright (C) 2019-2022 Junruoyu Zheng. Home page: https://junruoyu-zheng.gitee.io/ligral

   Distributed under MIT license.
   See file LICENSE for detail or copy at https://opensource.org/licenses/MIT
*/

using Ligral.Commands;

namespace Ligral
{
    class Options
    {
        private string[] args;
        private int index = 0;
        private Logger logger = new Logger("Options");
        private string arg 
        { 
            get 
            {
                if (index < args.Length)
                {
                    return args[index];
                }
                else
                {
                    return null;
                }
            }
        }
        public Options(string[] args)
        {
            this.args = args;
        }
        private string Eat()
        {
            if (arg is null)
            {
                throw logger.Error(new LigralException("Options are incomplete"));
            }
            index++;
            return arg;
        }
        private void StepBack()
        {
            index--;
        }
        private HelpCommand GetHelp()
        {
            Eat();
            return new HelpCommand();
        }
        private VersionCommand GetVersion()
        {
            Eat();
            return new VersionCommand();
        }
        private string GetString()
        {
            string str = Eat();
            if (str is null)
            {
                throw logger.Error(new LigralException("String value expected, options are incomplete"));
            }
            else if (str.StartsWith('-'))
            {
                throw logger.Error(new LigralException($"String value expected, but got option {str}. Do not pass string that starts with `-`."));
            }
            return str;
        }
        private double GetDouble()
        {
            string str = Eat();
            if (str is null)
            {
                throw logger.Error(new LigralException("String value expected, options are incomplete"));
            }
            try
            {
                return double.Parse(str);
            }
            catch
            {
                throw logger.Error(new OptionException(arg, "Cannot be converted to double"));
            }
        }
        private bool GetBoolean()
        {
            switch (Eat())
            {
            case "false":
                return false;
            case "true":
            case null:
                StepBack();
                return true;
            default:
                StepBack();
                return true;
            }
        }
        private bool IsHelpRequested(Command command)
        {
            if (Eat() is string option)
            {
                switch (option)
                {
                case "-h":
                case "--help":
                    command.RequestHelp = true;
                    Eat();
                    return true;
                }
            }
            StepBack();
            return false;
        }
        private DocumentCommand GetDocument()
        {
            DocumentCommand document = new DocumentCommand();
            if (IsHelpRequested(document)) return document;
            bool metUnknownOption = false;
            while (!metUnknownOption && Eat() is string option)
            {
                switch (option)
                {
                case "-j":
                case "--json":
                    document.ToJson = GetBoolean();
                    break;
                case "-o":
                case "--output":
                    string folder = GetString();
                    if (folder is null)
                    { 
                        throw logger.Error(new OptionException(option, "output parameter need a string value."));
                    }
                    document.OutputFolder = folder;
                    break;
                default:
                    metUnknownOption = document.ModelName != null;
                    if (!metUnknownOption) document.ModelName = arg;
                    break;
                }
            }
            return document;
        }
        private ExampleCommand GetExample()
        {
            ExampleCommand example = new ExampleCommand();
            if (IsHelpRequested(example)) return example;
            bool metUnknownOption = false;
            while (!metUnknownOption && Eat() is string option)
            {
                switch (option)
                {
                case "-a":
                case "--all":
                    if (example.ExampleName != null)
                    {
                        throw logger.Error(new OptionException(option, "Cannot specify example project name when requesting all examples"));
                    }
                    example.DownloadAll = GetBoolean();
                    break;
                default:
                    metUnknownOption = example.ExampleName != null;
                    if (!metUnknownOption) example.ExampleName = arg;
                    break;
                }
            }
            return example;
        }
        private LinearizationCommand GetLinearization()
        {
            LinearizationCommand linearization = new LinearizationCommand();
            if (IsHelpRequested(linearization)) return linearization;
            bool metUnknownOption = false;
            linearization.FileName = GetString();
            while (!metUnknownOption && Eat() is string option)
            {
                switch (option)
                {
                case "-j":
                case "--json":
                    linearization.IsJsonFile = GetBoolean();
                    break;
                case "-o":
                case "--output":
                    string file = GetString();
                    if (file is null)
                    { 
                        throw logger.Error(new OptionException(option, "output parameter need a string value."));
                    }
                    linearization.OutputFile = file;
                    break;
                default:
                    metUnknownOption = true;
                    break;
                }
            }
            return linearization;
        }
        private TrimmingCommand GetTrimming()
        {
            TrimmingCommand trimming = new TrimmingCommand();
            if (IsHelpRequested(trimming)) return trimming;
            bool metUnknownOption = false;
            trimming.FileName = GetString();
            while (!metUnknownOption && Eat() is string option)
            {
                switch (option)
                {
                case "-j":
                case "--json":
                    trimming.IsJsonFile = GetBoolean();
                    break;
                case "-o":
                case "--output":
                    string file = GetString();
                    if (file is null)
                    { 
                        throw logger.Error(new OptionException(option, "output parameter need a string value."));
                    }
                    trimming.OutputFile = file;
                    break;
                default:
                    metUnknownOption = true;
                    break;
                }
            }
            return trimming;
        }
        private SimulationCommand GetSimulationProject()
        {
            SimulationCommand simulationProject = new SimulationCommand() {FileName = arg};
            bool metUnknownOption = false;
            while (!metUnknownOption && Eat() is string option)
            {
                switch (option)
                {
                case "-o":
                case "--output":
                    string folder = GetString();
                    if (folder is null)
                    { 
                        throw logger.Error(new OptionException(option, "output parameter need a string value."));
                    }
                    simulationProject.OutputFolder = folder;
                    break;
                case "-s":
                case "--step-size":
                    simulationProject.StepSize = GetDouble();
                    break;
                case "-t":
                case "--stop-time":
                    simulationProject.StopTime = GetDouble();
                    break;
                case "-j":
                case "--json":
                    simulationProject.IsJsonFile = GetBoolean();
                    break;
                case "-r":
                case "--real-time":
                    simulationProject.RealTimeSimulation = GetBoolean();
                    break;
                case "-c":
                case "--compile":
                    simulationProject.ToCompile = GetBoolean();
                    break;
                default:
                    metUnknownOption = true;
                    break;
                }
            }
            return simulationProject;
        }
        public Command GetCommand()
        {
            try
            {
                switch (arg)
                {
                case "doc":
                case "document":
                    return GetDocument();
                case "exm":
                case "example":
                case "examples":
                    return GetExample();
                case "lin":
                case "linearize":
                case "linearise":
                    return GetLinearization();
                case "trim":
                    return GetTrimming();
                case "help":
                case "-h":
                case "--help":
                    return GetHelp();
                case "-v":
                case "--version":
                    return GetVersion();
                case null:
                    return new MainCommand();
                default:
                    return GetSimulationProject();
                }
            }
            finally
            {
                if (index < args.Length)
                {
                    throw logger.Error(new OptionException(arg, $"Unknown option {arg}."));
                }
            }
        }
    }
}