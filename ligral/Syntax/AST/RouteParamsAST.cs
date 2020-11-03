using System.Collections.Generic;
using Ligral.Syntax;

namespace Ligral.Syntax.ASTs
{
    class RouteParamsAST : AST
    {
        public List<RouteParamAST> Parameters;
        public RouteParamsAST(List<RouteParamAST> parameters)
        {
            Parameters = parameters;
        }
        public override Token FindToken()
        {
            return Parameters[0].FindToken();
        }
    }
}