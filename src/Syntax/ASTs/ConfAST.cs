using System.Collections.Generic;
using Ligral.Syntax;

namespace Ligral.Syntax.ASTs
{
    class ConfAST : AST
    {
        public WordAST Id;
        public AST Expression;
        public bool Nested;
        public ConfAST(WordAST id, AST expression, bool nested)
        {
            Id = id;
            Expression = expression;
            Nested = nested;
        }
        public override Token FindToken()
        {
            return Id.FindToken();
        }
    }
}