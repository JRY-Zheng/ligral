using System.Collections.Generic;

namespace Ligral.ASTs
{
    class MatrixMuxAST : AST
    {
        public List<RowMuxAST> Rows;
        public MatrixMuxAST(List<RowMuxAST> rows)
        {
            Rows = rows;
        }
        public override Token FindToken()
        {
            return Rows[0].FindToken();
        }
    }
}