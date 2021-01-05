/* Copyright (C) 2019-2020 Junruoyu Zheng. Home page: https://junruoyu-zheng.gitee.io/ligral

   Distributed under MIT license.
   See file LICENSE for detail or copy at https://opensource.org/licenses/MIT
*/

using System.Collections.Generic;
using Ligral.Syntax;

namespace Ligral.Syntax.ASTs
{
    class ProgramAST : AST
    {
        public string Name;
        public StatementsAST Statements;
        public ProgramAST(string name, StatementsAST statements)
        {
            Name = name;
            Statements = statements;
        }
        public override Token FindToken()
        {
            return Statements.FindToken();
        }
    }
}