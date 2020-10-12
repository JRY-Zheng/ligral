using System.Collections.Generic;

namespace Ligral.ASTs
{
    class RowMuxAST : AST
    {
        public List<AST> Items;
        public RowMuxAST(List<AST> items)
        {
            Items = items;
        }
        public override Token FindToken()
        {
            return Items[0].FindToken();
        }
    }
}