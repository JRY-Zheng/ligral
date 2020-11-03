using System.Collections.Generic;
using Ligral.Syntax;

namespace Ligral.Syntax.ASTs
{
    class ImportAST : AST
    {
        public List<WordAST> FileName;
        public ImportAST(List<WordAST> fileName)
        {
            FileName = fileName;
        }
        public override Token FindToken()
        {
            return FileName[0].FindToken();
        }
    }
}