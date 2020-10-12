using System.Collections.Generic;
using Dict=System.Collections.Generic.Dictionary<string,object>;
using System.IO;
using System;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;
using Ligral.Models;
using Ligral.ASTs;

namespace Ligral
{
    class Interpreter
    {
        private string folder;
        private ScopeSymbolTable currentScope;
        public Interpreter(string folder=".")
        {
            this.folder = folder;
        }
        public void Interpret(ProgramAST programAST)
        {
            Visit(programAST);
        }
        public void Interpret(StatementsAST statementsAST)
        {
            Visit(statementsAST);
        }
        public void AppendInterpret(ProgramAST programAST)
        {
            if (currentScope==null)
            {
                Visit(programAST);
            }
            else
            {
                Visit(programAST.Statements);
            }
        }
        public void Interpret(string fileName)
        {
            string fullFileName = Path.Join(folder, fileName+".lig");
            string text = File.ReadAllText(fullFileName);
            Parser parser = new Parser();
            parser.Load(text);
            ProgramAST programAST = parser.Parse();
            Interpret(programAST);
        }
        public void SetScope(ScopeSymbolTable scope)
        {
            currentScope = scope;
        }
        private object Visit(AST ast)
        {
            switch (ast)
            {
                case ProgramAST programAST:
                    return Visit(programAST);
                case StatementsAST statementsAST:
                    return Visit(statementsAST);
                case IdAST idAST:
                    return Visit(idAST);
                case WordAST wordAST:
                    return Visit(wordAST);
                case BoolAST boolAST:
                    return Visit(boolAST);
                case RowAST rowAST:
                    return Visit(rowAST);
                case MatrixAST matrixAST:
                    return Visit(matrixAST);
                case MatrixMuxAST matrixMuxAST:
                    return Visit(matrixMuxAST);
                case MatrixDeMuxAST matrixDeMuxAST:
                    return Visit(matrixDeMuxAST);
                case RowMuxAST rowMuxAST:
                    return Visit(rowMuxAST);
                case RowDeMuxAST rowDeMuxAST:
                    return Visit(rowDeMuxAST);
                case DigitAST digitAST:
                    return Visit(digitAST);
                case DigitBlockAST digitBlockAST:
                    return Visit(digitBlockAST);
                case StringAST stringAST:
                    return Visit(stringAST);
                case BusAST busAST:
                    return Visit(busAST);
                case DictAST dictAST:
                    return Visit(dictAST);
                case KeyValuePairAST keyValuePairAST:
                    return Visit(keyValuePairAST);
                case DeclareAST declareAST:
                    return Visit(declareAST);
                case ConfigureAST configureAST:
                    return Visit(configureAST);
                case BinOpAST binOpAST:
                    return Visit(binOpAST);
                case UnaryOpAST unaryOpAST:
                    return Visit(unaryOpAST);
                case ValBinOpAST valBinOpAST:
                    return Visit(valBinOpAST);
                case ValUnaryOpAST valUnaryOpAST:
                    return Visit(valUnaryOpAST);
                case ConfAST confAST:
                    return Visit(confAST);
                case FromOpAST fromOpAST:
                    return Visit(fromOpAST);
                case GotoOpAST gotoOpAST:
                    return Visit(gotoOpAST);
                case ImportAST importAST:
                    return Visit(importAST);
                case UsingAST usingAST:
                    return Visit(usingAST);
                case PointerAST pointerAST:
                    return Visit(pointerAST);
                case SelectAST selectAST:
                    return Visit(selectAST);
                case ChainAST chainAST:
                    return Visit(chainAST);
                case InheritAST inheritAST:
                    return Visit(inheritAST);
                case RouteParamAST routeParamAST:
                    return Visit(routeParamAST);
                case RouteParamsAST routeParamsAST:
                    return Visit(routeParamsAST);
                case RoutePortAST routePortAST:
                    return Visit(routePortAST);
                case RouteAST routeAST:
                    return Visit(routeAST);
                default:
                    throw new LigralException($"Unknown AST {ast.GetType().Name}");
            }
        }
        private object Visit(ProgramAST programAST)
        {
            currentScope = new ScopeSymbolTable("global", 0);
            return Visit(programAST.Statements);
        }
        private object Visit(StatementsAST statementsAST)
        {
            foreach (AST ast in statementsAST.Statements)
            {
                Visit(ast);
            }
            return null;
        }
        private object Visit(IdAST idAST)
        {
            Symbol symbol = currentScope.Lookup(idAST.Id);
            if (symbol==null)
            {
                throw new SemanticException(idAST.FindToken(), $"Undefined variable {idAST.Id}");
            }
            else
            {
                return symbol.GetValue();
            }
        }
        private string Visit(WordAST wordAST)
        {
            return wordAST.Word;
        }
        private bool Visit(BoolAST boolAST)
        {
            return boolAST.Bool;
        }
        private Matrix<double> Visit(RowAST rowAST)
        {
            Matrix<double> matrix;
            if (rowAST.Items.Count>0)
            {
                object obj = Visit(rowAST.Items[0]);
                matrix = obj as Matrix<double>;
                if (matrix==null)
                {
                    matrix = DenseMatrix.Create(1, 1, (double)obj);
                }
            }
            else
            {
                throw new SemanticException(rowAST.FindToken(), "Null matrix.");
            }
            foreach(AST item in rowAST.Items.GetRange(1, rowAST.Items.Count-1))
            {
                Matrix<double> _matrix;
                object obj = Visit(item);
                _matrix = obj as Matrix<double>;
                if (_matrix==null)
                {
                    _matrix = DenseMatrix.Create(1, 1, (double)obj);
                }
                matrix = matrix.Append(_matrix);
            }
            return matrix;
        }
        private Matrix<double> Visit(MatrixAST matrixAST)
        {
            Matrix<double> matrix;
            if (matrixAST.Rows.Count>0)
            {
                matrix = Visit(matrixAST.Rows[0]);
            }
            else
            {
                throw new SemanticException(matrixAST.FindToken(), "Null matrix.");
            }
            foreach(RowAST row in matrixAST.Rows.GetRange(1, matrixAST.Rows.Count-1))
            {
                matrix = matrix.Stack(Visit(row));
            }
            return matrix;
        }
        private Group Visit(RowMuxAST rowMuxAST)
        {
            Model hStack = ModelManager.Create("HStack");
            for (int i = 0; i < rowMuxAST.Items.Count; i++)
            {
                switch (Visit(rowMuxAST.Items[i]))
                {
                    case ModelBase modelBase:
                        if (modelBase.OutPortCount() != 1)
                        {
                            throw new SemanticException(rowMuxAST.Items[i].FindToken(), "Model in matrix mux should have only single out port");
                        }
                        else
                        {
                            modelBase.Connect(0, hStack.Expose(i));
                        }
                        break;
                    case OutPort outPort:
                        outPort.Bind(hStack.Expose(i));
                        break;
                    default:
                        throw new SemanticException(rowMuxAST.Items[i].FindToken(), "Model or port expected");
                } 
            }
            Group group = new Group();
            group.AddOutputModel(hStack);
            return group;
        }
        private Group Visit(MatrixMuxAST matrixMuxAST)
        {
            Model vStack = ModelManager.Create("VStack");
            for (int i = 0; i < matrixMuxAST.Rows.Count; i++)
            {
                Group rowGroup = Visit(matrixMuxAST.Rows[i]);
                if (rowGroup.OutPortCount() != 1)
                {
                    throw new SemanticException(matrixMuxAST.Rows[i].FindToken(), "Group in matrix mux should have only single out port");
                }
                else
                {
                    rowGroup.Connect(0, vStack.Expose(i));
                }
            }
            Group group = new Group();
            group.AddOutputModel(vStack);
            return group;
        }
        private double Visit(DigitAST digitAST)
        {
            return digitAST.Digit;
        }
        private Model Visit(DigitBlockAST digitBlockAST)
        {
            Model constant = ModelManager.Create("Constant");
            Dict dictionary = new Dict() {{"value", (double)digitBlockAST.Digit}};
            constant.Configure(dictionary);
            return constant;
        }
        private string Visit(StringAST stringAST)
        {
            return stringAST.String;
        }
        private Group Visit(BusAST busAST)
        {
            Group busGroup = new Group();
            foreach (ChainAST chainAST in busAST.Chains)
            {
                Group group = Visit(chainAST);
                busGroup.Merge(group);
            }
            return busGroup;
        }
        private Dict Visit(DictAST dictAST)
        {
            Dict dictionary = new Dict();
            foreach (KeyValuePairAST keyValuePairAST in dictAST.Parameters)
            {
                KeyValuePair<string,object> keyValuePair = Visit(keyValuePairAST);
                dictionary.Add(keyValuePair.Key,keyValuePair.Value);
            }
            return dictionary;
        }
        private KeyValuePair<string,object> Visit(KeyValuePairAST keyValuePairAST)
        {
            string key = Visit(keyValuePairAST.Key);
            object value = Visit(keyValuePairAST.Value);
            return new KeyValuePair<string, object>(key, value);
        }
        private ModelBase Visit(DeclareAST declareAST)
        {
            string id = Visit(declareAST.Id);
            Symbol valueSymbol = currentScope.Lookup(id);
            if (valueSymbol!=null)
            {
                throw new SemanticException(declareAST.Id.ReferenceToken, $"Duplicated ID {id}");
            }
            else
            {
                AST modelTypeAST = declareAST.ModelType;
                ModelBase modelBase = Visit(modelTypeAST) as ModelBase;
                if (modelBase!=null)
                {
                    Model model = modelBase as Model;
                    if (model!=null)
                    {
                        model.Name = id;
                    }
                    TypeSymbol typeSymbol = currentScope.Lookup(modelBase.GetTypeName()) as TypeSymbol;
                    ModelSymbol modelSymbol = new ModelSymbol(id, typeSymbol, modelBase);
                    currentScope.Insert(modelSymbol);
                    return modelBase;
                }
                else
                {
                    throw new SemanticException(modelTypeAST.FindToken(), $"Invalid type");
                }
            }
        }
        private ModelBase Visit(ConfigureAST configureAST)
        {
            AST modelAST = configureAST.Model;
            object modelObject = Visit(modelAST);
            ModelBase modelBase = modelObject as ModelBase;
            if (modelBase==null)
            {
                modelBase = ModelManager.Create("Constant");
                Dict dictionary = new Dict(){{"value", modelObject}};
                modelBase.Configure(dictionary);
            }
            try
            {
                modelBase.Configure(Visit(configureAST.ModelParameters));
            }
            catch (LigralException ex)
            {
                throw new SemanticException(configureAST.FindToken(), ex.Message);
            }
            return modelBase;
        }
        private Group Visit(BinOpAST binOpAST)
        {
            object leftObject = Visit(binOpAST.Left);
            ModelBase left = leftObject as ModelBase;
            object rightObject = Visit(binOpAST.Right);
            ModelBase right = rightObject as ModelBase;
            switch (binOpAST.Operator.Value)
            {
                case '+':
                    return left+right;
                case '-':
                    return left-right;
                case '*':
                    return left*right;
                case '/':
                    return left/right;
                case '^':
                    return left^right;
                default:
                    throw new SemanticException(binOpAST.FindToken(), "Invalid operator");
            }
        }
        private Group Visit(UnaryOpAST unaryOpAST)
        {
            ModelBase value = Visit(unaryOpAST.Value) as ModelBase;
            switch (unaryOpAST.Operator.Value)
            {
                case '+':
                    return +value;
                case '-':
                    return -value;
                default:
                    throw new SemanticException(unaryOpAST.FindToken(), "Invalid operator");
            }
        }
        private double Visit(ValBinOpAST valBinOpAST)
        {
            double left = (double) Visit(valBinOpAST.Left);
            double right = (double) Visit(valBinOpAST.Right);
            switch (valBinOpAST.Operator.Value)
            {
                case '+':
                    return left+right;
                case '-':
                    return left-right;
                case '*':
                    return left*right;
                case '/':
                    if (right==0)
                    {
                        throw new SemanticException(valBinOpAST.Right.FindToken(), "0 Division");
                    }
                    return left/right;
                case '^':
                    return Math.Pow(left, right);
                default:
                    throw new SemanticException(valBinOpAST.FindToken(), "Invalid operator");
            }
        }
        private double Visit(ValUnaryOpAST valUnaryOpAST)
        {
            double value = (double) Visit(valUnaryOpAST.Value);
            switch (valUnaryOpAST.Operator.Value)
            {
                case '+':
                    return +value;
                case '-':
                    return -value;
                default:
                    throw new SemanticException(valUnaryOpAST.FindToken(), "Invalid operator");
            }
        }
        private object Visit(ConfAST confAST)
        {
            string id = Visit(confAST.Id);
            object value;
            try
            {
                value = Visit(confAST.Expression);
            }
            catch 
            {
                throw new SemanticException(confAST.FindToken(), "Only digit, boolean and string are accepted");
            }
            Settings settings = Settings.GetInstance();
            settings.AddSetting(id, value);
            return null;
        }
        private object Visit(FromOpAST fromOpAST)
        {
            string id = Visit(fromOpAST.Id);
            object value;
            TypeSymbol typeSymbol;
            value = Visit(fromOpAST.Expression);
            if ((value  as Matrix<double> != null))
            {
                typeSymbol = currentScope.Lookup("MATRIX") as TypeSymbol;
            }
            else if (value is double)
            {
                typeSymbol = currentScope.Lookup("DIGIT") as TypeSymbol;
            }
            else 
            {
                throw new SemanticException(fromOpAST.FindToken(), "Only digit is accepted");
            }
            Symbol symbol = currentScope.Lookup(id);
            if (symbol!=null)
            {
                throw new SemanticException(fromOpAST.Id.FindToken(), $"Duplicated ID {id}");
            }
            else
            {
                if (typeSymbol.Name == "MATRIX")
                {
                    MatrixSymbol matrixSymbol = new MatrixSymbol(id, typeSymbol, value as Matrix<double>);
                    currentScope.Insert(matrixSymbol);
                }
                else
                {
                    DigitSymbol digitSymbol = new DigitSymbol(id, typeSymbol, (double) value);
                    currentScope.Insert(digitSymbol);
                }
                return value;
            }
        }
        private Group Visit(GotoOpAST gotoOpAST)
        {
            object source = Visit(gotoOpAST.Source);
            object destination = Visit(gotoOpAST.Destination);
            ModelBase sourceModel = source as ModelBase;
            ModelBase destinationModel = destination as ModelBase;
            OutPort sourceOutPort = source as OutPort;
            InPort destinationInPort = destination as InPort;
            if (sourceModel!=null && destinationModel!=null)
            {
                sourceModel.Connect(destinationModel);
            }
            else if (sourceModel!=null && destinationInPort!=null)
            {
                sourceModel.Connect(0,destinationInPort);
            }
            else if (sourceOutPort!=null && destinationModel!=null)
            {
                sourceOutPort.Bind(destinationModel.Expose(0));
            }
            else if (sourceOutPort!=null && destinationInPort!=null)
            {
                sourceOutPort.Bind(destinationInPort);
            }
            else
            {
                throw new SemanticException(gotoOpAST.FindToken(), "Invalid connection");
            }
            Group group = new Group();
            if (sourceModel as Model!=null)
            {
                group.AddInputModel(sourceModel as Model);
            }
            else
            {
                group.AddInputModel(sourceModel as Group);
            }
            if (destinationModel as Model!=null)
            {
                group.AddOutputModel(destinationModel as Model);
            }
            else
            {
                group.AddOutputModel(destinationModel as Group);
            }
            return group;
        }
        private object Visit(ImportAST importAST)
        {
            ScopeSymbolTable mainScope = currentScope;
            string fileName = string.Join('/', importAST.FileName.ConvertAll(file=>Visit(file)));
            Interpret(fileName);
            currentScope = mainScope.Merge(currentScope);
            return null;
        }
        private object Visit(UsingAST usingAST)
        {
            string module = Visit(usingAST.ModuleName);
            string fileName = string.Join('/', usingAST.FileName.ConvertAll(file=>Visit(file)));
            ScopeSymbolTable mainScope = currentScope;
            currentScope = new ScopeSymbolTable(module, 0);
            Interpret(fileName);
            TypeSymbol scopeType = currentScope.Lookup("SCOPE") as TypeSymbol;
            ScopeSymbol scopeSymbol = new ScopeSymbol(module, scopeType, currentScope);
            mainScope.Insert(scopeSymbol);
            currentScope = mainScope;
            return null;
        }
        private object Visit(PointerAST pointerAST)
        {
            ScopeSymbolTable scope = Visit(pointerAST.ScopeName) as ScopeSymbolTable;
            if (scope!=null)
            {
                ScopeSymbolTable tempScope = currentScope;
                currentScope = scope;
                object member = Visit(pointerAST.Member);
                currentScope = tempScope;
                return member;
            }
            else
            {
                throw new SemanticException(pointerAST.ScopeName.FindToken(), "Scope expected");
            }
        }
        private Model Visit(SelectAST selectAST)
        {
            ModelBase modelBase = Visit(selectAST.ModelObject) as ModelBase;
            if (modelBase!=null)
            {
                string portName = Visit(selectAST.PortName);
                try
                {
                    Port port = modelBase.Expose(portName);
                    Node node = ModelManager.Create("Node") as Node;
                    InPort inPort = port as InPort;
                    OutPort outPort = port as OutPort;
                    if (inPort!=null)
                    {
                        node.Connect(0, inPort);
                    }
                    else if (outPort!=null)
                    {
                        outPort.Bind(node.Expose(0));
                    }
                    return node;
                }
                catch (LigralException ex)
                {
                    throw new SemanticException(selectAST.PortName.FindToken(), ex.Message);
                }
            }
            else
            {
                throw new SemanticException(selectAST.ModelObject.FindToken(), "Non model object");
            }
        }
        private Group Visit(ChainAST chainAST)
        {
            ModelBase modelBase = Visit(chainAST.Tree) as ModelBase;
            if (modelBase as Model!=null)
            {
                Group group = new Group();
                Model model = modelBase as Model;
                group.AddInputModel(model);
                group.AddOutputModel(model);
                return group;
            }
            else if (modelBase as Group!=null)
            {
                return modelBase as Group;
            }
            else
            {
                throw new LigralException($"Unknown type {modelBase}");
            }
        }
        private RouteInherit Visit(InheritAST inheritAST)
        {
            RouteInherit routeInherit = new RouteInherit();
            routeInherit.Name = Visit(inheritAST.Name);
            routeInherit.Type = Visit(inheritAST.Type);
            return routeInherit;
        }
        private RouteParam Visit(RouteParamAST routeParamAST)
        {
            RouteParam routeParam = new RouteParam();
            routeParam.Name = Visit(routeParamAST.Name);
            routeParam.Type = Visit(routeParamAST.Type);
            if (routeParamAST.DefaultValue!=null)
            {
                routeParam.DefaultValue = Visit(routeParamAST.DefaultValue);
                if (routeParam.Type==null)
                {
                    try
                    {
                        double digit = (double) routeParam.DefaultValue;
                    }
                    catch
                    {
                        throw new SemanticException(routeParamAST.DefaultValue.FindToken(), $"Type inconsistency of {routeParam.Name}, digit expected");
                    }
                }
                else
                {
                    ModelBase modelBase = routeParam.DefaultValue as ModelBase;// validation in interpreter
                    if (modelBase==null || !currentScope.IsInheritFrom(modelBase.GetTypeName(), routeParam.Type))
                    {
                        throw new SemanticException(routeParamAST.DefaultValue.FindToken(), $"Type inconsistency for {routeParam.Name}, {routeParam.Type} expected");
                    }
                }
            }
            return routeParam;
        }
        private List<RouteParam> Visit(RouteParamsAST routeParamsAST)
        {
            return routeParamsAST.Parameters.ConvertAll(routeParam=>Visit(routeParam));
        }
        private List<string> Visit(RoutePortAST routePortAST)
        {
            return routePortAST.Ports.ConvertAll(routePort=>(Visit(routePort)));
        }
        private object Visit(RouteAST routeAST)
        {
            RouteConstructor routeConstructor = new RouteConstructor();
            object definition = Visit(routeAST.Definition);
            try
            {
                routeConstructor.SetUp((RouteInherit) definition);
            }
            catch {try
            {
                routeConstructor.SetUp((string) definition);
            }
            catch
            {
                throw new SemanticException(routeAST.Definition.FindToken(), "Invalid Definition");
            }}
            routeConstructor.RouteScope = new ScopeSymbolTable(routeConstructor.Name, currentScope.scopeLevel+1, currentScope);
            List<RouteParam> parameters = Visit(routeAST.Parameters);
            routeConstructor.SetUp(parameters);
            List<string> inPortNameList = Visit(routeAST.InPorts);
            List<string> outPortNameList = Visit(routeAST.OutPorts);
            routeConstructor.SetUp(inPortNameList, outPortNameList);
            routeConstructor.SetUp(routeAST.Statements);
            TypeSymbol routeType = currentScope.Lookup(routeConstructor.Type) as TypeSymbol;
            TypeSymbol routeSymbol = new TypeSymbol(routeConstructor.Name, routeType, routeConstructor);
            currentScope.Insert(routeSymbol);
            return null;
        }
    }
}