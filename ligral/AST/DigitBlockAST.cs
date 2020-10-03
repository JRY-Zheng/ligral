using System.Collections.Generic;

namespace Ligral.ASTs
{
    class DigitBlockAST : AST
    {
        public DigitToken ReferenceToken;
        public double Digit;
        public DigitBlockAST(DigitToken token)
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