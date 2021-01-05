/* Copyright 2019-2020 Junruoyu Zheng. All rights reserved.

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