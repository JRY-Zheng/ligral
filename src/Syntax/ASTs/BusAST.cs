/* Copyright (C) 2019-2022 Junruoyu Zheng. Home page: https://junruoyu-zheng.gitee.io/ligral

   Distributed under MIT license.
   See file LICENSE for detail or copy at https://opensource.org/licenses/MIT
*/

using System.Collections.Generic;
using Ligral.Syntax;

namespace Ligral.Syntax.ASTs
{
    class BusAST : AST
    {
        public List<ChainAST> Chains;
        public BusAST(List<ChainAST> chains)
        {
            Chains = chains;
        }
        public override Token FindToken()
        {
            return Chains[0].FindToken();
        }
    }
}