/* Copyright (C) 2019-2021 Junruoyu Zheng. Home page: https://junruoyu-zheng.gitee.io/ligral

   Distributed under MIT license.
   See file LICENSE for detail or copy at https://opensource.org/licenses/MIT
*/

using System.Collections.Generic;
using Ligral.Syntax;

namespace Ligral.Syntax.ASTs
{
    class ConfAST : AST
    {
        public WordAST Id;
        public AST Expression;
        public bool Nested;
        public ConfAST(WordAST id, AST expression, bool nested)
        {
            Id = id;
            Expression = expression;
            Nested = nested;
        }
        public override Token FindToken()
        {
            return Id.FindToken();
        }
    }
}