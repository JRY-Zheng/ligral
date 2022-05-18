/* Copyright (C) 2019-2022 Junruoyu Zheng. Home page: https://junruoyu-zheng.gitee.io/ligral

   Distributed under MIT license.
   See file LICENSE for detail or copy at https://opensource.org/licenses/MIT
*/

using System.Collections.Generic;
using Ligral.Syntax;

namespace Ligral.Syntax.ASTs
{
    class WordAST : AST
    {
        public StringToken ReferenceToken;
        public string Word;
        public WordAST(StringToken token)
        {
            ReferenceToken = token;
            if (token!=null)
            {
                Word = (string)token.Value;
            }
            else
            {
                Word = null;
            }
        }
        public override Token FindToken()
        {
            return ReferenceToken;
        }
    }
}