using System.Collections.Generic;

namespace Ligral
{
    class Parser
    {
        private Lexer lexer;
        private Token currentToken;
        private Token currentTokenBackup;
        public Parser() {}
        public void Load(string text)
        {
            lexer = new Lexer();
            lexer.Load(text);
            currentToken = Feed();
        }
        public ProgramAST Parse()
        {
            return Program();
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
                throw new SyntaxException(currentToken, $"{tokenType.ToString()} expected");
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
        private ProgramAST Program()
        {
            StatementsAST statementsAST = Statements();
            return new ProgramAST("global", statementsAST);
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
            if (currentToken.Type==TokenType.CONF)
            {
                ConfAST confAST = ProgramConfig();
                Eat(TokenType.SEMIC);
                return confAST;
            }
            else if (currentToken.Type==TokenType.ASSIGN)
            {
                FromOpAST fromOpAST = Define();
                Eat(TokenType.SEMIC);
                return fromOpAST;
            }
            else if (currentToken.Type==TokenType.USING)
            {
                UsingAST usingAST = Using();
                Eat(TokenType.SEMIC);
                return usingAST;
            }
            else if (currentToken.Type==TokenType.IMPORT)
            {
                ImportAST importAST = Import();
                Eat(TokenType.SEMIC);
                return importAST;
            }
            else if (currentToken.Type==TokenType.ROUTE)
            {
                return Route();
            }
            else
            {
                ChainAST chainAST = Chain();
                Eat(TokenType.SEMIC);
                return chainAST;
            }
        }
        private ConfAST ProgramConfig()
        {
            Eat(TokenType.CONF);
            StringToken idToken = Eat(TokenType.ID) as StringToken;
            WordAST wordAST = new WordAST(idToken);
            Eat(TokenType.FROM);
            AST valueAST;
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
            
            return new ConfAST(wordAST, valueAST);
        }
        private FromOpAST Define()
        {
            Eat(TokenType.ASSIGN);
            StringToken idToken = Eat(TokenType.ID) as StringToken;
            WordAST wordAST = new WordAST(idToken);
            Eat(TokenType.FROM);
            AST valueExprAST = ValueExpr();
            return new FromOpAST(wordAST, valueExprAST);
        }
        private AST ValueExpr()
        {
            AST valueFactorAST = ValueFactor();
            while (currentToken.Type==TokenType.PLUS||currentToken.Type==TokenType.MINUS)
            {
                CharToken binOpToken = Eat(currentToken.Type) as CharToken;
                valueFactorAST = new ValBinOpAST(valueFactorAST, binOpToken, ValueFactor());
            }
            return valueFactorAST;
        }
        private AST ValueFactor()
        {
            AST valueAST = ValueEntity();
            while (currentToken.Type==TokenType.MUL||currentToken.Type==TokenType.DIV)
            {
                CharToken binOpToken = Eat(currentToken.Type) as CharToken;
                valueAST = new ValBinOpAST(valueAST, binOpToken, ValueEntity());
            }
            return valueAST;
        }
        private AST ValueEntity()
        {
            AST valueAST = Value();
            while (currentToken.Type==TokenType.CARET)
            {
                CharToken binOpToken = Eat(currentToken.Type) as CharToken;
                valueAST = new ValBinOpAST(valueAST, binOpToken, Value());
            }
            return valueAST;
        }
        private AST Value()
        {
            if (currentToken.Type==TokenType.PLUS||currentToken.Type==TokenType.MINUS)
            {
                CharToken unaryOpToken = Eat(currentToken.Type) as CharToken;
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
        private AST Matrix(bool isBlock=false)
        {
            List<RowAST> rows = new List<RowAST>();
            Eat(TokenType.LBRK);
            while (currentToken.Type!=TokenType.RBRK)
            {
                rows.Add(Row());
                if (currentToken.Type==TokenType.SEMIC)
                {
                    Eat(TokenType.SEMIC);
                }
            }
            Eat(TokenType.RBRK);
            if (isBlock)
            {
                return new MatrixBlockAST(rows);
            }
            else
            {
                return new MatrixAST(rows);
            }
        }
        private RowAST Row()
        {
            List<AST> items = new List<AST>();
            while (currentToken.Type!=TokenType.RBRK && currentToken.Type!=TokenType.SEMIC)
            {
                items.Add(ValueExpr());
                if (currentToken.Type==TokenType.COMMA)
                {
                    Eat(TokenType.COMMA);
                }
            }
            return new RowAST(items);
        }
        private AST Pointer()
        {
            // StringToken idToken = Eat(TokenType.ID) as StringToken;
            // StringToken scopeToken = null;
            // if (currentToken.Type==TokenType.DOT)
            // {
            //     Eat(TokenType.DOT);
            //     scopeToken = idToken;
            //     idToken = Eat(TokenType.ID) as StringToken;
            // }
            // return new PointerAST(new IdAST(scopeToken), new WordAST(idToken));
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
        private ChainAST Chain()
        {
            AST nodeExprAST = NodeExpr();
            while (currentToken.Type==TokenType.GOTO)
            {
                Eat(TokenType.GOTO);
                nodeExprAST = new GotoOpAST(nodeExprAST, NodeExpr());
            }
            return new ChainAST(nodeExprAST);
        }
        private AST NodeExpr()
        {
            AST nodeFactorAST = NodeFactor();
            while (currentToken.Type==TokenType.PLUS||currentToken.Type==TokenType.MINUS)
            {
                CharToken binOpToken = Eat(currentToken.Type) as CharToken;
                nodeFactorAST = new BinOpAST(nodeFactorAST, binOpToken, NodeFactor());
            }
            return nodeFactorAST;
        }
        private AST NodeFactor()
        {
            AST nodeAST = NodeEntity();
            while (currentToken.Type==TokenType.MUL||currentToken.Type==TokenType.DIV)
            {
                CharToken binOpToken = Eat(currentToken.Type) as CharToken;
                nodeAST = new BinOpAST(nodeAST, binOpToken, NodeEntity());
            }
            return nodeAST;
        }
        private AST NodeEntity()
        {
            AST nodeAST = Node();
            while (currentToken.Type==TokenType.CARET)
            {
                CharToken binOpToken = Eat(currentToken.Type) as CharToken;
                nodeAST = new BinOpAST(nodeAST, binOpToken, Node());
            }
            return nodeAST;
        }
        private AST Node()
        {
            if (currentToken.Type==TokenType.PLUS||currentToken.Type==TokenType.MINUS)
            {
                CharToken unaryOpToken = Eat(currentToken.Type) as CharToken;
                return new UnaryOpAST(unaryOpToken, Node());
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
            else
            {
                return Selector();
            }
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
            WordAST portNameAST = null;
            if (currentToken.Type==TokenType.COLON)
            {
                Eat(TokenType.COLON);
                StringToken portIdToken = Eat(TokenType.ID) as StringToken;
                portNameAST = new WordAST(portIdToken);
                blockAST = new SelectAST(blockAST, portNameAST);
            }
            return blockAST;
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
            if (currentToken.Type==TokenType.FROM)
            {
                Eat(TokenType.FROM);
                return new RouteParamAST(nameAST, typeAST, ValueExpr());
            }
            else
            {
                return new RouteParamAST(nameAST, typeAST);
            }
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
        private UsingAST Using()
        {
            Eat(TokenType.USING);
            List<WordAST> fileName = new List<WordAST>();
            WordAST moduleName;
            while (true)
            {
                StringToken moduleToken = Eat(TokenType.ID) as StringToken;
                moduleName = new WordAST(moduleToken);
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
            return new UsingAST(fileName, moduleName);
        }
        private ImportAST Import()
        {
            Eat(TokenType.IMPORT);
            List<WordAST> fileName = new List<WordAST>();
            WordAST moduleName;
            while (true)
            {
                StringToken moduleToken = Eat(TokenType.ID) as StringToken;
                moduleName = new WordAST(moduleToken);
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
            return new ImportAST(fileName);
        }
    }
}