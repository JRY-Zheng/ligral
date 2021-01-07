/* Copyright (C) 2019-2021 Junruoyu Zheng. Home page: https://junruoyu-zheng.gitee.io/ligral

   Distributed under MIT license.
   See file LICENSE for detail or copy at https://opensource.org/licenses/MIT
*/

namespace Ligral.Syntax
{
    class Lexer
    {
        private string text;
        private int position = 0;
        private char currentChar;
        private int lineNO = 1;
        private int columnNO = 1;
        private int positionBackup = 0;
        private char currentCharBackup;
        private int lineNOBackup = 1;
        private int columnNOBackup = 1;
        private int file;
        private Logger logger = new Logger("Lexer");
        public void Backup()
        {
            positionBackup = position;
            currentCharBackup = currentChar;
            lineNOBackup = lineNO;
            columnNOBackup = columnNO;
        }
        public void Restore()
        {
            position = positionBackup;
            currentChar = currentCharBackup;
            lineNO = lineNOBackup;
            columnNO = columnNOBackup;
        }
        public void Load(string text, int file)
        {
            this.text = text;
            this.file = file;
            if (text.Length==0)
            {
                currentChar = '\0';
                // throw logger.Error(new LigralException("Lexer Error: No text given."));
            }
            else
            {
                // this.text = text;
                currentChar = text[0];
            }
        }
        private void Advance(int step=1)
        {
            while(step!=0)
            {
                if (currentChar=='\n')
                {
                    lineNO++;
                    columnNO = 0;
                }
                position++;
                if (position>=text.Length)
                {
                    currentChar = '\0';
                }
                else
                {
                    currentChar = text[position];
                    columnNO++;
                }
                step--;
            }
        }
        private char Peek(int provision=1)
        {
            if (position+provision<text.Length)
            {
                return text[position+provision];
            }
            else
            {
                return '\0';
            }
        }
        private void SkipWhitespace()
        {
            while (char.IsWhiteSpace(currentChar))
            {
                Advance();
            }
        }
        private void SkipComment()
        {
            if (currentChar=='#' && Peek()!='#')
            {
                Advance();
                while (currentChar!='\n' && currentChar!='\r' && currentChar!='\0')
                {
                    Advance();
                }
                Advance();
            }
            else if (currentChar=='#' && Peek()=='#' && Peek(2)!='#')
            {
                Advance(2);
                while (currentChar!='\n' && currentChar!='\r' && currentChar!='\0')
                {
                    if (currentChar=='#' && Peek()=='#')
                    {
                        Advance();
                        break;
                    }
                    Advance();
                }
                Advance();
            }
            else if (currentChar=='#' && Peek()=='#' && Peek(2)=='#')
            {
                Advance(3);
                while ((currentChar!='#'&&currentChar!='\0')||(Peek()!='#'&&Peek()!='\0')||(Peek(2)!='#'&&Peek(2)!='\0'))
                {
                    Advance();
                }
                Advance(3);
            }
        }
        private string String()
        {
            char quotation = currentChar;
            Advance();// validation is done in GetNextToken()
            int startPosition = position;
            while (currentChar!=quotation)
            {
                if (currentChar!='\n' && currentChar!='\r' && currentChar!='\0')
                {
                    Advance();
                }
                else
                {
                    throw logger.Error(new LigralException($"Lexer Error: Incomplete string at line {lineNO}"));
                }
            }
            int endPosition = position;
            Advance();
            return text.Substring(startPosition, endPosition-startPosition);
        }
        private double Digit()
        {
            int startPosition = position;
            while (char.IsDigit(currentChar))
            {
                Advance();
            }
            if (currentChar=='.')
            {
                Advance();
                while (char.IsDigit(currentChar))
                {
                    Advance();
                }
            }
            if (currentChar=='e')
            {
                Advance();
                if (currentChar=='-' || currentChar=='+')
                {
                    Advance();
                }
                while (char.IsDigit(currentChar))
                {
                    Advance();
                }
            }
            return System.Convert.ToDouble(text.Substring(startPosition,position-startPosition));
        }
        private string Word()
        {
            int startPosition = position;
            Advance();// validation is done in GetNextToken()
            while (char.IsLetterOrDigit(currentChar)||currentChar=='_')
            {
                Advance();
            }
            return text.Substring(startPosition, position-startPosition);
        }
        public Token GetNextToken()
        {
            while (char.IsWhiteSpace(currentChar)||currentChar=='#')
            {
                SkipWhitespace();
                SkipComment();
            }
            if (char.IsDigit(currentChar))
            {
                return new DigitToken(TokenType.DIGIT, Digit(), lineNO, columnNO, file);
            }
            else if (currentChar=='\''||currentChar=='"')
            {
                return new StringToken(TokenType.STRING, String(), lineNO, columnNO, file);
            }
            else if (char.IsLetter(currentChar)||currentChar=='_')
            {
                string word = Word();
                switch (word)
                {
                case "digit":
                    // supported in v0.2.0, unsupported in higher version
                    // replaced by `let`
                    return new StringToken(TokenType.ASSIGN, word, lineNO, columnNO, file);
                case "let":
                    return new StringToken(TokenType.ASSIGN, word, lineNO, columnNO, file);
                case "route":
                    return new StringToken(TokenType.ROUTE, word, lineNO, columnNO, file);
                case "end":
                    return new StringToken(TokenType.END, word, lineNO, columnNO, file);
                case "import":
                    return new StringToken(TokenType.IMPORT, word, lineNO, columnNO, file);
                case "using":
                    return new StringToken(TokenType.USING, word, lineNO, columnNO, file);
                case "conf":
                    return new StringToken(TokenType.CONF, word, lineNO, columnNO, file);
                case "true":
                    return new BoolToken(TokenType.TRUE, true, lineNO, columnNO, file);
                case "false":
                    return new BoolToken(TokenType.FALSE, false, lineNO, columnNO, file);
                case "signature":
                    return new BoolToken(TokenType.SIGN, false, lineNO, columnNO, file);
                default:
                    return new StringToken(TokenType.ID, word, lineNO, columnNO, file);
                }
            }
            else if (currentChar=='-'&&Peek()=='>')
            {
                Advance(2);
                return new StringToken(TokenType.GOTO, "->", lineNO, columnNO, file);
            }
            else if (currentChar=='<'&&Peek()=='-')
            {
                // supported in v0.2.0, unsupported in higher version
                // replaced by `=`
                // in v0.3.0 or higher logic expression will be supported
                Advance(2);
                return new StringToken(TokenType.FROM, "<-", lineNO, columnNO, file);
            }
            else if (currentChar=='=')
            {
                Advance();
                return new OperantToken(TokenType.FROM, "=", lineNO, columnNO, file);
            }
            else if (currentChar=='(')
            {
                Advance();
                return new OperantToken(TokenType.LPAR, "(", lineNO, columnNO, file);
            }
            else if (currentChar==')')
            {
                Advance();
                return new OperantToken(TokenType.RPAR, ")", lineNO, columnNO, file);
            }
            else if (currentChar=='[')
            {
                Advance();
                return new OperantToken(TokenType.LBRK, "[", lineNO, columnNO, file);
            }
            else if (currentChar==']')
            {
                Advance();
                return new OperantToken(TokenType.RBRK, "]", lineNO, columnNO, file);
            }
            else if (currentChar=='{')
            {
                Advance();
                return new OperantToken(TokenType.LBRC, "{", lineNO, columnNO, file);
            }
            else if (currentChar=='}')
            {
                Advance();
                return new OperantToken(TokenType.RBRC, "}", lineNO, columnNO, file);
            }
            else if (currentChar=='+')
            {
                Advance();
                return new OperantToken(TokenType.PLUS, "+", lineNO, columnNO, file);
            }
            else if (currentChar=='-')
            {
                Advance();
                return new OperantToken(TokenType.MINUS, "-", lineNO, columnNO, file);
            }
            else if (currentChar=='*')
            {
                Advance();
                return new OperantToken(TokenType.MUL, "*", lineNO, columnNO, file);
            }
            else if (currentChar=='/')
            {
                Advance();
                return new OperantToken(TokenType.DIV, "/", lineNO, columnNO, file);
            }
            else if (currentChar==';')
            {
                Advance();
                return new OperantToken(TokenType.SEMIC, ";", lineNO, columnNO, file);
            }
            else if (currentChar==':')
            {
                Advance();
                return new OperantToken(TokenType.COLON, ":", lineNO, columnNO, file);
            }
            else if (currentChar==',')
            {
                Advance();
                return new OperantToken(TokenType.COMMA, ",", lineNO, columnNO, file);
            }
            else if (currentChar=='.')
            {
                Advance();
                switch (currentChar)
                {
                case '*':
                    Advance();
                    return new OperantToken(TokenType.BCMUL, ".*", lineNO, columnNO, file);
                case '/':
                    Advance();
                    return new OperantToken(TokenType.BCDIV, "./", lineNO, columnNO, file);
                case '^':
                    Advance();
                    return new OperantToken(TokenType.BCPOW, ".^", lineNO, columnNO, file);
                default:
                    return new OperantToken(TokenType.DOT, ".", lineNO, columnNO, file);
                }
            }
            else if (currentChar=='^')
            {
                Advance();
                return new OperantToken(TokenType.CARET, "^", lineNO, columnNO, file);
            }
            else if (currentChar=='~')
            {
                Advance();
                return new OperantToken(TokenType.TILDE, "~", lineNO, columnNO, file);
            }
            else if (currentChar=='\0')
            {
                return new OperantToken(TokenType.EOF, "\0", lineNO, columnNO, file);
            }
            else 
            {
                throw logger.Error(new LigralException($"Lexer Error: Unexpected char {currentChar} at line {lineNO} column {columnNO}"));
            }
        }
    }
}