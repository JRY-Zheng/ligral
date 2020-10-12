using System.Collections.Generic;

namespace Ligral.ASTs
{
    class UnaryOpAST : AST
    {
        public AST Value;
        public CharToken Operator;
        public UnaryOpAST(CharToken op, AST value)
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