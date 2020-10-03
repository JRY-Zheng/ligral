using System.Collections.Generic;

namespace Ligral.ASTs
{
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
}