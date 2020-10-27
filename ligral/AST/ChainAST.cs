using System.Collections.Generic;
using Ligral.Syntax;

namespace Ligral.ASTs
{
    class ChainAST : AST
    {
        public AST Tree;
        public ChainAST(AST tree)
        {
            Tree = tree;
        }
        public override Token FindToken()
        {
            return Tree.FindToken();
        }
    }
}