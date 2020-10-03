using System.Collections.Generic;

namespace Ligral.ASTs
{
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
}