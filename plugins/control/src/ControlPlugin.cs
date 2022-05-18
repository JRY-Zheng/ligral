/* Copyright (C) 2019-2022 Junruoyu Zheng. Home page: https://junruoyu-zheng.gitee.io/ligral

   Distributed under MIT license.
   See file LICENSE for detail or copy at https://opensource.org/licenses/MIT
*/

using System;
using System.Collections.Generic;
using MathNet.Numerics.LinearAlgebra;
using Ligral.Component;
using Ligral.Extension;
using Ligral.Simulation;
using LigralPlugins.Control.Models;
using LigralPlugins.Control.Solvers;

namespace LigralPlugins.Control
{
    public class ControlPlugin : IPlugin
    {
        public Dictionary<string, Func<Model>> ModelTypePool {get; private set;} = new Dictionary<string, Func<Model>>()
        {
            {"TestModel", ()=>new TestModel()}
        };

        public string ReferenceName => "control";

        public int MajorVersion => 0;

        public int MinerVersion => 1;

        public int PatchVersion => 0;

        public string Document => "This plugin contains some models for control system domain.";

        public Solver GetSolver(string solverName)
        {
            switch (solverName)
            {
            case "ode2m":
                return new FixedStepRK2MSolver();
            default:
                return null;
            }
        }
    }
}
