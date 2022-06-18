/* Copyright (C) 2019-2022 Junruoyu Zheng. Home page: https://junruoyu-zheng.gitee.io/ligral

   Distributed under MIT license.
   See file LICENSE for detail or copy at https://opensource.org/licenses/MIT
*/

using System.Collections.Generic;
using Ligral.Syntax;

namespace Ligral.Syntax.ASTs
{
    class ScriptFileAST : AST
    {
        public List<WordAST> FileName;
        public bool Relative;
        public ScriptFileAST(List<WordAST> fileName, bool relative)
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