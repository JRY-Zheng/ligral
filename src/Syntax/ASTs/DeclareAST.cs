/* Copyright (C) 2019-2022 Junruoyu Zheng. Home page: https://junruoyu-zheng.gitee.io/ligral

   Distributed under MIT license.
   See file LICENSE for detail or copy at https://opensource.org/licenses/MIT
*/

using System.Collections.Generic;
using Ligral.Syntax;

namespace Ligral.Syntax.ASTs
{
    class DeclareAST : AST
    {
        public AST ModelType;
        public WordAST Id;
        public DeclareAST(AST modeType, WordAST id)
        {
            ModelType = modeType;
            Id = id;
        }
        public override Token FindToken()
        {
            return Id.FindToken();
        }
    }
}