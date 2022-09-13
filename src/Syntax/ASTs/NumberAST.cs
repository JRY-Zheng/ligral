/* Copyright (C) 2019-2022 Junruoyu Zheng. Home page: https://junruoyu-zheng.gitee.io/ligral

   Distributed under MIT license.
   See file LICENSE for detail or copy at https://opensource.org/licenses/MIT
*/

using System.Collections.Generic;
using Ligral.Syntax;
using Ligral.Component;

namespace Ligral.Syntax.ASTs
{
    class NumberAST : AST
    {
        public DigitToken ReferenceToken;
        public int Number;
        public NumberAST(DigitToken token)
        {
            ReferenceToken = token;
            Number = token.Value.ToInt();
        }
        public override Token FindToken()
        {
            return ReferenceToken;
        }
    }
}