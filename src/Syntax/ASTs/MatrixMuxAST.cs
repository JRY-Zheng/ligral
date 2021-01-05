/* Copyright (C) 2019-2021 Junruoyu Zheng. Home page: https://junruoyu-zheng.gitee.io/ligral

   Distributed under MIT license.
   See file LICENSE for detail or copy at https://opensource.org/licenses/MIT
*/

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