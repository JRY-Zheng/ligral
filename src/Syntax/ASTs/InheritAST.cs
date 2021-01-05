/* Copyright (C) 2019-2021 Junruoyu Zheng. Home page: https://junruoyu-zheng.gitee.io/ligral

   Distributed under MIT license.
   See file LICENSE for detail or copy at https://opensource.org/licenses/MIT
*/

using System.Collections.Generic;
using Ligral.Syntax;

namespace Ligral.Syntax.ASTs
{
    class InheritAST : AST
    {
        public WordAST Name;
        public WordAST Type;
        public InheritAST(WordAST name, WordAST type = null)
        {
            Name = name;
            Type = type;
        }
        public override Token FindToken()
        {
            return Name.FindToken();
        }
    }
}