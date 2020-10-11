using System.Collections.Generic;

namespace Ligral.ASTs
{
    class MatrixMuxAST : AST
    {
        public List<RowAST> Rows;
        public MatrixMuxAST(List<RowAST> rows)
        {
            Rows = rows;
        }
        public override Token FindToken()
        {
            return Rows[0].FindToken();
        }
    }
}