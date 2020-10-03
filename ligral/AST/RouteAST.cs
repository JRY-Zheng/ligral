using System.Collections.Generic;

namespace Ligral.ASTs
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