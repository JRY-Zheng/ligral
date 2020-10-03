using System.Collections.Generic;

namespace Ligral.ASTs
{
    class GotoOpAST : AST
    {
        public AST Source;
        public AST Destination;
        public GotoOpAST(AST source, AST destination)
        {
            Source = source;
            Destination = destination;
        }
        public override Token FindToken()
        {
            return Source.FindToken();
        }
    }
}