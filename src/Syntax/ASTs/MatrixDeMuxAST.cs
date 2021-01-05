/* Copyright 2019-2020 Junruoyu Zheng. All rights reserved.

   Distributed under MIT license.
   See file LICENSE for detail or copy at https://opensource.org/licenses/MIT
*/

using System.Collections.Generic;
using Ligral.Syntax;

namespace Ligral.Syntax.ASTs
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