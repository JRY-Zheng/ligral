/* Copyright (C) 2019-2021 Junruoyu Zheng. Home page: https://junruoyu-zheng.gitee.io/ligral

   Distributed under MIT license.
   See file LICENSE for detail or copy at https://opensource.org/licenses/MIT
*/

using System.Collections.Generic;
using Ligral.Syntax;

namespace Ligral.Syntax.ASTs
{
    class BinOpAST : AST
    {
        public AST Left;
        public AST Right;
        public CharToken Operator;
        public BinOpAST(AST left, CharToken op, AST right)
        {
            Left = left;
            Operator = op;
            Right = right;
        }
        public override Token FindToken()
        {
            return Operator;
        }
    }
}