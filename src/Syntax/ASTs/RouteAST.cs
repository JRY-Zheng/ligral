/* Copyright 2019-2020 Junruoyu Zheng. All rights reserved.

   Distributed under MIT license.
   See file LICENSE for detail or copy at https://opensource.org/licenses/MIT
*/

using System.Collections.Generic;
using Ligral.Syntax;

namespace Ligral.Syntax.ASTs
{
    class RouteAST : AST
    {
        public AST Definition;
        public RouteParamsAST Parameters;
        public RoutePortAST InPorts;
        public RoutePortAST OutPorts;
        public StatementsAST Statements;
        public RouteAST(AST definition, RouteParamsAST parameters, RoutePortAST inPorts, RoutePortAST outPorts, StatementsAST statements)
        {
            Definition = definition;
            Parameters = parameters;
            InPorts = inPorts;
            OutPorts = outPorts;
            Statements = statements;
        }
        public override Token FindToken()
        {
            return Definition.FindToken();
        }
    }
}