/* Copyright (C) 2019-2020 Junruoyu Zheng. Home page: https://junruoyu-zheng.gitee.io/ligral

   Distributed under MIT license.
   See file LICENSE for detail or copy at https://opensource.org/licenses/MIT
*/

using System.Collections.Generic;
using Ligral.Syntax;

namespace Ligral.Syntax.ASTs
{
    class FromOpAST : AST
    {
        public WordAST Id;
        public AST Expression;
        public FromOpAST(WordAST id, AST expression)
        {
            Id = id;
            Expression = expression;
        }
        public override Token FindToken()
        {
            return Id.FindToken();
        }
    }
}