using System.Collections.Generic;

namespace Ligral.ASTs
{
    class ConfigureAST : AST
    {
        public AST Model;
        public DictAST ModelParameters;
        public ConfigureAST(AST model, DictAST modelParameters)
        {
            Model = model;
            ModelParameters = modelParameters;
        }
        public override Token FindToken()
        {
            return Model.FindToken();
        }
    }
}