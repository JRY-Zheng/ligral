/* Copyright (C) 2019-2021 Junruoyu Zheng. Home page: https://junruoyu-zheng.gitee.io/ligral

   Distributed under MIT license.
   See file LICENSE for detail or copy at https://opensource.org/licenses/MIT
*/

namespace Ligral.Syntax
{
    enum TokenType
    {
        STRING,DIGIT,PLUS,MINUS,MUL,DIV,
        FROM,GOTO,LPAR,RPAR,LBRC,RBRC,LBRK,RBRK,
        COMMA,COLON,SEMIC,DOT,CARET,
        ID,EOF,ASSIGN,ROUTE,END,USING,IMPORT,CONF,
        TRUE,FALSE,TILDE,SIGN
    }

    class Token
    {
        public TokenType Type;
        public object Value;
        public int Line;
        public int Column;
        public Token(TokenType type, object value, int line, int column)
        {
            Type = type;
            Value = value;
            Line = line;
            Column = column;
        }
        public override string ToString()
        {
            return string.Format("Token({0}={1},at({2},{3}))",Type, Value, Line, Column);
        }
    }

    class DigitToken : Token
    {
        public DigitToken(TokenType type, double value, int line, int column) : base(type, value, line, column)
        {
        }
    }

    class StringToken : Token
    {
        public StringToken(TokenType type, string value, int line, int column) : base(type, value, line, column)
        {
        }
    }

    class CharToken : Token
    {
        public CharToken(TokenType type, char value, int line, int column) : base(type, value, line, column)
        {
        }
    }

    class BoolToken : Token
    {
        public BoolToken(TokenType type, bool value, int line, int column) : base(type, value, line, column)
        {
        }
    }
}