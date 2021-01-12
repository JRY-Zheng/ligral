/* Copyright (C) 2019-2021 Junruoyu Zheng. Home page: https://junruoyu-zheng.gitee.io/ligral

   Distributed under MIT license.
   See file LICENSE for detail or copy at https://opensource.org/licenses/MIT
*/

using System.Collections.Generic;
using Ligral.Syntax;

namespace Ligral.Syntax.ASTs
{
    class GotoOpAST : AST
    {
        public AST Source;
        public AST Destination;
        public OperantToken GotoToken;
        public GotoOpAST(AST source, AST destination, OperantToken gotoToken)
        {
            Source = source;
            Destination = destination;
            GotoToken = gotoToken;
        }
        public override Token FindToken()
        {
            return GotoToken;
        }
    }
}