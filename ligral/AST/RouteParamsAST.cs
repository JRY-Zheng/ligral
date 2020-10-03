using System.Collections.Generic;

namespace Ligral.ASTs
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