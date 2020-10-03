using System.Collections.Generic;

namespace Ligral.ASTs
{
    class ValBinOpAST : AST
    {
        public AST Left;
        public AST Right;
        public CharToken Operator;
        public ValBinOpAST(AST left, CharToken op, AST right)
        {
            Left = left;
            Operator = op;
            Right = right;
        }
        public override Token FindToken()
        {
            return Operator;
        }
    }
}