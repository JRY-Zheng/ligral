using System.Collections.Generic;

namespace Ligral.ASTs
{
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
}