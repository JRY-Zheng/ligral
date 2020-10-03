using System.Collections.Generic;

namespace Ligral.ASTs
{
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
}