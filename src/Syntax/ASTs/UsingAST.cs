/* Copyright 2019-2020 Junruoyu Zheng. All rights reserved.

   Distributed under MIT license.
   See file LICENSE for detail or copy at https://opensource.org/licenses/MIT
*/

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