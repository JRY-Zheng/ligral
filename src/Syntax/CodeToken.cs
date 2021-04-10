/* Copyright (C) 2019-2021 Junruoyu Zheng. Home page: https://junruoyu-zheng.gitee.io/ligral

   Distributed under MIT license.
   See file LICENSE for detail or copy at https://opensource.org/licenses/MIT
*/

namespace Ligral.Syntax
{
    enum CodeTokenType
    {
        WORD, ASSIGN, LPAR, RPAR, COMMA, COLON, ADDR
    }

    class CodeToken
    {
        public CodeTokenType Type;
        public object Value;
        public CodeToken(CodeTokenType type, object value)
        {
            Type = type;
            Value = value;
        }
        public override string ToString()
        {
            return string.Format("CodeToken({0}={1}",Type, Value);
        }
    }

}