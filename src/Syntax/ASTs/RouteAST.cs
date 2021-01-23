/* Copyright (C) 2019-2021 Junruoyu Zheng. Home page: https://junruoyu-zheng.gitee.io/ligral

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
        public RouteInPortAST InPorts;
        public RoutePortAST OutPorts;
        public StatementsAST Statements;
        public RouteAST(AST definition, RouteParamsAST parameters, RouteInPortAST inPorts, RoutePortAST outPorts, StatementsAST statements)
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