using System.Collections.Generic;

namespace Ligral.ASTs
{
    class MatrixBlockAST : AST
    {
        public List<RowAST> Rows;
        public MatrixBlockAST(List<RowAST> rows)
        {
            Rows = rows;
        }
        public override Token FindToken()
        {
            return Rows[0].FindToken();
        }
    }
}