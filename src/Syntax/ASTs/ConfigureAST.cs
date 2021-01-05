/* Copyright 2019-2020 Junruoyu Zheng. All rights reserved.

   Distributed under MIT license.
   See file LICENSE for detail or copy at https://opensource.org/licenses/MIT
*/

using System.Collections.Generic;
using Ligral.Syntax;

namespace Ligral.Syntax.ASTs
{
    class ConfigureAST : AST
    {
        public AST Model;
        public DictAST ModelParameters;
        public ConfigureAST(AST model, DictAST modelParameters)
        {
            Model = model;
            ModelParameters = modelParameters;
        }
        public override Token FindToken()
        {
            return Model.FindToken();
        }
    }
}