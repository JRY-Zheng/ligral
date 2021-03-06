/* Copyright (C) 2019-2021 Junruoyu Zheng. Home page: https://junruoyu-zheng.gitee.io/ligral

   Distributed under MIT license.
   See file LICENSE for detail or copy at https://opensource.org/licenses/MIT
*/

using System.Collections.Generic;
using Ligral.Syntax;

namespace Ligral.Syntax.ASTs
{
    class BoolAST : AST
    {
        public BoolToken ReferenceToken;
        public bool Bool;
        public BoolAST(BoolToken token)
        {
            ReferenceToken = token;
            Bool = (bool) token.Value;
        }
        public override Token FindToken()
        {
            return ReferenceToken;
        }
    }
}