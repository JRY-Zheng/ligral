using System.Collections.Generic;
using Ligral.Syntax;

namespace Ligral.Syntax.ASTs
{
    class DictAST : AST
    {
        public List<KeyValuePairAST> Parameters;
        public DictAST(List<KeyValuePairAST> parameters)
        {
            Parameters = parameters;
        }
        public override Token FindToken()
        {
            return Parameters[0].FindToken();
        }
    }
}