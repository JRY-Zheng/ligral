using System.Collections.Generic;

namespace Ligral.Syntax.ASTs
{
    class SignatureAST : AST
    {
        public RoutePortAST InPorts;
        public RoutePortAST OutPorts;
        public WordAST TypeName;
        public SignatureAST(WordAST signatureName, RoutePortAST inPorts, RoutePortAST outPorts)
        {
            TypeName = signatureName;
            InPorts = inPorts;
            OutPorts = outPorts;
        }
        public override Token FindToken()
        {
            return TypeName.FindToken();
        }
    }
}