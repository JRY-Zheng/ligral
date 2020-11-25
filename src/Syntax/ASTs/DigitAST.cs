using System.Collections.Generic;
using Ligral.Syntax;

namespace Ligral.Syntax.ASTs
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