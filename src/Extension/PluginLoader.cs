/* Copyright (C) 2019-2021 Junruoyu Zheng. Home page: https://junruoyu-zheng.gitee.io/ligral

   Distributed under MIT license.
   See file LICENSE for detail or copy at https://opensource.org/licenses/MIT
*/

using System.Collections.Generic;
using System.Reflection;
using System;
using Ligral.Component;

namespace Ligral.Extension
{
    class PluginLoader
    {
        public void Load(string dllFileName)
        {
            Assembly assembly = Assembly.LoadFile(dllFileName);
            Type[] types = assembly.GetTypes();
            foreach (Type t in types)
            {
                if (t.GetInterface("IPlugin")!=null)
                {
                    IPlugin plugin = (IPlugin) assembly.CreateInstance(t.FullName);
                    foreach (string modelName in plugin.ModelTypePool.Keys)
                    {
                        ModelManager.ModelTypePool.Add(modelName, plugin.ModelTypePool[modelName]);
                    }
                }
            }
        }
    }
}