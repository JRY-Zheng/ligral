/* Copyright (C) 2019-2022 Junruoyu Zheng. Home page: https://junruoyu-zheng.gitee.io/ligral

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
        public enum PortTypes
        {
            IN, OUT, EITHER
        }
        public PortTypes PortType; 
        public SelectAST(AST model, PortAST port)
        {
            ModelObject = model;
            Port = port;
            PortType = PortTypes.EITHER;
        }
        public SelectAST(AST model, PortAST port, PortTypes portType)
        {
            ModelObject = model;
            Port = port;
            PortType = portType;
        }
        public override Token FindToken()
        {
            return Port.FindToken();
        }
    }
}