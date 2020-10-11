using System.Collections.Generic;

namespace Ligral.ASTs
{
    class MatrixDeMuxAST : AST
    {
        public List<RowBlockAST> Rows;
        public int RowNumber;
        public int ColNumber;
        public MatrixDeMuxAST(List<RowBlockAST> rows, int colNumber)
        {
            Rows = rows;
            RowNumber = rows.Count;
            ColNumber = colNumber;
        }
        public override Token FindToken()
        {
            return Rows[0].FindToken();
        }
    }
}