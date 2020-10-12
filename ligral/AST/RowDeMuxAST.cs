using System.Collections.Generic;

namespace Ligral.ASTs
{
    class RowDeMuxAST : AST
    {
        public List<AST> Items;
        public RowDeMuxAST(List<AST> items)
        {
            Items = items;
        }
        public override Token FindToken()
        {
            return Items[0].FindToken();
        }
    }
}