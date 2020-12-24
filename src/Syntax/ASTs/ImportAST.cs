using System.Collections.Generic;
using Ligral.Syntax;

namespace Ligral.Syntax.ASTs
{
    class ImportAST : AST
    {
        public List<WordAST> FileName;
        public List<WordAST> Symbols;
        public bool Relative;
        public ImportAST(List<WordAST> fileName, bool relative, List<WordAST> symbols)
        {
            FileName = fileName;
            Relative = relative;
            Symbols = symbols;
        }
        public override Token FindToken()
        {
            return FileName[0].FindToken();
        }
    }
}