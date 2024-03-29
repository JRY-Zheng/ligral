/* Copyright (C) 2019-2022 Junruoyu Zheng. Home page: https://junruoyu-zheng.gitee.io/ligral

   Distributed under MIT license.
   See file LICENSE for detail or copy at https://opensource.org/licenses/MIT
*/

using System.Collections.Generic;
using Ligral.Syntax;

namespace Ligral.Syntax.ASTs
{
    class IdAST : AST
    {
        public StringToken ReferenceToken;
        public string Id;
        public IdAST(StringToken token)
        {
            ReferenceToken = token;
            if (token!=null)
            {
                Id = (string)token.Value;
            }
            else
            {
                Id = null;
            }
        }
        public override Token FindToken()
        {
            return ReferenceToken;
        }
    }
}