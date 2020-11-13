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
        public ScopeSymbolTable RouteScope;
        private List<RouteParam> parameters;
        private List<string> inPortNameList;
        private List<string> outPortNameList;
        private StatementsAST statementsAST;
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
        public void SetUp(StatementsAST statementsAST)
        {
            this.statementsAST = statementsAST;
        }
        public Route Construct()
        {
            Route route = new Route();
            Interpreter interpreter = Interpreter.GetInstance();
            ScopeSymbolTable enclosingScope = interpreter.SetScope(RouteScope);
            foreach (string inPortName in inPortNameList)
            {
                Model model = ModelManager.Create("<Input>");
                model.Name = inPortName;
                route.inputModels.Add(model);
                TypeSymbol modelType = RouteScope.Lookup(Type) as TypeSymbol;
                ModelSymbol modelSymbol = new ModelSymbol(inPortName, modelType, model);
                RouteScope.Insert(modelSymbol);
            }
            foreach (string outPortName in outPortNameList)
            {
                Model model = ModelManager.Create("<Output>");
                model.Name = outPortName;
                route.outputModels.Add(model);
                TypeSymbol modelType = RouteScope.Lookup(Type) as TypeSymbol;
                ModelSymbol modelSymbol = new ModelSymbol(outPortName, modelType, model);
                RouteScope.Insert(modelSymbol);
            }
            interpreter.SetScope(enclosingScope);
            route.SetUp(Name, Type, RouteScope.Clone(), parameters, statementsAST);
            route.Name = Name + id.ToString();
            id++;
            return route;
        }
    }
}