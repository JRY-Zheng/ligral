/* Copyright (C) 2019-2021 Junruoyu Zheng. Home page: https://junruoyu-zheng.gitee.io/ligral

   Distributed under MIT license.
   See file LICENSE for detail or copy at https://opensource.org/licenses/MIT
*/

namespace Ligral
{
    abstract class Command
    {
        public bool? RequestHelp;
        public abstract string HelpInfo {get;}
    }
    class Linearization : Command
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
    }
    class Trimming : Command
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
    }
    class SimulationProject : Command
    {
        public string FileName;
        public string OutputFolder;
        public double? StepSize;
        public double? StopTime;
        public bool? RealTimeSimulation;
        public bool? ToCompile;
        public bool? IsJsonFile;
        public override string HelpInfo {get => "";}
    }
    class Document : Command
    {
        public string ModelName;
        public bool? ToJson;
        public string OutputFolder;
        public override string HelpInfo {get => @"Command: doc & document
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
";}
    }
    class Example : Command
    {
        public string ExampleName;
        public bool? DownloadAll;
        public override string HelpInfo {get => @"Command: exm & example & examples
    Position parameter:
        ExampleName         optional string
            if exist        download this specific example project.
            else            if --all is not set, show all available examples.
    Named parameters:
        --all & -a          boolean
            if true         download all example projects.
            else            depends.
    Examples:
        ligral exm          print all examples on the screen
        ligral exm --all    download all example projects.
        ligral exm mass-spring-damper
                            download example mass-spring-damper.
";}
    }
    class Help : Command
    {
        public override string HelpInfo {get => @"Root:
    Position parameter: 
        FileName            required string
            if is file      interpret the file and run simulation.
            or is folder    the folder must be a package.
    Named parameters:
        --json & -j         boolean
            if true         the project is in .lig.json format.
            else            the project is in .lig format.
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
For other commands, use --help option to see detailed helping info.
Command list:
    trim, lin, exm, doc.
Example:
    ligral trim --help
";}
    }
    class Version : Command
    {
        public override string HelpInfo {get => @"Parameter: --version & -v:
    No values           print the current version of ligral.
";}
    }
    class Main : Command
    {
        public override string HelpInfo {get => "";}
    }
}