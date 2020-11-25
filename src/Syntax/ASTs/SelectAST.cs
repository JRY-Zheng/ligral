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