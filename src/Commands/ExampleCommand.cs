/* Copyright (C) 2019-2021 Junruoyu Zheng. Home page: https://junruoyu-zheng.gitee.io/ligral

   Distributed under MIT license.
   See file LICENSE for detail or copy at https://opensource.org/licenses/MIT
*/

using Ligral.Network;

namespace Ligral.Commands
{
    class ExampleCommand : Command
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

        public override void Run()
        {
            ExampleManager manager = new ExampleManager();
            if (DownloadAll is bool downloadAll && downloadAll)
            {
                manager.DownloadAllProjects();
            }
            else if (ExampleName is string exampleName)
            {
                manager.DownloadProject(exampleName);
            }
            else
            {
                manager.ShowExampleList();
            }
        }
    }
}