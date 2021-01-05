/* Copyright 2019-2020 Junruoyu Zheng. All rights reserved.

   Distributed under MIT license.
   See file LICENSE for detail or copy at https://opensource.org/licenses/MIT
*/

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