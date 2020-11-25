using System.Collections.Generic;
using Ligral.Syntax;

namespace Ligral.Syntax.ASTs
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