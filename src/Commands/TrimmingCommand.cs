/* Copyright (C) 2019-2021 Junruoyu Zheng. Home page: https://junruoyu-zheng.gitee.io/ligral

   Distributed under MIT license.
   See file LICENSE for detail or copy at https://opensource.org/licenses/MIT
*/

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
    }
}