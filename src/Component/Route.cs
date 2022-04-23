/* Copyright (C) 2019-2021 Junruoyu Zheng. Home page: https://junruoyu-zheng.gitee.io/ligral

   Distributed under MIT license.
   See file LICENSE for detail or copy at https://opensource.org/licenses/MIT
*/

using System.Collections.Generic;
using System.Linq;
using MathNet.Numerics.LinearAlgebra;
using Ligral.Syntax.ASTs;
using Ligral.Syntax;
using Ligral.Component.Models;

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
        private List<RouteInPort> inPortNameList;
        private List<RoutePort> outPortNameList;
        private StatementsAST statementsAST;
        private string routeFileName;
        protected Logger loggerInstance;
        protected Logger logger
        {
            get
            {
                if (loggerInstance is null)
                {
                    loggerInstance = new Logger(Name);
                }
                return loggerInstance;
            }
        }
        public void SetUp(
            string type, 
            string baseType, 
            ScopeSymbolTable scope, 
            List<RouteParam> parameters,
            List<RouteInPort> inPortNameList, 
            List<RoutePort> outPortNameList, 
            StatementsAST statementsAST,
            string routeFileName)
        {
            Type = type;
            Base = baseType;
            RouteScope = scope;
            this.inPortNameList = inPortNameList;
            this.outPortNameList = outPortNameList;
            this.parameters = parameters;
            this.statementsAST = statementsAST;
            this.routeFileName = routeFileName;
        }
        public override string GetTypeName()
        {
            return Type;
        }
        private void Interpret()
        {
            Interpreter interpreter = Interpreter.GetInstance();
            string lastFileName = interpreter.CurrentFileName;
            interpreter.CurrentFileName = routeFileName;
            ScopeSymbolTable scope = interpreter.SetScope(RouteScope);
            foreach (var routeInPort in inPortNameList)
            {
                Input model = (Input) ModelManager.Create("<Input>", routeInPort.InputToken);
                model.Name = routeInPort.Name;
                if (routeInPort.Nullable)
                {
                    model.SetDefaultSource(routeInPort.Default);
                }
                inputModels.Add(model);
                TypeSymbol modelType = RouteScope.Lookup(Type) as TypeSymbol;
                ModelSymbol modelSymbol = new ModelSymbol(routeInPort.Name, modelType, model);
                RouteScope.Insert(modelSymbol);
            }
            foreach (var routeOutPort in outPortNameList)
            {
                Model model = ModelManager.Create("<Output>", routeOutPort.PortToken);
                model.Name = routeOutPort.Name;
                outputModels.Add(model);
                TypeSymbol modelType = RouteScope.Lookup(Type) as TypeSymbol;
                ModelSymbol modelSymbol = new ModelSymbol(routeOutPort.Name, modelType, model);
                RouteScope.Insert(modelSymbol);
            }
            int modelsCount = ModelManager.ModelPool.Count;
            interpreter.Interpret(statementsAST);
            var innerModels = ModelManager.ModelPool.Skip(modelsCount);
            if (inputModels.Find(model => ((Model)model).IsConnected()) is Model connectedModel)
            {
                throw logger.Error(new ModelException(connectedModel, "Input cannot be connected inside the route."));
            }
            if (outputModels.Union(innerModels).ToList().Find(model => !((Model)model).IsConnected()) is Model unconnectedModel)
            {
                throw logger.Error(new ModelException(unconnectedModel, "models inside the route apart from inputs cannot be unconnected"));
            }
            interpreter.SetScope(scope);
            interpreter.CurrentFileName = lastFileName;
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
                throw logger.Error(new LigralException($"No port named {portName} in {Type}"));
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
                if (dictionary.ContainsKey(routeParam.Name))
                {
                    value = dictionary[routeParam.Name];
                }
                else if (routeParam.DefaultValue!=null)
                {
                    value = routeParam.DefaultValue;
                }
                else
                {
                    throw logger.Error(new LigralException(string.Format("Parameter {0} is required but not provided.", routeParam.Name)));
                }
                if (routeParam.Type==null)
                {
                    switch (value)
                    {
                    case double digit:
                        TypeSymbol digitType = RouteScope.Lookup("DIGIT") as TypeSymbol;
                        DigitSymbol digitSymbol = new DigitSymbol(routeParam.Name, digitType, digit);
                        RouteScope.Insert(digitSymbol);
                        break;
                    case Matrix<double> matrix:
                        TypeSymbol matrixType = RouteScope.Lookup("MATRIX") as TypeSymbol;
                        MatrixSymbol matrixSymbol = new MatrixSymbol(routeParam.Name, matrixType, matrix);
                        RouteScope.Insert(matrixSymbol);
                        break;
                    default:
                        throw logger.Error(new LigralException($"Type inconsistency of {routeParam.Name}, digit or matrix expected"));
                    }
                }
                else
                {
                    TypeSymbol typeSymbol = (TypeSymbol) RouteScope.Lookup(routeParam.Type);
                    Signature signature = (Signature) typeSymbol.GetValue();
                    switch (value)
                    {
                    case Model model:
                        if (signature.Derive(model))
                        {
                            TypeSymbol matrixType = RouteScope.Lookup("MODEL") as TypeSymbol;
                            ModelSymbol modelSymbol = new ModelSymbol(routeParam.Name, typeSymbol, model);
                            RouteScope.Insert(modelSymbol);
                            break;
                        }
                        else
                        {
                            throw logger.Error(new ModelException(model, $"Type inconsistency for {routeParam.Name} in {Name}, in ports or out ports of model {model.GetTypeName()} is not the same as {routeParam.Type}'s."));
                        }
                    case Route route:
                        if (signature.Derive(route))
                        {
                            TypeSymbol matrixType = RouteScope.Lookup(route.Type) as TypeSymbol;
                            ModelSymbol modelSymbol = new ModelSymbol(routeParam.Name, typeSymbol, route);
                            RouteScope.Insert(modelSymbol);
                            break;
                        }
                        else
                        {
                            throw logger.Error(new LigralException($"Type inconsistency for {routeParam.Name} in {Name}, {route.GetTypeName()} is not derived from {routeParam.Type}'s."));
                        }
                    default:
                        throw logger.Error(new LigralException($"Type inconsistency for {routeParam.Name} in {Name}, model or route expected."));
                    }
                }
            }
            Interpret();
        }
    }
}