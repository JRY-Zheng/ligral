/* Copyright (C) 2019-2022 Junruoyu Zheng. Home page: https://junruoyu-zheng.gitee.io/ligral

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