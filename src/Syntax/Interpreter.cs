using System.Collections.Generic;
using Dict=System.Collections.Generic.Dictionary<string,object>;
using System.IO;
using System;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;
using Ligral.Component.Models;
using Ligral.Syntax.ASTs;
using Ligral.Component;

namespace Ligral.Syntax
{
    class Interpreter
    {
        private string folder;
        private ScopeSymbolTable currentScope;
        private static Interpreter instance;
        public static string ScopeName 
        {
            get 
            {
                if (instance == null)
                {
                    return null;
                }
                else
                {
                    return instance.currentScope.ScopeName;
                }
            }
        }
        public static Interpreter GetInstance(string folder = ".")
        {
            if (instance == null)
            {
                instance = new Interpreter();
            }
            instance.folder = folder;
            return instance;
        }
        private Interpreter() {}
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
            ProgramAST programAST = parser.Parse(fileName);
            Interpret(programAST);
        }
        public ScopeSymbolTable SetScope(ScopeSymbolTable scope)
        {
            ScopeSymbolTable originalScope = currentScope;
            currentScope = scope;
            return originalScope;
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
            case null:
                return null;
            default:
                throw new LigralException($"Unknown AST {ast.GetType().Name}");
            }
        }
        private object Visit(ProgramAST programAST)
        {
            currentScope = new ScopeSymbolTable(programAST.Name, 0);
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
                TypeSymbol typeSymbol = currentScope.Lookup("Node") as TypeSymbol;
                Node node = typeSymbol.GetValue() as Node;
                node.Name = idAST.Id;
                ModelSymbol modelSymbol = new ModelSymbol(idAST.Id, typeSymbol, node);
                currentScope.Insert(modelSymbol);
                return node;
                // throw new SemanticException(idAST.FindToken(), $"Undefined variable {idAST.Id}");
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
                case ILinkable linkable:
                    if (linkable.OutPortCount() != 1)
                    {
                        throw new SemanticException(rowMuxAST.Items[i].FindToken(), "Model in matrix mux should have only single out port");
                    }
                    else
                    {
                        linkable.Connect(0, hStack.Expose(i));
                    }
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
        private Group Visit(RowDeMuxAST rowDeMuxAST)
        {
            Model split = ModelManager.Create("Split");
            var modelList = new List<ILinkable>();
            bool canOutputMatrix = true;
            for (int i = 0; i < rowDeMuxAST.Items.Count; i++)
            {
                switch (Visit(rowDeMuxAST.Items[i]))
                {
                case ILinkable linkable:
                    modelList.Add(linkable);
                    if (linkable.InPortCount() != 1)
                    {
                        throw new SemanticException(rowDeMuxAST.Items[i].FindToken(), "Model in matrix demux should have only single in port");
                    }
                    else
                    {
                        canOutputMatrix = canOutputMatrix && linkable.OutPortCount() == 1;
                        split.Connect(i, linkable.Expose(0));
                    }
                    break;
                default:
                    throw new SemanticException(rowDeMuxAST.Items[i].FindToken(), "Model expected");
                } 
            }
            Group group = new Group();
            group.AddInputModel(split);
            if (canOutputMatrix)
            {
                Model hStack = ModelManager.Create("HStack");
                for (int i = 0; i < modelList.Count; i++)
                {
                    ILinkable linkable = modelList[i];
                    linkable.Connect(0, hStack.Expose(i));
                }
                group.AddOutputModel(hStack);
            }
            return group;
        }
        private Group Visit(MatrixDeMuxAST matrixDeMuxAST)
        {
            Model vSplit = ModelManager.Create("VSplit");
            var groupList = new List<Group>();
            bool canOutputMatrix = true;
            for (int i = 0; i < matrixDeMuxAST.Rows.Count; i++)
            {
                Group rowGroup = Visit(matrixDeMuxAST.Rows[i]);
                groupList.Add(rowGroup);
                if (rowGroup.InPortCount() != 1)
                {
                    throw new SemanticException(matrixDeMuxAST.Rows[i].FindToken(), "Group in matrix mux should have only single in port");
                }
                else
                {
                    canOutputMatrix = canOutputMatrix && rowGroup.OutPortCount() == 1;
                    vSplit.Connect(i, rowGroup.Expose(0));
                }
            }
            Group group = new Group();
            group.AddInputModel(vSplit);
            if (canOutputMatrix)
            {
                Model vStack = ModelManager.Create("VStack");
                for (int i = 0; i < groupList.Count; i++)
                {
                    Group rowGroup = groupList[i];
                    rowGroup.Connect(0, vStack.Expose(i));
                }
                group.AddOutputModel(vStack);
            }
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
        private ILinkable Visit(DeclareAST declareAST)
        {
            string id = Visit(declareAST.Id);
            Symbol valueSymbol = currentScope.Lookup(id, false);
            if (valueSymbol!=null)
            {
                throw new SemanticException(declareAST.Id.ReferenceToken, $"Duplicated ID {id}");
            }
            else
            {
                AST modelTypeAST = declareAST.ModelType;
                ILinkable linkable = Visit(modelTypeAST) as ILinkable;
                if (linkable!=null)
                {
                    switch (linkable)
                    {
                    case Model model:
                        model.Name = id;
                        break;
                    case Route route:
                        route.Name = id;
                        break;
                    }
                    TypeSymbol typeSymbol = currentScope.Lookup(linkable.GetTypeName()) as TypeSymbol;
                    ModelSymbol modelSymbol = new ModelSymbol(id, typeSymbol, linkable);
                    currentScope.Insert(modelSymbol);
                    return linkable;
                }
                else
                {
                    throw new SemanticException(modelTypeAST.FindToken(), $"Invalid type");
                }
            }
        }
        private ILinkable Visit(ConfigureAST configureAST)
        {
            AST modelAST = configureAST.Model;
            object modelObject = Visit(modelAST);
            ILinkable linkable = modelObject as ILinkable;
            if (linkable==null)
            {
                linkable = ModelManager.Create("Constant");
                Dict dictionary = new Dict(){{"value", modelObject}};
                linkable.Configure(dictionary);
            }
            try
            {
                linkable.Configure(Visit(configureAST.ModelParameters));
            }
            catch (LigralException ex)
            {
                throw new SemanticException(configureAST.FindToken(), ex.Message);
            }
            return linkable;
        }
        private Group Visit(BinOpAST binOpAST)
        {
            object leftObject = Visit(binOpAST.Left);
            ILinkable left = leftObject as ILinkable;
            object rightObject = Visit(binOpAST.Right);
            ILinkable right = rightObject as ILinkable;
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
            ILinkable value = Visit(unaryOpAST.Value) as ILinkable;
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
            Symbol symbol = currentScope.Lookup(id, false);
            if (symbol!=null)
            {
                throw new SemanticException(fromOpAST.Id.FindToken(), $"Duplicated ID {id}");
            }
            else
            {
                switch (value)
                {
                case Matrix<double> matrix:
                    typeSymbol = currentScope.Lookup("MATRIX") as TypeSymbol;
                    MatrixSymbol matrixSymbol = new MatrixSymbol(id, typeSymbol, matrix);
                    currentScope.Insert(matrixSymbol);
                    break;
                case double val:
                    typeSymbol = currentScope.Lookup("DIGIT") as TypeSymbol;
                    DigitSymbol digitSymbol = new DigitSymbol(id, typeSymbol, val);
                    currentScope.Insert(digitSymbol);
                    break;
                default:
                    throw new SemanticException(fromOpAST.FindToken(), "Only digit is accepted");
                }
                return value;
            }
        }
        private Group Visit(GotoOpAST gotoOpAST)
        {
            ILinkable source = Visit(gotoOpAST.Source) as ILinkable;
            ILinkable destination = Visit(gotoOpAST.Destination) as ILinkable;
            if (source!=null && destination!=null)
            {
                source.Connect(destination);
            }
            else
            {
                throw new SemanticException(gotoOpAST.FindToken(), "Invalid connection");
            }
            Group group = new Group();
            group.AddInputModel(source);
            group.AddOutputModel(destination);
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
            // currentScope = new ScopeSymbolTable(module, 0);
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
        private ILinkable Visit(SelectAST selectAST)
        {
            ILinkable linkable = Visit(selectAST.ModelObject) as ILinkable;
            if (linkable!=null)
            {
                string portId = Visit(selectAST.Port.PortId);
                try
                {
                    Port port = linkable.Expose(portId);
                    switch (port)
                    {
                    case InPort inPort:
                        return inPort;
                    case OutPort outPort:
                        string signalName = selectAST.Port.PortName == null? null : Visit(selectAST.Port.PortName);
                        if (signalName != null)
                        {
                            Node node = ModelManager.Create("Node") as Node;
                            outPort.SignalName = signalName;
                            node.Name = outPort.SignalName;
                            outPort.Bind(node.Expose(0));
                            Symbol valueSymbol = currentScope.Lookup(signalName, false);
                            if (valueSymbol!=null)
                            {
                                throw new SemanticException(selectAST.Port.PortName.ReferenceToken, $"Duplicated ID {signalName}");
                            }
                            TypeSymbol typeSymbol = currentScope.Lookup("Node") as TypeSymbol;
                            ModelSymbol modelSymbol = new ModelSymbol(signalName, typeSymbol, node);
                            currentScope.Insert(modelSymbol);
                        }
                        Group group = new Group();
                        group.AddInputModel(linkable);
                        group.AddOutputModel(outPort);
                        return group;
                    default:
                        throw new SemanticException(selectAST.Port.FindToken(), "Ambiguous port");
                    }
                }
                catch (LigralException ex)
                {
                    throw new SemanticException(selectAST.Port.FindToken(), ex.Message);
                }
            }
            else
            {
                throw new SemanticException(selectAST.ModelObject.FindToken(), "Non model object");
            }
        }
        private Group Visit(ChainAST chainAST)
        {
            ILinkable linkable = Visit(chainAST.Tree) as ILinkable;
            if (linkable as Model!=null)
            {
                Group group = new Group();
                Model model = linkable as Model;
                group.AddInputModel(model);
                group.AddOutputModel(model);
                return group;
            }
            else if (linkable as Group!=null)
            {
                return linkable as Group;
            }
            else
            {
                throw new LigralException($"Unknown type {linkable}");
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
                    ILinkable linkable = routeParam.DefaultValue as ILinkable;// validation in interpreter
                    if (linkable==null || !currentScope.IsInheritFrom(linkable.GetTypeName(), routeParam.Type))
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
            switch (definition)
            {
            case RouteInherit routeInherit:
                routeConstructor.SetUp(routeInherit);
                break;
            case string routeName:
                routeConstructor.SetUp(routeName);
                break;
            default:
                throw new SemanticException(routeAST.Definition.FindToken(), "Invalid Definition");
            }
            Symbol symbol = currentScope.Lookup(routeConstructor.Name, false);
            if (symbol != null)
            {
                throw new SemanticException(routeAST.Definition.FindToken(), $"Cannot declare {routeConstructor.Name} since it already exists.");
            }
            routeConstructor.SetUp(currentScope.scopeLevel+1, currentScope);
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