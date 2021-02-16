/* Copyright (C) 2019-2021 Junruoyu Zheng. Home page: https://junruoyu-zheng.gitee.io/ligral

   Distributed under MIT license.
   See file LICENSE for detail or copy at https://opensource.org/licenses/MIT
*/

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
    }
}