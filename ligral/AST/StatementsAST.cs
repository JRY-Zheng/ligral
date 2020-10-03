using System.Collections.Generic;

namespace Ligral.ASTs
{
    class StatementsAST : AST
    {
        public List<AST> Statements;
        public StatementsAST(List<AST> statements)
        {
            Statements = statements;
        }
        public override Token FindToken()
        {
            return Statements[0].FindToken();
        }
    }
}