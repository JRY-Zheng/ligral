using System.Collections.Generic;

namespace Ligral.ASTs
{
    class MatrixMuxAST : AST
    {
        public List<RowBlockAST> Rows;
        public MatrixMuxAST(List<RowBlockAST> rows)
        {
            Rows = rows;
        }
        public override Token FindToken()
        {
            return Rows[0].FindToken();
        }
    }
}