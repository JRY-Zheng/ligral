using System.Collections.Generic;
using Ligral.Syntax;

namespace Ligral.ASTs
{
    class ValUnaryOpAST : AST
    {
        public AST Value;
        public CharToken Operator;
        public ValUnaryOpAST(CharToken op, AST value)
        {
            Operator = op;
            Value = value;
        }
        public override Token FindToken()
        {
            return Operator;
        }
    }
}