using System.Collections.Generic;

namespace Ligral
{
    class AST 
    {
        public virtual Token FindToken()
        {
            return null;
        }
    }

    class ProgramAST : AST
    {
        public string Name;
        public StatementsAST Statements;
        public ProgramAST(string name, StatementsAST statements)
        {
            Name = name;
            Statements = statements;
        }
        public override Token FindToken()
        {
            return Statements.FindToken();
        }
    }

    class StatementsAST : AST
    {
        public List<AST> Statements;
        public StatementsAST(List<AST> statements)
        {
            Statements = statements;
        }
        public override Token FindToken()
        {
            return Statements[0].FindToken();
        }
    }

    class IdAST : AST
    {
        public StringToken ReferenceToken;
        public string Id;
        public IdAST(StringToken token)
        {
            ReferenceToken = token;
            if (token!=null)
            {
                Id = (string)token.Value;
            }
            else
            {
                Id = null;
            }
        }
        public override Token FindToken()
        {
            return ReferenceToken;
        }
    }

    class WordAST : AST
    {
        public StringToken ReferenceToken;
        public string Word;
        public WordAST(StringToken token)
        {
            ReferenceToken = token;
            if (token!=null)
            {
                Word = (string)token.Value;
            }
            else
            {
                Word = null;
            }
        }
        public override Token FindToken()
        {
            return ReferenceToken;
        }
    }

    class BoolAST : AST
    {
        public BoolToken ReferenceToken;
        public bool Bool;
        public BoolAST(BoolToken token)
        {
            ReferenceToken = token;
            Bool = (bool) token.Value;
        }
        public override Token FindToken()
        {
            return ReferenceToken;
        }
    }

    class DigitAST : AST
    {
        public DigitToken ReferenceToken;
        public double Digit;
        public DigitAST(DigitToken token)
        {
            ReferenceToken = token;
            Digit = (double)token.Value;
        }
        public override Token FindToken()
        {
            return ReferenceToken;
        }
    }

    class DigitBlockAST : AST
    {
        public DigitToken ReferenceToken;
        public double Digit;
        public DigitBlockAST(DigitToken token)
        {
            ReferenceToken = token;
            Digit = (double)token.Value;
        }
        public override Token FindToken()
        {
            return ReferenceToken;
        }
    }

    class StringAST : AST
    {
        public StringToken ReferenceToken;
        public string String;
        public StringAST(StringToken token)
        {
            ReferenceToken = token;
            String = (string)token.Value;
        }
        public override Token FindToken()
        {
            return ReferenceToken;
        }
    }

    class BusAST : AST
    {
        public List<ChainAST> Chains;
        public BusAST(List<ChainAST> chains)
        {
            Chains = chains;
        }
        public override Token FindToken()
        {
            return Chains[0].FindToken();
        }
    }

    class DictAST : AST
    {
        public List<KeyValuePairAST> Parameters;
        public DictAST(List<KeyValuePairAST> parameters)
        {
            Parameters = parameters;
        }
        public override Token FindToken()
        {
            return Parameters[0].FindToken();
        }
    }

    class KeyValuePairAST : AST
    {
        public WordAST Key;
        public AST Value;
        public KeyValuePairAST(WordAST key, AST value)
        {
            Key = key;
            Value = value;
        }
        public override Token FindToken()
        {
            return Key.FindToken();
        }
    }

    class DeclareAST : AST
    {
        public AST ModelType;
        public WordAST Id;
        public DeclareAST(AST modeType, WordAST id)
        {
            ModelType = modeType;
            Id = id;
        }
        public override Token FindToken()
        {
            return Id.FindToken();
        }
    }

    class ConfigureAST : AST
    {
        public AST Model;
        public DictAST ModelParameters;
        public ConfigureAST(AST model, DictAST modelParameters)
        {
            Model = model;
            ModelParameters = modelParameters;
        }
        public override Token FindToken()
        {
            return Model.FindToken();
        }
    }

    class BinOpAST : AST
    {
        public AST Left;
        public AST Right;
        public CharToken Operator;
        public BinOpAST(AST left, CharToken op, AST right)
        {
            Left = left;
            Operator = op;
            Right = right;
        }
        public override Token FindToken()
        {
            return Operator;
        }
    }

    class UnaryOpAST : AST
    {
        public AST Value;
        public CharToken Operator;
        public UnaryOpAST(CharToken op, AST value)
        {
            Operator = op;
            Value = value;
        }
        public override Token FindToken()
        {
            return Operator;
        }
    }

