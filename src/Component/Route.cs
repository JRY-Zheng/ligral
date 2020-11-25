using System.Collections.Generic;
using Ligral.Syntax.ASTs;
using Ligral.Syntax;

namespace Ligral.Component
{
    class Route : Group
    {
        public string Name
        {
            get
            {
                return RouteScope.ScopeName;
            }
            set
            {
                RouteScope.ScopeName = value;
            }
        }
        public string Type;
        public string Base;
        public ScopeSymbolTable RouteScope;
        private List<RouteParam> parameters;
        private List<string> inPortNameList;
        private List<string> outPortNameList;
        private StatementsAST statementsAST;
        public void SetUp(
            string type, 
            string baseType, 
            ScopeSymbolTable scope, 
            List<RouteParam> parameters,
            List<string> inPortNameList, 
            List<string> outPortNameList, 
            StatementsAST statementsAST)
        {
            Type = type;
            Base = baseType;
            RouteScope = scope;
            this.inPortNameList = inPortNameList;
            this.outPortNameList = outPortNameList;
            this.parameters = parameters;
            this.statementsAST = statementsAST;
        }
        public override string GetTypeName()
        {
            return Type;
        }
        private void Interpret()
        {
            Interpreter interpreter = Interpreter.GetInstance();
            ScopeSymbolTable scope = interpreter.SetScope(RouteScope);
            foreach (string inPortName in inPortNameList)
            {
                Model model = ModelManager.Create("<Input>");
                model.Name = inPortName;
                inputModels.Add(model);
                TypeSymbol modelType = RouteScope.Lookup(Type) as TypeSymbol;
                ModelSymbol modelSymbol = new ModelSymbol(inPortName, modelType, model);
                RouteScope.Insert(modelSymbol);
            }
            foreach (string outPortName in outPortNameList)
            {
                Model model = ModelManager.Create("<Output>");
                model.Name = outPortName;
                outputModels.Add(model);
                TypeSymbol modelType = RouteScope.Lookup(Type) as TypeSymbol;
                ModelSymbol modelSymbol = new ModelSymbol(outPortName, modelType, model);
                RouteScope.Insert(modelSymbol);
            }
            interpreter.Interpret(statementsAST);
            interpreter.SetScope(scope);
        }
        public override Port Expose(string portName)
        {
            Model inputModel = inputModels.ConvertAll(model => (Model) model).Find(model=>model.Name==portName);
            Model outputModel = outputModels.ConvertAll(model => (Model) model).Find(model=>model.Name==portName);
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
                throw new LigralException($"No port named {portName} in {Type}");
            }
        }
        public override void Configure(Dictionary<string, object> dictionary)
        {
            if (IsConfigured)
            {
                return;
            }
            IsConfigured = true;
            // route is first constructed then Isconfigured, 
            // but construction needs configuration.
            foreach (RouteParam routeParam in parameters)
            {
                object value;
                if (routeParam.DefaultValue==null)
                {
                    if (dictionary.ContainsKey(routeParam.Name))
                    {
                        value = dictionary[routeParam.Name];
                    }
                    else
                    {
                        throw new LigralException(string.Format("Parameter {0} is required but not provided.", routeParam.Name));
                    }
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
                    ILinkable linkable = value as ILinkable;// validation in interpreter
                    if (linkable!=null && RouteScope.IsInheritFrom(linkable.GetTypeName(), routeParam.Type))
                    {
                        TypeSymbol modelType = RouteScope.Lookup(routeParam.Type) as TypeSymbol;
                        ModelSymbol modelSymbol = new ModelSymbol(routeParam.Name, modelType, linkable);
                        RouteScope.Insert(modelSymbol);
                    }
                    else
                    {
                        throw new LigralException($"Type inconsistency for {routeParam.Name}, {routeParam.Type} expected");
                    }
                }
            }
            Interpret();
        }
    }
}