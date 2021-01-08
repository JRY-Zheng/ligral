/* Copyright (C) 2019-2021 Junruoyu Zheng. Home page: https://junruoyu-zheng.gitee.io/ligral

   Distributed under MIT license.
   See file LICENSE for detail or copy at https://opensource.org/licenses/MIT
*/

using System.Collections.Generic;
using Ligral.Component;
using Ligral.Simulation;

namespace Ligral.Extension
{
    public interface IPlugin
    {
        string ReferenceName {get;}
        int MajorVersion {get;}
        int MinerVersion {get;}
        int PatchVersion {get;}
        string Document {get;}
        Dictionary<string,System.Func<Model>> ModelTypePool {get;}
        Solver GetSolver(string solverName);
    }
}