    class ValBinOpAST : AST
    {
        public AST Left;
        public AST Right;
        public CharToken Operator;
        public ValBinOpAST(AST left, CharToken op, AST right)
        {
            Left = left;
            Operator = op;
            Right = right;
        }
        public override Token FindToken()
        {
            return Operator;
        }
    }

    class ValUnaryOpAST : AST
    {
        public AST Value;
        public CharToken Operator;
        public ValUnaryOpAST(CharToken op, AST value)
        {
            Operator = op;
            Value = value;
        }
        public override Token FindToken()
        {
            return Operator;
        }
    }

    class ConfAST : AST
    {
        public WordAST Id;
        public AST Expression;
        public ConfAST(WordAST id, AST expression)
        {
            Id = id;
            Expression = expression;
        }
        public override Token FindToken()
        {
            return Id.FindToken();
        }
    }

    class FromOpAST : AST
    {
        public WordAST Id;
        public AST Expression;
        public FromOpAST(WordAST id, AST expression)
        {
            Id = id;
            Expression = expression;
        }
        public override Token FindToken()
        {
            return Id.FindToken();
        }
    }

    class GotoOpAST : AST
    {
        public AST Source;
        public AST Destination;
        public GotoOpAST(AST source, AST destination)
        {
            Source = source;
            Destination = destination;
        }
        public override Token FindToken()
        {
            return Source.FindToken();
        }
    }

    class ImportAST : AST
    {
        public List<WordAST> FileName;
        public ImportAST(List<WordAST> fileName)
        {
            FileName = fileName;
        }
        public override Token FindToken()
        {
            return FileName[0].FindToken();
        }
    }

    class UsingAST : AST
    {
        public List<WordAST> FileName;
        public WordAST ModuleName;
        public UsingAST(List<WordAST> fileName, WordAST moduleName)
        {
            FileName = fileName;
            ModuleName = moduleName;
        }
        public override Token FindToken()
        {
            return FileName[0].FindToken();
        }
    }

    class PointerAST : AST
    {
        public AST ScopeName;
        public IdAST Member;
        public PointerAST(AST scope, IdAST member)
        {
            ScopeName = scope;
            Member = member;
        }
        public override Token FindToken()
        {
            return ScopeName.FindToken();
        }
    }

    class SelectAST : AST
    {
        public AST ModelObject;
        public WordAST PortName;
        public SelectAST(AST model, WordAST port)
        {
            ModelObject = model;
            PortName = port;
        }
        public override Token FindToken()
        {
            return PortName.FindToken();
        }
    }

    class ChainAST : AST
    {
        public AST Tree;
        public ChainAST(AST tree)
        {
            Tree = tree;
        }
        public override Token FindToken()
        {
            return Tree.FindToken();
        }
    }

    class InheritAST : AST
    {
        public WordAST Name;
        public WordAST Type;
        public InheritAST(WordAST name, WordAST type = null)
        {
            Name = name;
            Type = type;
        }
        public override Token FindToken()
        {
            return Name.FindToken();
        }
    }

    class RouteParamAST : AST
    {
        public WordAST Name;
        public WordAST Type;
        public AST DefaultValue;
        public RouteParamAST(WordAST name, WordAST type = null, AST defaultValue = null)
        {
            Name = name;
            Type = type;
            DefaultValue = defaultValue;
        }
        public override Token FindToken()
        {
            return Name.FindToken();
        }
    }

    class RouteParamsAST : AST
    {
        public List<RouteParamAST> Parameters;
        public RouteParamsAST(List<RouteParamAST> parameters)
        {
            Parameters = parameters;
        }
        public override Token FindToken()
        {
            return Parameters[0].FindToken();
        }
    }

    class RoutePortAST : AST
    {
        public List<WordAST> Ports;
        public RoutePortAST(List<WordAST> ports)
        {
            Ports = ports;
        }
        public override Token FindToken()
        {
            return Ports[0].FindToken();
        }
    }

    class RouteAST : AST
    {
        public AST Definition;
        public RouteParamsAST Parameters;
        public RoutePortAST InPorts;
        public RoutePortAST OutPorts;
        public StatementsAST Statements;
        public RouteAST(AST definition, RouteParamsAST parameters, RoutePortAST inPorts, RoutePortAST outPorts, StatementsAST statements)
        {
            Definition = definition;
            Parameters = parameters;
            InPorts = inPorts;
            OutPorts = outPorts;
            Statements = statements;
        }
        public override Token FindToken()
        {
            return Definition.FindToken();
        }
    }
}