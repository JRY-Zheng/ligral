using System.Collections.Generic;
using Ligral.Syntax;

namespace Ligral.Syntax.ASTs
{
    class DeclareAST : AST
    {
        public AST ModelType;
        public WordAST Id;
        public DeclareAST(AST modeType, WordAST id)
        {
            ModelType = modeType;
            Id = id;
        }
        public override Token FindToken()
        {
            return Id.FindToken();
        }
    }
}