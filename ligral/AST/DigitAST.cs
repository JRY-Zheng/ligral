using System.Collections.Generic;

namespace Ligral.ASTs
{
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
}