/* Copyright 2019-2020 Junruoyu Zheng. All rights reserved.

   Distributed under MIT license.
   See file LICENSE for detail or copy at https://opensource.org/licenses/MIT
*/

using System.Collections.Generic;
using Ligral.Syntax;

namespace Ligral.Syntax.ASTs
{
    class SelectAST : AST
    {
        public AST ModelObject;
        public PortAST Port;
        public SelectAST(AST model, PortAST port)
        {
            ModelObject = model;
            Port = port;
        }
        public override Token FindToken()
        {
            return Port.FindToken();
        }
    }
}