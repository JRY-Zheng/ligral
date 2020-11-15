using System.Collections.Generic;
using Ligral.Syntax;

namespace Ligral.Syntax.ASTs
{
    class AST 
    {
        public virtual Token FindToken()
        {
            return null;
        }
    }
}