/* Copyright (C) 2019-2021 Junruoyu Zheng. Home page: https://junruoyu-zheng.gitee.io/ligral

   Distributed under MIT license.
   See file LICENSE for detail or copy at https://opensource.org/licenses/MIT
*/

using System.Collections.Generic;
using Ligral.Syntax;

namespace Ligral.Syntax.ASTs
{
    class RoutePortAST : AST
    {
        public List<WordAST> Ports;
        public RoutePortAST(List<WordAST> ports)
        {
            Ports = ports;
        }
        public override Token FindToken()
        {
            return Ports[0].FindToken();
        }
    }
}