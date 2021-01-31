/* Copyright (C) 2019-2021 Junruoyu Zheng. Home page: https://junruoyu-zheng.gitee.io/ligral

   Distributed under MIT license.
   See file LICENSE for detail or copy at https://opensource.org/licenses/MIT
*/

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
            index++;
            return arg;
        }
        private void StepBack()
        {
            index--;
        }
        private Help GetHelp()
        {
            Eat();
            return new Help();
        }
        private Version GetVersion()
        {
            Eat();
            return new Version();
        }
        private string GetString()
        {
            return Eat();
        }
        private double GetDouble()
        {
            try
            {
                return double.Parse(Eat());
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
                return true;
            default:
                StepBack();
                return true;
            }
        }
        private Document GetDocument()
        {
            Document document = new Document();
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
        private Linearization GetLinearization()
        {
            Linearization linearization = new Linearization();
            bool metUnknownOption = false;
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
                    string folder = GetString();
                    if (folder is null)
                    { 
                        throw logger.Error(new OptionException(option, "output parameter need a string value."));
                    }
                    linearization.OutputFolder = folder;
                    break;
                default:
                    metUnknownOption = linearization.FileName != null;
                    if (!metUnknownOption) linearization.FileName = arg;
                    break;
                }
            }
            return linearization;
        }
        private SimulationProject GetSimulationProject()
        {
            SimulationProject simulationProject = new SimulationProject() {FileName = arg};
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
                case "lin":
                case "linearize":
                case "linearise":
                    return GetLinearization();
                case "help":
                case "-h":
                case "--help":
                    return GetHelp();
                case "-v":
                case "--version":
                    return GetVersion();
                case null:
                    return new Main();
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