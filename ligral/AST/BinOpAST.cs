using System.Collections.Generic;
using Ligral.Syntax;

namespace Ligral.ASTs
{
    class BinOpAST : AST
    {
        public AST Left;
        public AST Right;
        public CharToken Operator;
        public BinOpAST(AST left, CharToken op, AST right)
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