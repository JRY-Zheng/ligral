using System.Collections.Generic;
using Ligral.Syntax;

namespace Ligral.Syntax.ASTs
{
    class ImportAST : AST
    {
        public List<WordAST> FileName;
        public bool Relative;
        public ImportAST(List<WordAST> fileName, bool relative)
        {
            FileName = fileName;
            Relative = relative;
        }
        public override Token FindToken()
        {
            return FileName[0].FindToken();
        }
    }
}