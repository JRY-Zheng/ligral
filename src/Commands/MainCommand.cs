/* Copyright (C) 2019-2022 Junruoyu Zheng. Home page: https://junruoyu-zheng.gitee.io/ligral

   Distributed under MIT license.
   See file LICENSE for detail or copy at https://opensource.org/licenses/MIT
*/

namespace Ligral.Commands
{
    class MainCommand : Command
    {
        public override string HelpInfo {get => "";}

        public override void Run()
        {
            logger.Prompt(@"Copyright (c) Ligral Tech. All rights reserved. 
                    __    _                  __
                   / /   (_)___ __________ _/ /
                  / /   / / __ `/ ___/ __ `/ / 
                 / /___/ / /_/ / /  / /_/ / /  
                /_____/_/\__, /_/   \__,_/__/   
                        /____/                 
----------------------------------------------------------------
Hi, Ligral is a textual language for simulation.

Usage:
    ligral exm --all        to download example projects.
    ligral main.lig         to parse and simulate main.lig project.
    ligral --help           to view more detailed helping info.
    
Learn more:
    Visit https://junruoyu-zheng.gitee.io/ligral
    Clone the source at https://gitee.com/junruoyu-zheng/ligral");
        }
    }
}