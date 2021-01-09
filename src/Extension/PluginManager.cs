/* Copyright (C) 2019-2021 Junruoyu Zheng. Home page: https://junruoyu-zheng.gitee.io/ligral

   Distributed under MIT license.
   See file LICENSE for detail or copy at https://opensource.org/licenses/MIT
*/

using System.Collections.Generic;
using Ligral.Syntax;
using Ligral.Simulation;
using Ligral.Component;

namespace Ligral.Extension
{
    static class PluginManager
    {
        private static Logger logger = new Logger("PluginManager");
        public static Dictionary<string, IPlugin> Plugins = new Dictionary<string, IPlugin>();
        public static void ImportPlugin(string pluginName, ScopeSymbolTable currentScope, IEnumerable<string> items=null)
        {
            if (Plugins.ContainsKey(pluginName))
            {
                IPlugin plugin = Plugins[pluginName];
                if (ModelManager.ExtendedModelTypePool.ContainsKey(pluginName))
                {
                    throw logger.Error(new PluginException(plugin, $"Cannot import {pluginName} since the same plugin has already exists."));
                }
                else
                {
                    ModelManager.ExtendedModelTypePool.Add(pluginName, plugin.ModelTypePool);
                }
                items = items??plugin.ModelTypePool.Keys;
                TypeSymbol modelType = currentScope.Lookup("MODEL") as TypeSymbol;
                foreach (string modelName in items)
                {
                    if (!plugin.ModelTypePool.ContainsKey(modelName))
                    {
                        throw logger.Error(new PluginException(plugin, $"No model named {modelName}"));
                    }
                    ScopedModelType scopedModelType = new ScopedModelType(){ ModelName = modelName, ScopeName = pluginName };
                    if (!currentScope.Insert(new TypeSymbol(modelName, modelType, scopedModelType), false))
                    {
                        throw logger.Error(new PluginException(plugin, $"Cannot import model {modelName} since the same symbol has already existed."));
                    }
                }
            }
            else
            {
                throw logger.Error(new NotFoundException($"Plugin {pluginName}"));
            }
        }
        public static void UsingPlugin(string pluginName, ScopeSymbolTable currentScope, string moduleName)
        {
            if (Plugins.ContainsKey(pluginName))
            {
                IPlugin plugin = Plugins[pluginName];
                if (ModelManager.ExtendedModelTypePool.ContainsKey(moduleName))
                {
                    throw logger.Error(new PluginException(plugin, $"Cannot using {moduleName} since the same plugin has already exists."));
                }
                else
                {
                    ModelManager.ExtendedModelTypePool.Add(moduleName, plugin.ModelTypePool);
                }
                ScopeSymbolTable pluginScope = new ScopeSymbolTable(plugin.ReferenceName, currentScope.scopeLevel+1, currentScope);
                TypeSymbol scopeType = currentScope.Lookup("SCOPE") as TypeSymbol;
                TypeSymbol modelType = currentScope.Lookup("MODEL") as TypeSymbol;
                foreach (string modelName in plugin.ModelTypePool.Keys)
                {
                    ScopedModelType scopedModelType = new ScopedModelType(){ ModelName = modelName, ScopeName = moduleName };
                    pluginScope.Insert(new TypeSymbol(modelName, modelType, scopedModelType));
                }
                ScopeSymbol scopeSymbol = new ScopeSymbol(moduleName, scopeType, pluginScope);
                if (!currentScope.Insert(scopeSymbol, false))
                {
                    throw logger.Error(new PluginException(plugin, $"Cannot using {moduleName} since the same symbol has already exists."));
                }
            }
            else
            {
                throw logger.Error(new NotFoundException($"Plugin {pluginName}"));
            }
        }
        public static Solver GetSolver(string solverName, string pluginName)
        {
            if (!Plugins.ContainsKey(pluginName))
            {
                throw logger.Error(new NotFoundException($"Plugin {pluginName}"));
            }
            else
            {
                return Plugins[pluginName].GetSolver(solverName);
            }
        }
        public static Solver GetSolver(string solverName)
        {
            Solver solver = null;
            foreach (IPlugin plugin in Plugins.Values)
            {
                solver = plugin.GetSolver(solverName);
                if (solver != null)
                {
                    return solver;
                }
            }
            throw logger.Error(new NotFoundException($"Solver {solverName}."));
        }
    }
}