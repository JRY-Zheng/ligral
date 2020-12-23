using System.Collections.Generic;
using Ligral.Syntax;

namespace Ligral.Syntax.ASTs
{
    class UsingAST : AST
    {
        public List<WordAST> FileName;
        public WordAST ModuleName;
        public bool Relative;
        public UsingAST(List<WordAST> fileName, WordAST moduleName, bool relative)
        {
            FileName = fileName;
            ModuleName = moduleName;
            Relative = relative;
        }
        public override Token FindToken()
        {
            return FileName[0].FindToken();
        }
    }
}