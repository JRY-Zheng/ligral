/* Copyright 2019-2020 Junruoyu Zheng. All rights reserved.

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