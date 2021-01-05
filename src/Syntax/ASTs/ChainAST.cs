/* Copyright 2019-2020 Junruoyu Zheng. All rights reserved.

   Distributed under MIT license.
   See file LICENSE for detail or copy at https://opensource.org/licenses/MIT
*/

using System.Collections.Generic;
using Ligral.Syntax;

namespace Ligral.Syntax.ASTs
{
    class ChainAST : AST
    {
        public AST Tree;
        public ChainAST(AST tree)
        {
            Tree = tree;
        }
        public override Token FindToken()
        {
            return Tree.FindToken();
        }
    }
}