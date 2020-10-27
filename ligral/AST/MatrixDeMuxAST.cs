using System.Collections.Generic;
using Ligral.Syntax;

namespace Ligral.ASTs
{
    class MatrixDeMuxAST : AST
    {
        public List<RowDeMuxAST> Rows;
        public int RowNumber;
        public int ColNumber;
        public MatrixDeMuxAST(List<RowDeMuxAST> rows, int colNumber)
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