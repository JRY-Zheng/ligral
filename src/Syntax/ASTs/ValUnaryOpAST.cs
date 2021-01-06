/* Copyright (C) 2019-2021 Junruoyu Zheng. Home page: https://junruoyu-zheng.gitee.io/ligral

   Distributed under MIT license.
   See file LICENSE for detail or copy at https://opensource.org/licenses/MIT
*/

using System.Collections.Generic;
using Ligral.Syntax;

namespace Ligral.Syntax.ASTs
{
    class ValUnaryOpAST : AST
    {
        public AST Value;
        public OperantToken Operator;
        public ValUnaryOpAST(OperantToken op, AST value)
        {
            Operator = op;
            Value = value;
        }
        public override Token FindToken()
        {
            return Operator;
        }
    }
}