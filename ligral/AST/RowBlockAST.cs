using System.Collections.Generic;

namespace Ligral.ASTs
{
    class RowBlockAST : AST
    {
        public List<AST> Items;
        public RowBlockAST(List<AST> items)
        {
            Items = items;
        }
        public override Token FindToken()
        {
            return Items[0].FindToken();
        }
    }
}