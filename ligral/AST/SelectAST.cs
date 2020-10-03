using System.Collections.Generic;

namespace Ligral.ASTs
{
    class SelectAST : AST
    {
        public AST ModelObject;
        public WordAST PortName;
        public SelectAST(AST model, WordAST port)
        {
            ModelObject = model;
            PortName = port;
        }
        public override Token FindToken()
        {
            return PortName.FindToken();
        }
    }
}