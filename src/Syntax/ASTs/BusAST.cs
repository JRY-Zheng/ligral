/* Copyright 2019-2020 Junruoyu Zheng. All rights reserved.

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