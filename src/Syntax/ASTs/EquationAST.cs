/* Copyright (C) 2019-2022 Junruoyu Zheng. Home page: https://junruoyu-zheng.gitee.io/ligral

   Distributed under MIT license.
   See file LICENSE for detail or copy at https://opensource.org/licenses/MIT
*/

using System.Collections.Generic;
using Ligral.Syntax;

namespace Ligral.Syntax.ASTs
{
    class EquationAST : AST
    {
        public ChainAST LeftChain;
        public ChainAST RightChain;
        public OperatorToken EqualToOperator;
        public EquationAST(ChainAST leftChain, OperatorToken equalToOperator, ChainAST rightChain)
        {
            LeftChain = leftChain;
            EqualToOperator = equalToOperator;
            RightChain = rightChain;
        }
        public override Token FindToken()
        {
            return EqualToOperator;
        }
    }
}