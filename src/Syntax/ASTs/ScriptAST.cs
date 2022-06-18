/* Copyright (C) 2019-2022 Junruoyu Zheng. Home page: https://junruoyu-zheng.gitee.io/ligral

   Distributed under MIT license.
   See file LICENSE for detail or copy at https://opensource.org/licenses/MIT
*/

using System.Collections.Generic;
using Ligral.Syntax;

namespace Ligral.Syntax.ASTs
{
    class ScriptAST : AST
    {
        public List<WordAST> ParameterTokens;
        public OperatorToken StartToken;
        public StringToken EndToken;
        public ScriptAST(List<WordAST> parameterTokens, OperatorToken start, StringToken end)
        {
            ParameterTokens = parameterTokens;
            StartToken = start;
            EndToken = end;
        }
        public override Token FindToken()
        {
            return StartToken;
        }
    }
}