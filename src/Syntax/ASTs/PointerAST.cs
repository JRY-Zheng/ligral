/* Copyright (C) 2019-2021 Junruoyu Zheng. Home page: https://junruoyu-zheng.gitee.io/ligral

   Distributed under MIT license.
   See file LICENSE for detail or copy at https://opensource.org/licenses/MIT
*/

using System.Collections.Generic;
using Ligral.Syntax;

namespace Ligral.Syntax.ASTs
{
    class PointerAST : AST
    {
        public AST ScopeName;
        public IdAST Member;
        public PointerAST(AST scope, IdAST member)
        {
            ScopeName = scope;
            Member = member;
        }
        public override Token FindToken()
        {
            return ScopeName.FindToken();
        }
    }
}