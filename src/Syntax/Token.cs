/* Copyright (C) 2019-2022 Junruoyu Zheng. Home page: https://junruoyu-zheng.gitee.io/ligral

   Distributed under MIT license.
   See file LICENSE for detail or copy at https://opensource.org/licenses/MIT
*/

namespace Ligral.Syntax
{
    public enum TokenType
    {
        STRING,DIGIT,PLUS,MINUS,MUL,DIV,RDIV,SCRIPT,
        EQUAL,GOTO,LPAR,RPAR,LBRC,RBRC,LBRK,RBRK,
        COMMA,COLON,SEMIC,DOT,CARET,QUE,AT,DOLL,
        ID,EOF,ASSIGN,ROUTE,END,USING,IMPORT,CONF,
        TRUE,FALSE,TILDE,SIGN,BCMUL,BCDIV,BCPOW,
        INPORT,OUTPORT
    }

    public class Token
    {
        public TokenType Type;
        public object Value;
        public int Line;
        public int Column;
        public int File;
        public Token(TokenType type, object value, int line, int column, int file)
        {
            Type = type;
            Value = value;
            Line = line;
            Column = column;
            File = file;
        }
        public override string ToString()
        {
            return string.Format("Token({0}={1},in file {2}:line {3} column {4}",Type, Value, File, Line, Column);
        }
    }

    class DigitToken : Token
    {
        public DigitToken(TokenType type, double value, int line, int column, int file) : base(type, value, line, column, file)
        {
        }
    }

    class StringToken : Token
    {
        public StringToken(TokenType type, string value, int line, int column, int file) : base(type, value, line, column, file)
        {
        }
    }

    class OperatorToken : Token
    {
        public OperatorToken(TokenType type, string value, int line, int column, int file) : base(type, value, line, column, file)
        {
        }
    }

    class BoolToken : Token
    {
        public BoolToken(TokenType type, bool value, int line, int column, int file) : base(type, value, line, column, file)
        {
        }
    }
}