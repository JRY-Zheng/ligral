using System.Collections.Generic;
using Ligral.Syntax;

namespace Ligral.Syntax.ASTs
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