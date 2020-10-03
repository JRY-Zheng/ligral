using System.Collections.Generic;

namespace Ligral.ASTs
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