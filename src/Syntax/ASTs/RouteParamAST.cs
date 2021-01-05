/* Copyright (C) 2019-2020 Junruoyu Zheng. Home page: https://junruoyu-zheng.gitee.io/ligral

   Distributed under MIT license.
   See file LICENSE for detail or copy at https://opensource.org/licenses/MIT
*/

using System.Collections.Generic;
using Ligral.Syntax;

namespace Ligral.Syntax.ASTs
{
    class RouteParamAST : AST
    {
        public WordAST Name;
        public WordAST Type;
        public AST DefaultValue;
        public RouteParamAST(WordAST name, WordAST type = null, AST defaultValue = null)
        {
            Name = name;
            Type = type;
            DefaultValue = defaultValue;
        }
        public override Token FindToken()
        {
            return Name.FindToken();
        }
    }
}