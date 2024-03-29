/* Copyright (C) 2019-2022 Junruoyu Zheng. Home page: https://junruoyu-zheng.gitee.io/ligral

   Distributed under MIT license.
   See file LICENSE for detail or copy at https://opensource.org/licenses/MIT
*/

using System.Collections.Generic;
using Ligral.Syntax;

namespace Ligral.Syntax.ASTs
{
    class StatementsAST : AST
    {
        public List<AST> Statements;
        public StatementsAST(List<AST> statements)
        {
            Statements = statements;
        }
        public override Token FindToken()
        {
            return Statements[0].FindToken();
        }
    }
}