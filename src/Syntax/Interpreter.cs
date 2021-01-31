/* Copyright (C) 2019-2021 Junruoyu Zheng. Home page: https://junruoyu-zheng.gitee.io/ligral

   Distributed under MIT license.
   See file LICENSE for detail or copy at https://opensource.org/licenses/MIT
*/

using System.Collections.Generic;
using Dict=System.Collections.Generic.Dictionary<string,object>;
using System.IO;
using System.Linq;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;
using Ligral.Component.Models;
using Ligral.Syntax.ASTs;
using Ligral.Component;
using Ligral.Extension;

namespace Ligral.Syntax
{
    class Interpreter
    {
        private string rootFolder;
        private string relativeFolder;
        private string folder;
        private string currentFileName;
        public string CurrentFileName 
        {
            get
            {
                return currentFileName;
            }
            set
            {
                currentFileName = value;
                if (!files.Contains(currentFileName))
                {
                    files.Add(currentFileName);
                }
                logger.Debug($"Current file changed to {currentFileName}");
            }
        }
        private ScopeSymbolTable currentScope;
        private Dictionary<string, ScopeSymbolTable> modules = new Dictionary<string, ScopeSymbolTable>();
        private List<string> files = new List<string>();
        private Logger logger = new Logger("Interpreter");
        private static Interpreter instance;
        public delegate void CompletedHandler();
        public static event CompletedHandler Completed;
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
        public static Interpreter GetInstance(string filename)
        {
            if (instance == null)
            {
                instance = new Interpreter();
            }
            instance.CurrentFileName = Path.GetFullPath(filename);
            string folder = Path.GetDirectoryName(filename);
            instance.folder = folder;
            instance.rootFolder = folder;
            instance.relativeFolder = folder;
            return instance;
        }
        public static Interpreter GetInstance()
        {
            if (instance == null)
            {
                instance = new Interpreter();
            }
            return instance;
        }
        private Interpreter() {}
        public void Interpret()
        {
            Interpret(Path.GetFileName(GetInstance().CurrentFileName), true);
            if (Completed != null) Completed();
        }
        public void Interpret(ProgramAST programAST)
        {
            logger.Info($"Interpreter started at {CurrentFileName}");
            Visit(programAST);
        }
        public void Interpret(StatementsAST statementsAST)
        {
            Visit(statementsAST);
        }
        public void Interpret(string fileName, bool global=false)
        {
            string fullFileName = Path.Join(folder, fileName);
            if (Directory.Exists(fullFileName))
            {
                fullFileName = Path.Join(fullFileName, "index.lig");
            }
            else if (File.Exists(fullFileName)){}
            else
            {
                fullFileName += ".lig";
            }
            string nextFileName = Path.GetFullPath(fullFileName);
            if (modules.ContainsKey(nextFileName))
            {
                currentScope = modules[nextFileName];
                return;
            }
            string lastFileName = CurrentFileName;
            CurrentFileName = nextFileName;
            string text;
            try
            {
                text = File.ReadAllText(fullFileName);
            }
            catch (IOException)
            {
                throw logger.Error(new NotFoundException($"File {fullFileName}"));
            }
            relativeFolder = Path.GetDirectoryName(fullFileName);
            Parser parser = new Parser();
            parser.Load(text, files.IndexOf(CurrentFileName));
            ProgramAST programAST;
            if (global)
            {
                programAST = parser.Parse();
            }
            else
            {
                programAST = parser.Parse(fileName);
            }
            Interpret(programAST);
            CurrentFileName = lastFileName;
        }
        public ScopeSymbolTable SetScope(ScopeSymbolTable scope)
        {
            ScopeSymbolTable originalScope = currentScope;
            currentScope = scope;
            return originalScope;
        }
        public string GetFileNameByIndex(int index)
        {
            return files[index];
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
            case ConfListAST confListAST:
                return Visit(confListAST);
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
            case RouteInPortAST routeInPortAST:
                return Visit(routeInPortAST);
            case RouteNullablePortAST routeNullablePortAST:
                return Visit(routeNullablePortAST);
            case RouteAST routeAST:
                return Visit(routeAST);
            case SignatureAST signatureAST:
                return Visit(signatureAST);
            case null:
                return null;
            default:
                throw logger.Error(new LigralException($"Unknown AST {ast.GetType().Name}"));
            }
        }
        private object Visit(ProgramAST programAST)
        {
            currentScope = new ScopeSymbolTable(programAST.Name, 0);
            modules[CurrentFileName] = currentScope;
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
                // throw logger.Error(new SemanticException(idAST.FindToken(), $"Undefined variable {idAST.Id}"));
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
                throw logger.Error(new SemanticException(rowAST.FindToken(), "Null matrix."));
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
                try
                {
                    matrix = matrix.Append(_matrix);
                }
                catch (System.ArgumentException e)
                {
                    throw logger.Error(new SemanticException(item.FindToken(), e.Message));
                }
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
                throw logger.Error(new SemanticException(matrixAST.FindToken(), "Null matrix."));
            }
            foreach(RowAST row in matrixAST.Rows.GetRange(1, matrixAST.Rows.Count-1))
            {
                try
                {
                    matrix = matrix.Stack(Visit(row));
                }
                catch (System.ArgumentException e)
                {
                    throw logger.Error(new SemanticException(row.FindToken(), e.Message));
                }
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
                        throw logger.Error(new SemanticException(rowMuxAST.Items[i].FindToken(), "Model in matrix mux should have only single out port"));
                    }
                    else
                    {
                        linkable.Connect(0, hStack.Expose(i));
                    }
                    break;
                default:
                    throw logger.Error(new SemanticException(rowMuxAST.Items[i].FindToken(), "Model or port expected"));
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
                    throw logger.Error(new SemanticException(matrixMuxAST.Rows[i].FindToken(), "Group in matrix mux should have only single out port"));
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
                        throw logger.Error(new SemanticException(rowDeMuxAST.Items[i].FindToken(), "Model in matrix demux should have only single in port"));
                    }
                    else
                    {
                        canOutputMatrix = canOutputMatrix && linkable.OutPortCount() == 1;
                        split.Connect(i, linkable.Expose(0));
                    }
                    break;
                default:
                    throw logger.Error(new SemanticException(rowDeMuxAST.Items[i].FindToken(), "Model expected"));
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
                    throw logger.Error(new SemanticException(matrixDeMuxAST.Rows[i].FindToken(), "Group in matrix mux should have only single in port"));
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
            if (value is ILinkable linkable && !linkable.IsConfigured)
            {
                linkable.Configure(new Dict());
            }
            return new KeyValuePair<string, object>(key, value);
        }
        private ILinkable Visit(DeclareAST declareAST)
        {
            string id = Visit(declareAST.Id);
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
                if (!currentScope.Insert(modelSymbol, false))
                {
                    throw logger.Error(new SemanticException(declareAST.Id.ReferenceToken, $"Duplicated ID {id}"));
                }
                return linkable;
            }
            else
            {
                throw logger.Error(new SemanticException(modelTypeAST.FindToken(), $"Invalid type"));
            }
        }
        private ILinkable Visit(ConfigureAST configureAST)
        {
            AST modelAST = configureAST.Model;
            object modelObject = Visit(modelAST);
            ILinkable linkable = modelObject as ILinkable;
            if (linkable==null)
            {
                Model constant = ModelManager.Create("Constant");
                string name = "";
                while (modelAST is PointerAST pointerAST)
                {
                    name = "." + pointerAST.Member.Id + name;
                    modelAST = pointerAST.ScopeName;
                }
                if (modelAST is IdAST idAST)
                {
                    constant.Name = idAST.Id + name;
                }
                linkable = constant;
                Dict dictionary = new Dict(){{"value", modelObject}};
                linkable.Configure(dictionary);
            }
            try
            {
                linkable.Configure(Visit(configureAST.ModelParameters));
            }
            catch (LigralException)
            {
                throw logger.Error(new SemanticException(configureAST.FindToken()));
            }
            return linkable;
        }
        private Group Visit(BinOpAST binOpAST)
        {
            object leftObject = Visit(binOpAST.Left);
            ILinkable left = leftObject as ILinkable;
            object rightObject = Visit(binOpAST.Right);
            ILinkable right = rightObject as ILinkable;
            try
            {
                switch (binOpAST.Operator.Value)
                {
                case "+":
                    return left.Add(right);
                case "-":
                    return left.Subtract(right);
                case "*":
                    return left.Multiply(right);
                case "/":
                    return left.Divide(right);
                case "^":
                    return left.Power(right);
                case ".*":
                    return left.BroadcastMultiply(right);
                case "./":
                    return left.BroadcastDivide(right);
                case ".^":
                    return left.BroadcastPower(right);
                default:
                    throw logger.Error(new SemanticException(binOpAST.FindToken(), "Invalid operator"));
                }
            }
            catch (LigralException)
            {
                throw logger.Error(new SemanticException(binOpAST.FindToken(), "Calculation fault"));
            }
            catch (System.Exception e)
            {
                throw logger.Error(new SemanticException(binOpAST.FindToken(), e.Message));
            }
        }
        private Group Visit(UnaryOpAST unaryOpAST)
        {
            ILinkable value = Visit(unaryOpAST.Value) as ILinkable;
            switch (unaryOpAST.Operator.Value)
            {
            case "+":
                return value.Positive();
            case "-":
                return value.Negative();
            default:
                throw logger.Error(new SemanticException(unaryOpAST.FindToken(), "Invalid operator"));
            }
        }
        private object Visit(ValBinOpAST valBinOpAST)
        {
            Signal left = new Signal(Visit(valBinOpAST.Left));
            Signal right = new Signal(Visit(valBinOpAST.Right));
            try
            {
                switch (valBinOpAST.Operator.Value)
                {
                case "+":
                    return (left+right).Unpack();
                case "-":
                    return (left-right).Unpack();
                case "*":
                    return (left*right).Unpack();
                case "/":
                    if (right.Unpack() is double v && v == 0)
                    {
                        throw logger.Error(new SemanticException(valBinOpAST.Right.FindToken(), "0 Division"));
                    }
                    return (left/right).Unpack();
                case "^":
                    return (left.RaiseToPower(right)).Unpack();
                case ".*":
                    return (left.BroadcastMultiply(right)).Unpack();
                case "./":
                    return (left.BroadcastDivide(right)).Unpack();
                case ".^":
                    return (left.BroadcastPower(right)).Unpack();
                default:
                    throw logger.Error(new SemanticException(valBinOpAST.FindToken(), "Invalid operator"));
                }
            }
            catch (LigralException)
            {
                throw logger.Error(new SemanticException(valBinOpAST.FindToken(), "Calculation fault"));
            }
            catch (System.Exception e)
            {
                throw logger.Error(new SemanticException(valBinOpAST.FindToken(), e.Message));
            }
        }
        private object Visit(ValUnaryOpAST valUnaryOpAST)
        {
            Signal signal = new Signal(Visit(valUnaryOpAST.Value));
            switch (valUnaryOpAST.Operator.Value)
            {
            case "+":
                return (+signal).Unpack();
            case "-":
                return (-signal).Unpack();
            default:
                throw logger.Error(new SemanticException(valUnaryOpAST.FindToken(), "Invalid operator"));
            }
        }
        private Dictionary<string, object> Visit(ConfAST confAST)
        {
            string id = Visit(confAST.Id);
            object value;
            try
            {
                value = Visit(confAST.Expression);
            }
            catch 
            {
                throw logger.Error(new SemanticException(confAST.FindToken(), "Only digit, boolean, string and nested conf are accepted"));
            }
            if (!confAST.Nested)
            {
                Settings settings = Settings.GetInstance();
                try
                {
                    settings.AddSetting(id, value);
                }
                catch (LigralException)
                {
                    throw logger.Error(new SemanticException(confAST.FindToken()));
                }
            }
            var dict = new Dictionary<string, object>();
            dict.Add(id, value);
            return dict;
        }
        private Dictionary<string, object> Visit(ConfListAST confListAST)
        {
            var dict = new Dictionary<string, object>();
            foreach (var conf in confListAST.Confs)
            {
                var singleConfDict = Visit(conf);
                foreach (string key in singleConfDict.Keys)
                {
                    dict[key] = singleConfDict[key];
                }
            }
            return dict;
        }
        private object Visit(FromOpAST fromOpAST)
        {
            string id = Visit(fromOpAST.Id);
            object value;
            TypeSymbol typeSymbol;
            value = Visit(fromOpAST.Expression);
            Symbol symbol;
            switch (value)
            {
            case Matrix<double> matrix:
                typeSymbol = currentScope.Lookup("MATRIX") as TypeSymbol;
                symbol = new MatrixSymbol(id, typeSymbol, matrix);
                break;
            case double val:
                typeSymbol = currentScope.Lookup("DIGIT") as TypeSymbol;
                symbol = new DigitSymbol(id, typeSymbol, val);
                break;
            default:
                throw logger.Error(new SemanticException(fromOpAST.FindToken(), "Only number or matrix is accepted"));
            }
            if (!currentScope.Insert(symbol, false))
            {
                throw logger.Error(new SemanticException(fromOpAST.Id.FindToken(), $"Duplicated ID {id}"));
            }
            return value;
        }
        private Group Visit(GotoOpAST gotoOpAST)
        {
            ILinkable source = Visit(gotoOpAST.Source) as ILinkable;
            ILinkable destination = Visit(gotoOpAST.Destination) as ILinkable;
            if (source!=null && destination!=null)
            {
                try
                {
                    source.Connect(destination);
                }
                catch (LigralException)
                {
                    throw logger.Error(new SemanticException(gotoOpAST.FindToken()));
                }
            }
            else
            {
                throw logger.Error(new SemanticException(gotoOpAST.FindToken(), "Invalid connection"));
            }
            Group group = new Group();
            group.AddInputModel(source);
            group.AddOutputModel(destination);
            return group;
        }
        private object Visit(ImportAST importAST)
        {
            try
            {
                ImportLocalFile(importAST);
            }
            catch (NotFoundException)
            {
                string pluginName = Visit(importAST.FileName.First());
                if (!PluginManager.Plugins.ContainsKey(pluginName))
                {
                    throw logger.Error(new SemanticException(importAST.FindToken(), $"No local file nor plugin was found."));
                }
                logger.Solve();
                if (importAST.FileName.Count > 1 || importAST.Relative)
                {
                    throw logger.Error(new SemanticException(importAST.FindToken(), "Plugin reference does not support path."));
                }
                try
                {
                    if (importAST.Symbols.Count == 0)
                    {
                        PluginManager.ImportPlugin(pluginName, currentScope);
                    }
                    else
                    {
                        List<string> items = importAST.Symbols.ConvertAll(symbol => Visit(symbol));
                        PluginManager.ImportPlugin(pluginName, currentScope, items);
                    }
                }
                catch (PluginException)
                {
                    throw logger.Error(new SemanticException(importAST.FindToken(), $"Cannot import plugin {pluginName}"));
                }
            }
            return null;
        }
        private void ImportLocalFile(ImportAST importAST)
        {
            ScopeSymbolTable mainScope = currentScope;
            string fileName = string.Join('/', importAST.FileName.ConvertAll(file=>Visit(file)));
            folder = importAST.Relative ? relativeFolder : rootFolder;
            Interpret(fileName);
            if (importAST.Symbols.Count == 0)
            {
                currentScope = mainScope.Merge(currentScope);
            }
            else
            {
                foreach (var symbolName in importAST.Symbols)
                {
                    var symbol = currentScope.Lookup(Visit(symbolName), false);
                    if (symbol == null)
                    {
                        throw logger.Error(new SemanticException(symbolName.FindToken(), $"Cannot import {Visit(symbolName)} from {importAST.FileName.Last()}"));
                    }
                    if (!mainScope.Insert(symbol, false))
                    {
                        throw logger.Error(new SemanticException(symbolName.FindToken(), $"Cannot import {Visit(symbolName)} since it has already exists"));
                    }
                }
                currentScope = mainScope;
            }
        }
        private object Visit(UsingAST usingAST)
        {
            try
            {
                UsingLocalFile(usingAST);
            }
            catch (NotFoundException)
            {
                string pluginName = Visit(usingAST.FileName.First());
                if (!PluginManager.Plugins.ContainsKey(pluginName))
                {
                    throw logger.Error(new SemanticException(usingAST.FindToken(), $"No local file nor plugin was found."));
                }
                logger.Solve();
                if (usingAST.FileName.Count > 1 || usingAST.Relative)
                {
                    throw logger.Error(new SemanticException(usingAST.FindToken(), "Plugin reference does not support path."));
                }
                string module;
                if (usingAST.ModuleName != null)
                {
                    module = Visit(usingAST.ModuleName);
                }
                else
                {
                    module = pluginName;
                }
                try
                {
                    PluginManager.UsingPlugin(pluginName, currentScope, module);
                }
                catch (PluginException)
                {
                    throw logger.Error(new SemanticException(usingAST.FindToken(), $"Cannot using plugin {pluginName}"));
                }
            }
            return null;
        }
        private void UsingLocalFile(UsingAST usingAST)
        {
            string fileName = string.Join('/', usingAST.FileName.ConvertAll(file=>Visit(file)));
            TypeSymbol scopeType = currentScope.Lookup("SCOPE") as TypeSymbol;
            ScopeSymbolTable mainScope = currentScope;
            ScopeSymbolTable scopeSymbolTable = mainScope;
            folder = usingAST.Relative ? relativeFolder : rootFolder;
            Interpret(fileName);
            ScopeSymbol scopeSymbol;
            string module;
            if (usingAST.ModuleName != null)
            {
                module = Visit(usingAST.ModuleName);
            }
            else
            {
                module = Visit(usingAST.FileName.Last());
                foreach (WordAST folder in usingAST.FileName.Take(usingAST.FileName.Count - 1))
                {
                    var symbol = scopeSymbolTable.Lookup(folder.Word);
                    if (symbol != null && symbol is ScopeSymbol scope)
                    {
                        scopeSymbolTable = scope.GetValue() as ScopeSymbolTable;
                    }
                    else if (folder.Word == "..")
                    {
                        continue;
                    }
                    else
                    {
                        var temp = new ScopeSymbolTable(folder.Word, 0);
                        scopeSymbol = new ScopeSymbol(folder.Word, scopeType, temp);
                        if (!scopeSymbolTable.Insert(scopeSymbol, false))
                        {
                            throw logger.Error(new SemanticException(folder.FindToken(), $"Cannot using {folder.Word} since it has already exists, consider rename it."));
                        }
                        scopeSymbolTable = temp;
                    }
                }
            }
            scopeSymbol = new ScopeSymbol(module, scopeType, currentScope);
            if (!scopeSymbolTable.Insert(scopeSymbol, false))
            {
                throw logger.Error(new SemanticException((usingAST.ModuleName??usingAST.FileName.Last()).FindToken(), $"Cannot using {module} since it has already exists."));
            }
            currentScope = mainScope;
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
                throw logger.Error(new SemanticException(pointerAST.ScopeName.FindToken(), "Scope expected"));
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
                            TypeSymbol typeSymbol = currentScope.Lookup("Node") as TypeSymbol;
                            ModelSymbol modelSymbol = new ModelSymbol(signalName, typeSymbol, node);
                            if (!currentScope.Insert(modelSymbol, false))
                            {
                                throw logger.Error(new SemanticException(selectAST.Port.PortName.ReferenceToken, $"Duplicated ID {signalName}"));
                            }
                        }
                        Group group = new Group();
                        group.AddInputModel(linkable);
                        group.AddOutputModel(outPort);
                        return group;
                    default:
                        throw logger.Error(new SemanticException(selectAST.Port.FindToken(), "Ambiguous port"));
                    }
                }
                catch (LigralException)
                {
                    throw logger.Error(new SemanticException(selectAST.Port.FindToken()));
                }
            }
            else
            {
                throw logger.Error(new SemanticException(selectAST.ModelObject.FindToken(), "Non model object"));
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
                throw logger.Error(new LigralException($"Unknown type {linkable}"));
            }
        }
        private RouteInherit Visit(InheritAST inheritAST)
        {
            RouteInherit routeInherit = new RouteInherit();
            routeInherit.Name = Visit(inheritAST.Name);
            routeInherit.Type = Visit(inheritAST.Type);
            if (!(currentScope.Lookup(routeInherit.Type) is TypeSymbol typeSymbol && typeSymbol.Type.Name == "SIGN"))
            {
                throw logger.Error(new SemanticException(inheritAST.Type.FindToken(), $"{routeInherit.Type} is not a valid signature"));
            }
            return routeInherit;
        }
        private RouteParam Visit(RouteParamAST routeParamAST)
        {
            RouteParam routeParam = new RouteParam();
            routeParam.Name = Visit(routeParamAST.Name);
            routeParam.Type = Visit(routeParamAST.Type);
            if (routeParam.Type != null)
            {
                TypeSymbol baseTypeSymbol = currentScope.Lookup(routeParam.Type) as TypeSymbol;
                if (baseTypeSymbol == null || baseTypeSymbol.Type.Name != "SIGN")
                {
                    throw logger.Error(new SemanticException(routeParamAST.Type.FindToken(), "Explicit type of a parameter must be a signature."));
                }
            }
            if (routeParamAST.DefaultValue!=null)
            {
                object defaultValue = Visit(routeParamAST.DefaultValue);
                if (routeParam.Type==null)
                {
                    if (defaultValue is double || defaultValue is Matrix<double>)
                    {
                        routeParam.DefaultValue = defaultValue;
                    }
                    else
                    {
                        throw logger.Error(new SemanticException(routeParamAST.DefaultValue.FindToken(), $"Type inconsistency of {routeParam.Name}, digit or matrix expected"));
                    }
                }
                else
                {
                    throw logger.Error(new SemanticException(routeParamAST.DefaultValue.FindToken(), "Default value not supported when type is model or route."));
                }
            }
            return routeParam;
        }
        private List<RouteParam> Visit(RouteParamsAST routeParamsAST)
        {
            return routeParamsAST.Parameters.ConvertAll(routeParam=>Visit(routeParam));
        }
        private RouteInPort Visit(RouteNullablePortAST routeNullablePortAST)
        {
            return new RouteInPort() 
            {
                Name = Visit(routeNullablePortAST.Id),
                Nullable = true,
                Default = new Signal(Visit(routeNullablePortAST.Expression)??0)
            };
        }
        private List<string> Visit(RoutePortAST routePortAST)
        {
            return routePortAST.Ports.ConvertAll(routePort=>(Visit(routePort)));
        }
        private List<RouteInPort> Visit(RouteInPortAST routeInPortAST)
        {
            return routeInPortAST.Ports.ConvertAll(routePort=>
                {
                    switch (Visit(routePort))
                    {
                    case string name:
                        return new RouteInPort() {Name = name, Nullable = false};
                    case RouteInPort routeInPort:
                        return routeInPort;
                    default:
                        throw logger.Error(new SemanticException(routePort.FindToken(), "Unknown type for in port."));
                    }
                });
        }
        private object Visit(RouteAST routeAST)
        {
            RouteConstructor routeConstructor = new RouteConstructor();
            List<RouteInPort> inPortList = Visit(routeAST.InPorts);
            List<string> inPortNameList = inPortList.ConvertAll(inPort => inPort.Name);
            List<string> outPortNameList = Visit(routeAST.OutPorts);
            if (routeAST.OutPorts.Ports.Find(port => inPortNameList.Contains(Visit(port))) is WordAST duplicatedPort)
            {
                throw logger.Error(new SemanticException(duplicatedPort.FindToken(), "Out ports should be distinguished from in ports"));
            }
            // if (routeAST.InPorts.Ports.Find(port => inPortNameList.Count(name => name==Visit(port))>1) is WordAST duplicatedInPort)
            if (inPortNameList.Find(inPortName => inPortNameList.Count(name => name == inPortName)>1) is string duplicatedName)
            {
                var duplicatedInPort = routeAST.InPorts.Ports[(inPortNameList.IndexOf(duplicatedName))];
                throw logger.Error(new SemanticException(duplicatedInPort.FindToken(), "In ports should be unique"));
            }
            if (routeAST.OutPorts.Ports.Find(port => outPortNameList.Count(name => name==Visit(port))>1) is WordAST duplicatedOutPort)
            {
                throw logger.Error(new SemanticException(duplicatedOutPort.FindToken(), "Out ports should be unique"));
            }
            object definition = Visit(routeAST.Definition);
            switch (definition)
            {
            case RouteInherit routeInherit:
                routeConstructor.SetUp(routeInherit);
                TypeSymbol typeSymbol = (TypeSymbol) currentScope.Lookup(routeInherit.Type);
                Signature signature = (Signature) typeSymbol.GetValue();
                if (!signature.Validate(inPortNameList, outPortNameList))
                {
                    throw logger.Error(new SemanticException(((InheritAST)routeAST.Definition).Type.FindToken(), $"Inconsistency between in ports/out ports and the signature {routeConstructor.Type}"));
                }
                break;
            case string routeName:
                routeConstructor.SetUp(routeName);
                break;
            default:
                throw logger.Error(new SemanticException(routeAST.Definition.FindToken(), "Invalid Definition"));
            }
            routeConstructor.SetUp(currentScope.scopeLevel+1, currentScope);
            List<RouteParam> parameters = Visit(routeAST.Parameters);
            if (routeAST.Parameters.Parameters.Find(parameter => inPortNameList.Contains(Visit(parameter).Name)) is RouteParamAST duplicatedParamInPort)
            {
                throw logger.Error(new SemanticException(duplicatedParamInPort.FindToken(), "In ports should be distinguished from parameters"));
            }
            if (routeAST.Parameters.Parameters.Find(parameter => outPortNameList.Contains(Visit(parameter).Name)) is RouteParamAST duplicatedParamOutPort)
            {
                throw logger.Error(new SemanticException(duplicatedParamOutPort.FindToken(), "Out ports should be distinguished from parameters"));
            }
            if (routeAST.Parameters.Parameters.Find(parameter => parameters.Count(param => param.Name==Visit(parameter).Name)>1) is RouteParamAST duplicatedParam)
            {
                throw logger.Error(new SemanticException(duplicatedParam.FindToken(), "Parameters should be unique"));
            }
            routeConstructor.SetUp(parameters);
            routeConstructor.SetUp(inPortList, outPortNameList);
            routeConstructor.SetUp(routeAST.Statements, CurrentFileName);
            TypeSymbol routeType = currentScope.Lookup(routeConstructor.Type) as TypeSymbol;
            TypeSymbol routeSymbol = new TypeSymbol(routeConstructor.Name, routeType, routeConstructor);
            if (!currentScope.Insert(routeSymbol, false))
            {
                throw logger.Error(new SemanticException(routeAST.FindToken(), $"Cannot declare {routeConstructor.Name} since it already exists."));
            }
            return null;
        }
        private object Visit(SignatureAST signatureAST)
        {
            string name = Visit(signatureAST.TypeName);
            List<string> inPortNameList = Visit(signatureAST.InPorts);
            List<string> outPortNameList = Visit(signatureAST.OutPorts);
            if (signatureAST.OutPorts.Ports.Find(port => inPortNameList.Contains(Visit(port))) is WordAST duplicatedPort)
            {
                throw logger.Error(new SemanticException(duplicatedPort.FindToken(), "Out ports should be distinguished from in ports"));
            }
            if (signatureAST.InPorts.Ports.Find(port => inPortNameList.Count(name => name==Visit(port))>1) is WordAST duplicatedInPort)
            {
                throw logger.Error(new SemanticException(duplicatedInPort.FindToken(), "In ports should be unique"));
            }
            if (signatureAST.OutPorts.Ports.Find(port => outPortNameList.Count(name => name==Visit(port))>1) is WordAST duplicatedOutPort)
            {
                throw logger.Error(new SemanticException(duplicatedOutPort.FindToken(), "Out ports should be unique"));
            }
            Signature signature = new Signature(name, inPortNameList, outPortNameList);
            TypeSymbol typeSymbol = (TypeSymbol) currentScope.Lookup("SIGN");
            TypeSymbol signatureSymbol = new TypeSymbol(name, typeSymbol, signature);
            if (!currentScope.Insert(signatureSymbol, false))
            {
                throw logger.Error(new SemanticException(signatureAST.FindToken(), $"Cannot declare {name} since it already exists."));
            }
            return null;
        }
    }
}