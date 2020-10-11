using System.Collections.Generic;

namespace Ligral.ASTs
{
    class MatrixDeMuxAST : AST
    {
        public List<RowAST> Rows;
        public MatrixDeMuxAST(List<RowAST> rows)
        {
            Rows = rows;
        }
        public override Token FindToken()
        {
            return Rows[0].FindToken();
        }
    }
}