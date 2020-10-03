using System.Collections.Generic;

namespace Ligral.ASTs
{
    class UsingAST : AST
    {
        public List<WordAST> FileName;
        public WordAST ModuleName;
        public UsingAST(List<WordAST> fileName, WordAST moduleName)
        {
            FileName = fileName;
            ModuleName = moduleName;
        }
        public override Token FindToken()
        {
            return FileName[0].FindToken();
        }
    }
}