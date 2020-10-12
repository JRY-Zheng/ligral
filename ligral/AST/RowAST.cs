using System.Collections.Generic;

namespace Ligral.ASTs
{
    class RowAST : AST
    {
        public List<AST> Items;
        public RowAST(List<AST> items)
        {
            Items = items;
        }
        public override Token FindToken()
        {
            return Items[0].FindToken();
        }
    }
}