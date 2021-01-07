/* Copyright (C) 2019-2021 Junruoyu Zheng. Home page: https://junruoyu-zheng.gitee.io/ligral

   Distributed under MIT license.
   See file LICENSE for detail or copy at https://opensource.org/licenses/MIT
*/

using System.Collections.Generic;
using Ligral.Syntax.ASTs;
using Ligral.Syntax;

namespace Ligral.Component
{
    struct RouteParam
    {
        public string Name;
        public string Type;
        public object DefaultValue;
    }
    struct RouteInherit
    {
        public string Name;
        public string Type;
    }

    class RouteConstructor
    {
        public string Name;
        public string Type;
        private int id = 1;
        private List<RouteParam> parameters;
        private List<string> inPortNameList;
        private List<string> outPortNameList;
        private StatementsAST statementsAST;
        private string routeFileName;
        private int scopeLevel;
        private ScopeSymbolTable enclosingScope;
        public void SetUp(RouteInherit routeInherit)
        {
            Name = routeInherit.Name;
            Type = routeInherit.Type;
        }
        public void SetUp(string name)
        {
            Name = name;
            Type = "ROUTE";
        }
        public void SetUp(List<RouteParam> parameters)
        {
            this.parameters = parameters;
        }
        public void SetUp(List<string> inPortNameList, List<string> outPortNameList)
        {
            this.inPortNameList = inPortNameList;
            this.outPortNameList = outPortNameList;
        }
        public void SetUp(int scopeLevel, ScopeSymbolTable enclosingScope)
        {
            this.scopeLevel = scopeLevel;
            this.enclosingScope = enclosingScope;
        }
        public void SetUp(StatementsAST statementsAST, string routeFileName)
        {
            this.statementsAST = statementsAST;
            this.routeFileName = routeFileName;
        }
        public Route Construct()
        {
            Route route = new Route();
            ScopeSymbolTable routeScope = new ScopeSymbolTable(Name, scopeLevel, enclosingScope);
            route.SetUp(Name, Type, routeScope, parameters, inPortNameList, outPortNameList, statementsAST, routeFileName);
            route.Name = Name + id.ToString();
            id++;
            return route;
        }
    }
}