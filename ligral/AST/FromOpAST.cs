using System.Collections.Generic;

namespace Ligral.ASTs
{
    class FromOpAST : AST
    {
        public WordAST Id;
        public AST Expression;
        public FromOpAST(WordAST id, AST expression)
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