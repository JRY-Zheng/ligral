using System.Collections.Generic;

namespace Ligral.ASTs
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