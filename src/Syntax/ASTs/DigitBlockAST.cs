using System.Collections.Generic;
using Ligral.Syntax;

namespace Ligral.Syntax.ASTs
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