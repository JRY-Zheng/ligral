using System.Collections.Generic;
using Ligral.Syntax;

namespace Ligral.ASTs
{
    class ConfAST : AST
    {
        public WordAST Id;
        public AST Expression;
        public ConfAST(WordAST id, AST expression)
        {
            Id = id;
            Expression = expression;
        }
        public override Token FindToken()
        {
            return Id.FindToken();
        }
    }
}