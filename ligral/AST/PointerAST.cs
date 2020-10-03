using System.Collections.Generic;

namespace Ligral.ASTs
{
    class PointerAST : AST
    {
        public AST ScopeName;
        public IdAST Member;
        public PointerAST(AST scope, IdAST member)
        {
            ScopeName = scope;
            Member = member;
        }
        public override Token FindToken()
        {
            return ScopeName.FindToken();
        }
    }
}