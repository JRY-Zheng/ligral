using System.Collections.Generic;
using Ligral.Syntax;

namespace Ligral.Syntax.ASTs
{
    class ConfListAST : AST
    {
        public List<ConfAST> Confs;
        public ConfListAST(List<ConfAST> confs)
        {
            Confs = confs;
        }
        public override Token FindToken()
        {
            return Confs[0].FindToken();
        }
    }
}