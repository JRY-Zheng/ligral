/* Copyright (C) 2019-2022 Junruoyu Zheng. Home page: https://junruoyu-zheng.gitee.io/ligral

   Distributed under MIT license.
   See file LICENSE for detail or copy at https://opensource.org/licenses/MIT
*/

using System.Collections.Generic;
using Ligral.Syntax.ASTs;

namespace Ligral.Syntax
{
    class Parser
    {
        private Lexer lexer;
        private Token currentToken;
        private Token currentTokenBackup;
        private Logger logger = new Logger("Parser");
        private int file;
        public Parser() {}
        public void Load(string text, int file)
        {
            lexer = new Lexer();
            lexer.Load(text, file);
            this.file = file;
            currentToken = Feed();
        }
        public ProgramAST Parse(string name = "<global>")
        {
            logger.Info($"Parser started at {name}.");
            return Program(name);
        }
        private Token Feed() => lexer.GetNextToken();
        private Token Eat(TokenType tokenType)
        {
            if (currentToken.Type == tokenType)
            {
                Token previousToken = currentToken;
                currentToken = Feed();
                return previousToken;
            }
            else
            {
                throw logger.Error(new SyntaxException(currentToken, $"{tokenType.ToString()} expected"));
            }
        }
        private void Backup()
        {
            lexer.Backup();
            currentTokenBackup = currentToken;
        }
        private void Restore()
        {
            lexer.Restore();
            currentToken = currentTokenBackup;
        }
        private ProgramAST Program(string programName)
        {
            StatementsAST statementsAST = Statements();
            return new ProgramAST(programName, statementsAST);
        }
        private StatementsAST Statements()
        {
            List<AST> statements = new List<AST>();
            while (currentToken.Type!=TokenType.EOF&&currentToken.Type!=TokenType.END)
            {
                statements.Add(Statement());
            }
            return new StatementsAST(statements);
        }
        private AST Statement()
        {
            switch (currentToken.Type)
            {
            case TokenType.CONF:
                return ProgramConfig();
            case TokenType.ASSIGN:
                return Define();
            case TokenType.USING:
                return Using();
            case TokenType.IMPORT:
                return Import();
            case TokenType.SIGN:
                return Signature();
            case TokenType.ROUTE:
                return Route();
            case TokenType.SCRIPT:
                return Script();
            default:
                return Equation();
                // ChainAST chainAST = Chain();
                // Eat(TokenType.SEMIC);
                // return chainAST;
            }
        }
        private ConfAST ProgramConfig(bool nested=false)
        {
            if (!nested) Eat(TokenType.CONF);
            StringToken idToken = Eat(TokenType.ID) as StringToken;
            WordAST wordAST = new WordAST(idToken);
            AST valueAST;
            if (currentToken.Type == TokenType.EQUAL)
            {
                Eat(TokenType.EQUAL);
                if (currentToken.Type == TokenType.TRUE || currentToken.Type == TokenType.FALSE)
                {
                    BoolToken valueToken = Eat(currentToken.Type) as BoolToken;
                    valueAST = new BoolAST(valueToken);
                }
                else if (currentToken.Type == TokenType.STRING)
                {
                    StringToken valueToken = Eat(TokenType.STRING) as StringToken;
                    valueAST = new StringAST(valueToken);
                }
                else
                {
                    valueAST = ValueExpr();
                }
                Eat(TokenType.SEMIC);
            }
            else if (currentToken.Type == TokenType.DOT)
            {
                Eat(TokenType.DOT);
                valueAST = ProgramConfig(true);
            }
            else
            {
                Eat(TokenType.COLON);
                valueAST = ConfList();
                Eat(TokenType.END);
            }
            return new ConfAST(wordAST, valueAST, nested);
        }
        private ConfListAST ConfList()
        {
            List<ConfAST> confList = new List<ConfAST>();
            while (currentToken.Type!=TokenType.EOF&&currentToken.Type!=TokenType.END)
            {
                confList.Add(ProgramConfig(true));
            }
            return new ConfListAST(confList);
        }
        private FromOpAST Define()
        {
            Eat(TokenType.ASSIGN);
            StringToken idToken = Eat(TokenType.ID) as StringToken;
            WordAST wordAST = new WordAST(idToken);
            Eat(TokenType.EQUAL);
            AST valueExprAST = ValueExpr();
            Eat(TokenType.SEMIC);
            return new FromOpAST(wordAST, valueExprAST);
        }
        private AST ValueExpr()
        {
            AST valueFactorAST = ValueFactor();
            while (currentToken.Type==TokenType.PLUS||currentToken.Type==TokenType.MINUS)
            {
                OperatorToken binOpToken = Eat(currentToken.Type) as OperatorToken;
                valueFactorAST = new ValBinOpAST(valueFactorAST, binOpToken, ValueFactor());
            }
            return valueFactorAST;
        }
        private AST ValueFactor()
        {
            AST valueAST = ValueEntity();
            while (currentToken.Type==TokenType.MUL||currentToken.Type==TokenType.DIV||currentToken.Type==TokenType.RDIV||currentToken.Type==TokenType.BCMUL||currentToken.Type==TokenType.BCDIV)
            {
                OperatorToken binOpToken = Eat(currentToken.Type) as OperatorToken;
                valueAST = new ValBinOpAST(valueAST, binOpToken, ValueEntity());
            }
            return valueAST;
        }
        private AST ValueEntity()
        {
            AST valueAST = Value();
            while (currentToken.Type==TokenType.CARET||currentToken.Type==TokenType.BCPOW)
            {
                OperatorToken binOpToken = Eat(currentToken.Type) as OperatorToken;
                valueAST = new ValBinOpAST(valueAST, binOpToken, Value());
            }
            return valueAST;
        }
        private AST Value()
        {
            if (currentToken.Type==TokenType.PLUS||currentToken.Type==TokenType.MINUS)
            {
                OperatorToken unaryOpToken = Eat(currentToken.Type) as OperatorToken;
                return new ValUnaryOpAST(unaryOpToken, Value());
            }
            else if (currentToken.Type==TokenType.LPAR)
            {
                Eat(TokenType.LPAR);
                AST valueExprAST = ValueExpr();
                Eat(TokenType.RPAR);
                return valueExprAST;
            }
            else if (currentToken.Type==TokenType.DIGIT)
            {
                return new DigitAST(Eat(TokenType.DIGIT) as DigitToken);
            }
            else if (currentToken.Type==TokenType.LBRK)
            {
                return Matrix();
            }
            else
            {
                return Pointer();
            }
        }
        private AST Matrix()
        {
            List<RowAST> rows = new List<RowAST>();
            Eat(TokenType.LBRK);
            rows.Add(Row());
            while (currentToken.Type!=TokenType.RBRK)
            {
                Eat(TokenType.SEMIC);
                rows.Add(Row());
            }
            Eat(TokenType.RBRK);
            return new MatrixAST(rows);
        }
        private RowAST Row()
        {
            List<AST> items = new List<AST>();
            items.Add(ValueExpr());
            while (currentToken.Type!=TokenType.RBRK && currentToken.Type!=TokenType.SEMIC)
            {
                Eat(TokenType.COMMA);
                items.Add(ValueExpr());
            }
            return new RowAST(items);
        }
        private AST Pointer()
        {
            if (currentToken.Type == TokenType.TILDE)
            {
                Eat(TokenType.TILDE);
                StringToken nodeToken = new StringToken(TokenType.ID, "Node", currentToken.Line, currentToken.Column, file);
                return new IdAST(nodeToken);
            }
            else if (currentToken.Type == TokenType.AT)
            {
                Eat(TokenType.AT);
                StringToken inputToken = new StringToken(TokenType.ID, "InputMarker", currentToken.Line, currentToken.Column, file);
                return new IdAST(inputToken);
            }
            else if (currentToken.Type == TokenType.DOLL)
            {
                Eat(TokenType.DOLL);
                StringToken outputToken = new StringToken(TokenType.ID, "OutputSink", currentToken.Line, currentToken.Column, file);
                return new IdAST(outputToken);
            }
            StringToken idToken = Eat(TokenType.ID) as StringToken;
            AST pointerAST = new IdAST(idToken);
            while (currentToken.Type==TokenType.DOT)
            {
                Eat(TokenType.DOT);
                idToken = Eat(TokenType.ID) as StringToken;
                pointerAST = new PointerAST(pointerAST, new IdAST(idToken));
            }
            return pointerAST;
        }
        private AST Equation()
        {
            ChainAST leftChain = Chain();
            AST ast = leftChain;
            if (currentToken.Type == TokenType.EQUAL)
            {
                OperatorToken equalToOperator = (OperatorToken)currentToken;
                Eat(currentToken.Type);
                ast = new EquationAST(leftChain, equalToOperator, Chain());
            }
            Eat(TokenType.SEMIC);
            return ast;
        }
        private ChainAST Chain()
        {
            AST nodeExprAST = NodeExpr(true);
            while (currentToken.Type==TokenType.GOTO)
            {
                OperatorToken gotoToken = (OperatorToken) currentToken;
                Eat(TokenType.GOTO);
                nodeExprAST = new GotoOpAST(nodeExprAST, NodeExpr(false), gotoToken);
            }
            return new ChainAST(nodeExprAST);
        }
        private AST NodeExpr(bool isFirstNode)
        {
            AST nodeFactorAST = NodeFactor(isFirstNode);
            while (currentToken.Type==TokenType.PLUS||currentToken.Type==TokenType.MINUS)
            {
                OperatorToken binOpToken = Eat(currentToken.Type) as OperatorToken;
                nodeFactorAST = new BinOpAST(nodeFactorAST, binOpToken, NodeFactor(isFirstNode));
            }
            return nodeFactorAST;
        }
        private AST NodeFactor(bool isFirstNode)
        {
            AST nodeAST = NodeEntity(isFirstNode);
            while (currentToken.Type==TokenType.MUL||currentToken.Type==TokenType.DIV||currentToken.Type==TokenType.RDIV||currentToken.Type==TokenType.BCMUL||currentToken.Type==TokenType.BCDIV)
            {
                OperatorToken binOpToken = Eat(currentToken.Type) as OperatorToken;
                nodeAST = new BinOpAST(nodeAST, binOpToken, NodeEntity(isFirstNode));
            }
            return nodeAST;
        }
        private AST NodeEntity(bool isFirstNode)
        {
            AST nodeAST = Node(isFirstNode);
            while (currentToken.Type==TokenType.CARET||currentToken.Type==TokenType.BCPOW)
            {
                OperatorToken binOpToken = Eat(currentToken.Type) as OperatorToken;
                nodeAST = new BinOpAST(nodeAST, binOpToken, Node(isFirstNode));
            }
            return nodeAST;
        }
        private AST Node(bool isFirstNode)
        {
            if (currentToken.Type==TokenType.PLUS||currentToken.Type==TokenType.MINUS)
            {
                OperatorToken unaryOpToken = Eat(currentToken.Type) as OperatorToken;
                return new UnaryOpAST(unaryOpToken, Node(isFirstNode));
            }
            else if (currentToken.Type==TokenType.LPAR)
            {
                Eat(TokenType.LPAR);
                BusAST busAST = Bus();
                Eat(TokenType.RPAR);
                return busAST;
            }
            else if (currentToken.Type==TokenType.DIGIT)
            {
                DigitToken digitToken = Eat(TokenType.DIGIT) as DigitToken;
                return new DigitBlockAST(digitToken);
            }
            else if (currentToken.Type==TokenType.LBRK)
            {
                if (isFirstNode)
                {
                    return MatrixMux();
                }
                else
                {
                    return MatrixDeMux();
                }
            }
            else
            {
                return Selector();
            }
        }
        private MatrixMuxAST MatrixMux()
        {
            List<RowMuxAST> rows = new List<RowMuxAST>();
            Eat(TokenType.LBRK);
            rows.Add(RowMux());
            while (currentToken.Type!=TokenType.RBRK)
            {
                Eat(TokenType.SEMIC);
                rows.Add(RowMux());
            }
            Eat(TokenType.RBRK);
            return new MatrixMuxAST(rows);
        }
        private MatrixDeMuxAST MatrixDeMux()
        {
            int? columnNumber = null;
            List<RowDeMuxAST> rows = new List<RowDeMuxAST>();
            Eat(TokenType.LBRK);
            rows.Add(RowDeMux(columnNumber, out int parsedColumnNumber));
            columnNumber = parsedColumnNumber;
            while (currentToken.Type!=TokenType.RBRK)
            {
                Eat(TokenType.SEMIC);
                rows.Add(RowDeMux(columnNumber, out parsedColumnNumber));
                columnNumber = parsedColumnNumber;
            }
            Eat(TokenType.RBRK);
            return new MatrixDeMuxAST(rows, (int)columnNumber);
        }
        private RowMuxAST RowMux()
        {
            List<AST> items = new List<AST>();
            items.Add(NodeExpr(true));
            while (currentToken.Type!=TokenType.RBRK && currentToken.Type!=TokenType.SEMIC)
            {
                Eat(TokenType.COMMA);
                items.Add(NodeExpr(true));
            }
            return new RowMuxAST(items);
        }
        private RowDeMuxAST RowDeMux(int? numberRequired, out int numberParsed)
        {
            List<AST> items = new List<AST>();
            items.Add(Selector());
            while (currentToken.Type!=TokenType.RBRK && currentToken.Type!=TokenType.SEMIC)
            {
                Eat(TokenType.COMMA);
                items.Add(Selector());
            }
            numberParsed = items.Count;
            if (numberRequired != null && numberRequired != numberParsed)
            {
                throw logger.Error(new SyntaxException(currentToken, "Inconsistency rows of a DeMux block"));
            }
            return new RowDeMuxAST(items);
        }
        private BusAST Bus()
        {
            List<ChainAST> chains = new List<ChainAST>();
            chains.Add(Chain());
            while (currentToken.Type==TokenType.COMMA)
            {
                Eat(TokenType.COMMA);
                chains.Add(Chain());
            }
            return new BusAST(chains);
        }
        private AST Selector()
        {
            AST blockAST = Block();
            if (currentToken.Type==TokenType.COLON)
            {
                Eat(TokenType.COLON);
                PortAST portAST = Port();
                blockAST = new SelectAST(blockAST, portAST);
            }
            else if (currentToken.Type==TokenType.INPORT)
            {
                Eat(TokenType.INPORT);
                PortAST portAST = NumberedPort();
                blockAST = new SelectAST(blockAST, portAST, SelectAST.PortTypes.IN);
            }
            else if (currentToken.Type==TokenType.OUTPORT)
            {
                Eat(TokenType.OUTPORT);
                PortAST portAST = NumberedPort();
                blockAST = new SelectAST(blockAST, portAST, SelectAST.PortTypes.OUT);
            }
            else if (currentToken.Type==TokenType.LPAR)
            {
                Eat(TokenType.LPAR);
                var busAST = Bus();
                Eat(TokenType.RPAR);
                blockAST = new GotoOpAST(busAST, blockAST, blockAST.FindToken());
            }
            return blockAST;
        }
        private PortAST Port()
        {
            StringToken portIdToken = Eat(TokenType.ID) as StringToken;
            WordAST portIdAST = new WordAST(portIdToken);
            WordAST portNameAST = null;
            if (currentToken.Type == TokenType.LBRK)
            {
                Eat(TokenType.LBRK);
                StringToken portNameToken = Eat(TokenType.ID) as StringToken;
                portNameAST = new WordAST(portNameToken);
                Eat(TokenType.RBRK);
            }
            return new PortAST(portIdAST, portNameAST);
        }
        private PortAST NumberedPort()
        {
            DigitToken portNoToken = Eat(TokenType.DIGIT) as DigitToken;
            NumberAST portNoAST = new NumberAST(portNoToken);
            WordAST portNameAST = null;
            if (currentToken.Type == TokenType.LBRK)
            {
                Eat(TokenType.LBRK);
                StringToken portNameToken = Eat(TokenType.ID) as StringToken;
                portNameAST = new WordAST(portNameToken);
                Eat(TokenType.RBRK);
            }
            return new PortAST(portNoAST, portNameAST);
        }
        private ConfigureAST Block()
        {
            AST declareAST = Declare();
            if (currentToken.Type==TokenType.LBRC)
            {
                DictAST dictAST = Configure();
                return new ConfigureAST(declareAST, dictAST);
            }
            else
            {
                return new ConfigureAST(declareAST, new DictAST(new List<KeyValuePairAST>()));
            }
        }
        private AST Declare()
        {
            AST pointerAST = Pointer();
            if (currentToken.Type==TokenType.LBRK)
            {
                Eat(TokenType.LBRK);
                StringToken idToken = Eat(TokenType.ID) as StringToken;
                WordAST wordAST = new WordAST(idToken);
                Eat(TokenType.RBRK);
                return new DeclareAST(pointerAST, wordAST);
            }
            else
            {
                return pointerAST;
            }
        }
        private DictAST Configure()
        {
            Eat(TokenType.LBRC);
            List<KeyValuePairAST> parameters = new List<KeyValuePairAST>();
            while (true)
            {
                Backup();
                KeyValuePairAST keyValuePairAST = Parameter();
                if (currentToken.Type!=TokenType.COMMA && currentToken.Type!=TokenType.RBRC)
                {
                    Restore();
                    keyValuePairAST = BlockParameter();
                }
                parameters.Add(keyValuePairAST);
                if (currentToken.Type!=TokenType.COMMA)
                {
                    break;
                }
                else
                {
                    Eat(TokenType.COMMA);
                }
            }
            Eat(TokenType.RBRC);
            return new DictAST(parameters);
        }
        private KeyValuePairAST Parameter()
        {
            StringToken keyToken = Eat(TokenType.ID) as StringToken;
            WordAST keyAST = new WordAST(keyToken);
            Eat(TokenType.COLON);
            if (currentToken.Type==TokenType.STRING)
            {
                StringToken valueToken = Eat(TokenType.STRING) as StringToken;
                StringAST valueAST = new StringAST(valueToken);
                return new KeyValuePairAST(keyAST, valueAST);
            }
            else
            {
                AST valueAST = ValueExpr();
                return new KeyValuePairAST(keyAST, valueAST);
            }
        }
        private KeyValuePairAST BlockParameter()
        {
            StringToken keyToken = Eat(TokenType.ID) as StringToken;
            WordAST keyAST = new WordAST(keyToken);
            Eat(TokenType.COLON);
            AST valueAST = Block();
            return new KeyValuePairAST(keyAST, valueAST);
        }
        private SignatureAST Signature()
        {
            Eat(TokenType.SIGN);
            StringToken typeNameToken = Eat(TokenType.ID) as StringToken;
            WordAST typeName = new WordAST(typeNameToken);
            Eat(TokenType.LPAR);
            RoutePortAST inPortAST;
            if (currentToken.Type!=TokenType.SEMIC)
            {
                inPortAST = RoutePorts();
            }
            else
            {
                inPortAST = new RoutePortAST(new List<WordAST>());
            }
            Eat(TokenType.SEMIC);
            RoutePortAST outPortAST;
            if (currentToken.Type!=TokenType.RPAR)
            {
                outPortAST = RoutePorts();
            }
            else
            {
                outPortAST = new RoutePortAST(new List<WordAST>());
            }
            Eat(TokenType.RPAR);
            Eat(TokenType.SEMIC);
            return new SignatureAST(typeName, inPortAST, outPortAST);
        }
        private RouteAST Route()
        {
            Eat(TokenType.ROUTE);
            AST definition = Inherit();
            Eat(TokenType.LPAR);
            RouteParamsAST routeParamsAST;
            if (currentToken.Type!=TokenType.SEMIC)
            {
                routeParamsAST = RouteParameters();
            }
            else
            {
                routeParamsAST = new RouteParamsAST(new List<RouteParamAST>());
            }
            Eat(TokenType.SEMIC);
            RouteInPortAST inPortAST;
            if (currentToken.Type!=TokenType.SEMIC)
            {
                inPortAST = RouteInPorts();
            }
            else
            {
                inPortAST = new RouteInPortAST(new List<AST>());
            }
            Eat(TokenType.SEMIC);
            RoutePortAST outPortAST;
            if (currentToken.Type!=TokenType.RPAR)
            {
                outPortAST = RoutePorts();
            }
            else
            {
                outPortAST = new RoutePortAST(new List<WordAST>());
            }
            Eat(TokenType.RPAR);
            StatementsAST statementsAST = Statements();
            Eat(TokenType.END);
            return new RouteAST(definition, routeParamsAST, inPortAST, outPortAST, statementsAST);
        }
        private AST Inherit()
        {
            StringToken nameToken = Eat(TokenType.ID) as StringToken;
            AST definitionAST = new WordAST(nameToken);
            if (currentToken.Type==TokenType.COLON)
            {
                Eat(TokenType.COLON);
                StringToken typeToken = Eat(TokenType.ID) as StringToken;
                WordAST typeAST = new WordAST(typeToken);
                definitionAST = new InheritAST(definitionAST as WordAST, typeAST);
            }
            return definitionAST;
        }
        private RouteParamsAST RouteParameters()
        {
            List<RouteParamAST> parameters = new List<RouteParamAST>();
            while (true)
            {
                parameters.Add(RouteParameter());
                if (currentToken.Type!=TokenType.COMMA)
                {
                    break;
                }
                else
                {
                    Eat(TokenType.COMMA);
                }
            }
            return new RouteParamsAST(parameters);
        }
        private RouteParamAST RouteParameter()
        {
            StringToken nameToken = Eat(TokenType.ID) as StringToken;
            StringToken typeToken = null;
            if (currentToken.Type==TokenType.COLON)
            {
                Eat(TokenType.COLON);
                typeToken = Eat(TokenType.ID) as StringToken;
            }
            WordAST nameAST = new WordAST(nameToken);
            WordAST typeAST = new WordAST(typeToken);
            if (currentToken.Type==TokenType.EQUAL)
            {
                Eat(TokenType.EQUAL);
                return new RouteParamAST(nameAST, typeAST, ValueExpr());
            }
            else
            {
                return new RouteParamAST(nameAST, typeAST);
            }
        }
        private RouteInPortAST RouteInPorts()
        {
            List<AST> ports = new List<AST>();
            while (true)
            {
                StringToken portNameToken = Eat(TokenType.ID) as StringToken;
                var name = new WordAST(portNameToken);
                if (currentToken.Type==TokenType.QUE)
                {
                    Eat(TokenType.QUE);
                    if (currentToken.Type != TokenType.COMMA && currentToken.Type != TokenType.SEMIC)
                    {
                        var value = ValueExpr();
                        ports.Add(new RouteNullablePortAST(name, value));
                    }
                    else
                    {
                        ports.Add(new RouteNullablePortAST(name, null));
                    }
                }
                else
                {
                    ports.Add(name);
                }
                if (currentToken.Type!=TokenType.COMMA)
                {
                    break;
                }
                else
                {
                    Eat(TokenType.COMMA);
                }
            }
            return new RouteInPortAST(ports);
        }
        private RoutePortAST RoutePorts()
        {
            List<WordAST> ports = new List<WordAST>();
            while (true)
            {
                StringToken portNameToken = Eat(TokenType.ID) as StringToken;
                ports.Add(new WordAST(portNameToken));
                if (currentToken.Type!=TokenType.COMMA)
                {
                    break;
                }
                else
                {
                    Eat(TokenType.COMMA);
                }
            }
            return new RoutePortAST(ports);
        }
        private AST Script()
        {
            Eat(TokenType.SCRIPT);
            if (currentToken.Type == TokenType.DOT || currentToken.Type == TokenType.ID)
            {
                return ScriptFile();
            }
            List<WordAST> parameters = null;
            if (currentToken.Type == TokenType.LPAR)
            {
                Eat(TokenType.LPAR);
                parameters = new List<WordAST>();
                while (true)
                {
                    StringToken paramNameToken = Eat(TokenType.ID) as StringToken;
                    parameters.Add(new WordAST(paramNameToken));
                    if (currentToken.Type!=TokenType.COMMA)
                    {
                        break;
                    }
                    else
                    {
                        Eat(TokenType.COMMA);
                    }
                }
                Eat(TokenType.RPAR);
            }
            var start = (OperatorToken) Eat(TokenType.COLON);
            while (currentToken.Type != TokenType.END)
            {
                Eat(currentToken.Type);
            }
            var end = (StringToken) Eat(TokenType.END);
            return new ScriptAST(parameters, start, end);
        }
        private ScriptFileAST ScriptFile()
        {
            // Eat(TokenType.SCRIPT);
            List<WordAST> fileName = new List<WordAST>();
            bool relative = false;
            if (currentToken.Type == TokenType.DOT)
            {
                relative = true;
                Eat(TokenType.DOT);
                while (currentToken.Type == TokenType.DOT)
                {
                    StringToken token = new StringToken(TokenType.ID, "..", currentToken.Line, currentToken.Column, file);
                    Eat(TokenType.DOT);
                    fileName.Add(new WordAST(token));
                }
            }
            while (true)
            {
                StringToken moduleToken = Eat(TokenType.ID) as StringToken;
                fileName.Add(new WordAST(moduleToken));
                if (currentToken.Type!=TokenType.DOT)
                {
                    break;
                }
                else
                {
                    Eat(TokenType.DOT);
                }
            }
            Eat(TokenType.SEMIC);
            return new ScriptFileAST(fileName, relative);
        }
        private UsingAST Using()
        {
            Eat(TokenType.USING);
            List<WordAST> fileName = new List<WordAST>();
            WordAST moduleName = null;
            bool relative = false;
            if (currentToken.Type == TokenType.DOT)
            {
                relative = true;
                Eat(TokenType.DOT);
                while (currentToken.Type == TokenType.DOT)
                {
                    StringToken token = new StringToken(TokenType.ID, "..", currentToken.Line, currentToken.Column, file);
                    Eat(TokenType.DOT);
                    fileName.Add(new WordAST(token));
                }
            }
            while (true)
            {
                StringToken moduleToken = Eat(TokenType.ID) as StringToken;
                fileName.Add(new WordAST(moduleToken));
                if (currentToken.Type!=TokenType.DOT)
                {
                    break;
                }
                else
                {
                    Eat(TokenType.DOT);
                }
            }
            if (currentToken.Type == TokenType.COLON)
            {
                Eat(TokenType.COLON);
                StringToken moduleToken = Eat(TokenType.ID) as StringToken;
                moduleName = new WordAST(moduleToken);
            }
            Eat(TokenType.SEMIC);
            return new UsingAST(fileName, moduleName, relative);
        }
        private ImportAST Import()
        {
            Eat(TokenType.IMPORT);
            List<WordAST> fileName = new List<WordAST>();
            List<WordAST> symbols = new List<WordAST>();
            bool relative = false;
            if (currentToken.Type == TokenType.DOT)
            {
                relative = true;
                Eat(TokenType.DOT);
                while (currentToken.Type == TokenType.DOT)
                {
                    StringToken token = new StringToken(TokenType.ID, "..", currentToken.Line, currentToken.Column, file);
                    Eat(TokenType.DOT);
                    fileName.Add(new WordAST(token));
                }
            }
            while (true)
            {
                StringToken moduleToken = Eat(TokenType.ID) as StringToken;
                WordAST moduleName = new WordAST(moduleToken);
                fileName.Add(moduleName);
                if (currentToken.Type!=TokenType.DOT)
                {
                    break;
                }
                else
                {
                    Eat(TokenType.DOT);
                }
            }
            if (currentToken.Type == TokenType.COLON)
            {
                Eat(TokenType.COLON);
                while (true)
                {
                    StringToken moduleToken = Eat(TokenType.ID) as StringToken;
                    WordAST symbol = new WordAST(moduleToken);
                    symbols.Add(symbol);
                    if (currentToken.Type!=TokenType.COMMA)
                    {
                        break;
                    }
                    else
                    {
                        Eat(TokenType.COMMA);
                    }
                }
            }
            Eat(TokenType.SEMIC);
            return new ImportAST(fileName, relative, symbols);
        }
    }
}