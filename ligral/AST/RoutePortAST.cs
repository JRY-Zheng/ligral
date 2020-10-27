using System.Collections.Generic;
using Ligral.Syntax;

namespace Ligral.ASTs
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