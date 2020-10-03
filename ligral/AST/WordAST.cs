using System.Collections.Generic;

namespace Ligral.ASTs
{
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
}