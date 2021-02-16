/* Copyright (C) 2019-2021 Junruoyu Zheng. Home page: https://junruoyu-zheng.gitee.io/ligral

   Distributed under MIT license.
   See file LICENSE for detail or copy at https://opensource.org/licenses/MIT
*/

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
    }
}