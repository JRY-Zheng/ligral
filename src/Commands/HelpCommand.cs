/* Copyright (C) 2019-2022 Junruoyu Zheng. Home page: https://junruoyu-zheng.gitee.io/ligral

   Distributed under MIT license.
   See file LICENSE for detail or copy at https://opensource.org/licenses/MIT
*/

namespace Ligral.Commands
{
    class HelpCommand : Command
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

        public override void Run()
        {
            logger.Prompt($"Help on ligral:\n{HelpInfo}");
        }
    }
}