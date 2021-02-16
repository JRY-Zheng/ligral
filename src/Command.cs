/* Copyright (C) 2019-2021 Junruoyu Zheng. Home page: https://junruoyu-zheng.gitee.io/ligral

   Distributed under MIT license.
   See file LICENSE for detail or copy at https://opensource.org/licenses/MIT
*/

namespace Ligral
{
    abstract class Command
    {
        public bool? RequestHelp;
        protected Logger logger;
        public abstract string HelpInfo {get;}
        public Command()
        {
            logger = new Logger(GetType().Name);
        }
        public abstract void Run();
    }
}