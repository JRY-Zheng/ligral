using System.Collections.Generic;

namespace Ligral.ASTs
{
    class ProgramAST : AST
    {
        public string Name;
        public StatementsAST Statements;
        public ProgramAST(string name, StatementsAST statements)
        {
            Name = name;
            Statements = statements;
        }
        public override Token FindToken()
        {
            return Statements.FindToken();
        }
    }
}