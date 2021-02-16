/* Copyright (C) 2019-2021 Junruoyu Zheng. Home page: https://junruoyu-zheng.gitee.io/ligral

   Distributed under MIT license.
   See file LICENSE for detail or copy at https://opensource.org/licenses/MIT
*/

namespace Ligral.Commands
{
    class VersionCommand : Command
    {
        public override string HelpInfo {get => @"Parameter: --version & -v:
    No values           print the current version of ligral.
";}
    }
}