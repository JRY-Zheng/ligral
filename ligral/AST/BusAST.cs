using System.Collections.Generic;

namespace Ligral.ASTs
{
    class BusAST : AST
    {
        public List<ChainAST> Chains;
        public BusAST(List<ChainAST> chains)
        {
            Chains = chains;
        }
        public override Token FindToken()
        {
            return Chains[0].FindToken();
        }
    }
}