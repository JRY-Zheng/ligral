using System.Collections.Generic;
using Ligral.Syntax;

namespace Ligral.Syntax.ASTs
{
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
}