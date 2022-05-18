/* Copyright (C) 2019-2022 Junruoyu Zheng. Home page: https://junruoyu-zheng.gitee.io/ligral

   Distributed under MIT license.
   See file LICENSE for detail or copy at https://opensource.org/licenses/MIT
*/

using System.Collections.Generic;
using Ligral.Syntax;

namespace Ligral.Syntax.ASTs
{
    class KeyValuePairAST : AST
    {
        public WordAST Key;
        public AST Value;
        public KeyValuePairAST(WordAST key, AST value)
        {
            Key = key;
            Value = value;
        }
        public override Token FindToken()
        {
            return Key.FindToken();
        }
    }
}