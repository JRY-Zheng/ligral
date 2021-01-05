/* Copyright (C) 2019-2021 Junruoyu Zheng. Home page: https://junruoyu-zheng.gitee.io/ligral

   Distributed under MIT license.
   See file LICENSE for detail or copy at https://opensource.org/licenses/MIT
*/

using System.Collections.Generic;
using Ligral.Syntax;

namespace Ligral.Syntax.ASTs
{
    class DigitAST : AST
    {
        public DigitToken ReferenceToken;
        public double Digit;
        public DigitAST(DigitToken token)
        {
            ReferenceToken = token;
            Digit = (double)token.Value;
        }
        public override Token FindToken()
        {
            return ReferenceToken;
        }
    }
}