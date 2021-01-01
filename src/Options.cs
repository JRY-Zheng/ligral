namespace Ligral
{
    abstract class Command
    {}
    class SimulationProject : Command
    {
        public string FileName;
        public string OutputFolder;
        public double? StepSize;
        public double? StopTime;
        public bool? RealTimeSimulation;
        public bool? ToCompile;
        public bool? IsJsonFile;
        public override string ToString()
        {
            return $"[sim] {FileName}, to {OutputFolder} ({StepSize}:{StopTime}), -r {RealTimeSimulation} -c {ToCompile} -j {IsJsonFile}";
        }
    }
    class Document : Command
    {
        public string ModelName;
        public bool? ToJson;
        public string OutputFolder;
        public override string ToString()
        {
            return $"[doc] model={ModelName} to_json={ToJson}";
        }
    }
    class Help : Command
    {
        public override string ToString()
        {
            return "[help]";
        }
    }
    class Version : Command
    {
        public override string ToString()
        {
            return "[version]";
        }
    }
    class Main : Command
    {}
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
                    document.ToJson = true;
                    break;
                case "-o":
                case "--output":
                    document.OutputFolder = GetString();
                    break;
                default:
                    metUnknownOption = document.ModelName != null;
                    if (!metUnknownOption) document.ModelName = arg;
                    break;
                }
            }
            return document;
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
                    simulationProject.OutputFolder = GetString();
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