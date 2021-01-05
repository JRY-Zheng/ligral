/* Copyright 2019-2020 Junruoyu Zheng. All rights reserved.

   Distributed under MIT license.
   See file LICENSE for detail or copy at https://opensource.org/licenses/MIT
*/

using System.Collections.Generic;
using Ligral.Syntax;

namespace Ligral.Syntax.ASTs
{
    class ConfListAST : AST
    {
        public List<ConfAST> Confs;
        public ConfListAST(List<ConfAST> confs)
        {
            Confs = confs;
        }
        public override Token FindToken()
        {
            return Confs[0].FindToken();
        }
    }
}