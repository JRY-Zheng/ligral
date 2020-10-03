using System.Collections.Generic;

namespace Ligral.ASTs
{
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
}