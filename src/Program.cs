/* Copyright (C) 2019-2022 Junruoyu Zheng. Home page: https://junruoyu-zheng.gitee.io/ligral

   Distributed under MIT license.
   See file LICENSE for detail or copy at https://opensource.org/licenses/MIT
*/

using System;

namespace Ligral
{
    public class Program
    {
        private static Logger logger = new Logger("Main");
        public static string Version = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();
        static void Main(string[] args)
        {
            try
            {
                Options options = new Options(args);
                Command command = options.GetCommand();
                if (command.RequestHelp is bool requestHelp && requestHelp)
                {
                    logger.Prompt(command.HelpInfo);
                }
                else
                {
                    command.Run();
                }
            }
            catch (LigralException)
            {
                logger.Throw();
                logger.Warn($"Unexpected error occurred, ligral exited with error.");
            }
            catch (Exception e)
            {
                if (Logger.LogFile is null)
                {
                    Logger.LogFile = "ligral.log";
                }
                logger.Fatal(e);
            }
        }
    }
}
