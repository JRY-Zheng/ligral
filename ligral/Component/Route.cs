using System.Collections.Generic;
using Ligral.Syntax.ASTs;
using Ligral.Syntax;

namespace Ligral.Component
{
    class Route : Group
    {
        public string Name;
        public string Type;
        public ScopeSymbolTable RouteScope;
        private List<RouteParam> parameters;
        private StatementsAST statementsAST;
        public void SetUp(string name, string type, ScopeSymbolTable scope, List<RouteParam> parameters, StatementsAST statementsAST)
        {
            Name = name;
            Type = type;
            RouteScope = scope;
            this.parameters = parameters;
            this.statementsAST = statementsAST;
        }
        public override string GetTypeName()
        {
            return Name;
        }
        private void Interpret()
        {
            Interpreter interpreter = new Interpreter();
            interpreter.SetScope(RouteScope);
            interpreter.Interpret(statementsAST);
        }
        public override Port Expose(string portName)
        {
            Model inputModel = inputModels.Find(model=>model.Name==portName);
            Model outputModel = outputModels.Find(model=>model.Name==portName);
            if (inputModel!=null)
            {
                return inputModel.Expose("input");
            }
            else if (outputModel!=null)
            {
                return outputModel.Expose("output");
            }
            else
            {
                throw new LigralException($"No port named {portName} in {Name}");
            }
        }
        public override void Configure(Dictionary<string, object> dictionary)
        {
            if (Configured)
            {
                return;
            }
            Configured = true;
            // route is first constructed then configured, 
            // but construction needs configuration.
            foreach (RouteParam routeParam in parameters)
            {
                object value;
                if (routeParam.DefaultValue==null)
                {
                    value = ObtainKeyValue(dictionary, routeParam.Name);
                }
                else
                {
                    if (dictionary.ContainsKey(routeParam.Name))
                    {
                        value = dictionary[routeParam.Name];
                    }
                    else
                    {
                        value = routeParam.DefaultValue;
                    }
                }
                if (routeParam.Type==null)
                {
                    double digit;
                    try
                    {
                        digit = (double) value;
                    }
                    catch
                    {
                        throw new LigralException($"Type inconsistency of {routeParam.Name}, digit expected");
                    }
                    TypeSymbol digitType = RouteScope.Lookup("DIGIT") as TypeSymbol;
                    DigitSymbol digitSymbol = new DigitSymbol(routeParam.Name, digitType, digit);
                    RouteScope.Insert(digitSymbol);
                }
                else
                {
                    ModelBase modelBase = value as ModelBase;// validation in interpreter
                    if (modelBase!=null && RouteScope.IsInheritFrom(modelBase.GetTypeName(), routeParam.Type))
                    {
                        TypeSymbol modelType = RouteScope.Lookup(routeParam.Type) as TypeSymbol;
                        ModelSymbol modelSymbol = new ModelSymbol(routeParam.Name, modelType, modelBase);
                        RouteScope.Insert(modelSymbol);
                    }
                    else
                    {
                        throw new LigralException($"Type inconsistence for {routeParam.Name}, {routeParam.Type} expected");
                    }
                }
            }
            Interpret();
        }
    }
}