/* Copyright (C) 2019-2021 Junruoyu Zheng. Home page: https://junruoyu-zheng.gitee.io/ligral

   Distributed under MIT license.
   See file LICENSE for detail or copy at https://opensource.org/licenses/MIT
*/

using System;
using System.Collections.Generic;
using Ligral.Component;
using Ligral.Extension;
using LigralPlugins.Control.Models;

namespace LigralPlugins.Control
{
    public class ControlPlugin : IPlugin
    {
        public Dictionary<string, Func<Model>> ModelTypePool {get; private set;} = new Dictionary<string, Func<Model>>()
        {
            {"TestModel", ()=>new TestModel()}
        };
    }
}
