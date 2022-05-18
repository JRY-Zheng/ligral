/* Copyright (C) 2019-2022 Junruoyu Zheng. Home page: https://junruoyu-zheng.gitee.io/ligral

   Distributed under MIT license.
   See file LICENSE for detail or copy at https://opensource.org/licenses/MIT
*/

using System.Collections.Generic;
using Ligral.Syntax;

namespace Ligral.Syntax.ASTs
{
    class StringAST : AST
    {
        public StringToken ReferenceToken;
        public string String;
        public StringAST(StringToken token)
        {
            ReferenceToken = token;
            String = (string)token.Value;
        }
        public override Token FindToken()
        {
            return ReferenceToken;
        }
    }
}