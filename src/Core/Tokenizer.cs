// MIT License

// Copyright (c) 2025 W.M.R Jap-A-Joe

// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:

// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.

// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

using System.Collections.Generic;

namespace Luadio
{
    public class Tokenizer
    {
        public enum TokenType
        {
            Number,
            Identifier,
            Keyword,
            String,
            Operator,
            Comma,
            Semicolon,
            Colon,
            SquareBracketOpen,
            SquareBracketClose,
            ParenthesisOpen,
            ParenthesisClose,
            CurlyBraceOpen,
            CurlyBraceClose,
            Comment,
            EndOfFile,
            Unknown
        }

        public class Token
        {
            public TokenType Type { get; }
            public string Value { get; }
            public int Position { get; }

            public Token(TokenType type, string value, int position)
            {
                Type = type;
                Value = value;
                Position = position;
            }

            public override string ToString()
            {
                return $"{Position} {Type} {Value}";
            }
        }

        private string _input;
        private int _position;

        private static readonly HashSet<string> Keywords = new HashSet<string>
        {
            "and", "break", "do", "else", "elseif", "end", "false", "for", "function",
            "if", "in", "local", "nil", "not", "or", "repeat", "return", "then", "true", "until", "while"
        };

        public Tokenizer()
        {
            _input = string.Empty;
            _position = 0;
        }

        private char CurrentChar => _position < _input.Length ? _input[_position] : '\0';

        private void Advance()
        {
            _position++;
        }

        private bool IsEndOfFile => _position >= _input.Length;

        public List<Token> Tokenize(string input)
        {
            _input = input;
            _position = 0;
            var tokens = new List<Token>();

            while (!IsEndOfFile)
            {
                if (char.IsWhiteSpace(CurrentChar))
                {
                    Advance();
                }
                else if (CurrentChar == '-' && Peek() == '-')
                {
                    tokens.Add(TokenizeComment());
                }
                else if (CurrentChar == '-' && (!IsEndOfFile || char.IsDigit(Peek())) && Peek() != '-')
                {
                    // Handle negative numbers
                    tokens.Add(TokenizeNegativeNumber());
                }
                else if (char.IsDigit(CurrentChar) || (CurrentChar == '.' && char.IsDigit(Peek())))
                {
                    tokens.Add(TokenizeNumber());
                }

                else if (char.IsLetter(CurrentChar) || CurrentChar == '_')
                {
                    tokens.Add(TokenizeIdentifier());
                }
                else if (CurrentChar == '"')
                {
                    tokens.Add(TokenizeString());
                }
                else if ("+-*/=<>!&|".Contains(CurrentChar))
                {
                    tokens.Add(TokenizeOperator());
                }
                else if(CurrentChar == '[')
                {
                    tokens.Add(TokenizeSquareBracketOpen());
                }
                else if(CurrentChar == ']')
                {
                    tokens.Add(TokenizeSquareBracketClose());
                }
                else if(CurrentChar == '(')
                {
                    tokens.Add(TokenizeParenthesisOpen());
                }
                else if(CurrentChar == ')')
                {
                    tokens.Add(TokenizeParenthesisClose());
                }
                else if(CurrentChar == '{')
                {
                    tokens.Add(TokenizeCurlyBraceOpen());
                }
                else if(CurrentChar == '}')
                {
                    tokens.Add(TokenizeCurlyBraceClose());
                }
                else if(CurrentChar == ',')
                {
                    tokens.Add(TokenizeComma());
                }
                else if(CurrentChar == ';')
                {
                    tokens.Add(TokenizeSemicolon());
                }
                else if(CurrentChar == ':')
                {
                    tokens.Add(TokenizeColon());
                }
                else
                {
                    tokens.Add(new Token(TokenType.Unknown, CurrentChar.ToString(), _position));
                    Advance();
                }
            }

            tokens.Add(new Token(TokenType.EndOfFile, "", _position));
            return tokens;
        }

        private Token TokenizeComment()
        {
            var start = _position;
            Advance(); // Skip the first '-'
            Advance(); // Skip the second '-'

            while (!IsEndOfFile && CurrentChar != '\n')
            {
                Advance();
            }

            var value = _input.Substring(start, _position - start);
            return new Token(TokenType.Comment, value, start);
        }

        private Token TokenizeNegativeNumber()
        {
            var start = _position;
            Advance(); // Skip the minus sign

            // Now we expect a number
            bool hasDecimal = false;

            while (!IsEndOfFile && (char.IsDigit(CurrentChar) || CurrentChar == '.'))
            {
                if (CurrentChar == '.')
                {
                    if (hasDecimal) break; // Only one decimal point allowed
                    hasDecimal = true;
                }
                Advance();
            }

            var value = _input.Substring(start, _position - start);
            return new Token(TokenType.Number, value, start);
        }

        private Token TokenizeNumber()
        {
            var start = _position;
            bool hasDecimal = false;

            while (!IsEndOfFile && (char.IsDigit(CurrentChar) || CurrentChar == '.'))
            {
                if (CurrentChar == '.')
                {
                    if (hasDecimal) break; // Only one decimal point allowed
                    hasDecimal = true;
                }
                Advance();
            }

            var value = _input.Substring(start, _position - start);
            return new Token(TokenType.Number, value, start);
        }

        private Token TokenizeIdentifier()
        {
            var start = _position;

            while (!IsEndOfFile && (char.IsLetterOrDigit(CurrentChar) || CurrentChar == '_'))
            {
                Advance();
            }

            var value = _input.Substring(start, _position - start);
            var type = Keywords.Contains(value) ? TokenType.Keyword : TokenType.Identifier;
            return new Token(type, value, start);
        }

        private Token TokenizeString()
        {
            var start = _position;
            Advance(); // Skip the opening quote

            while (!IsEndOfFile && CurrentChar != '"')
            {
                Advance();
            }

            Advance(); // Skip the closing quote
            var value = _input.Substring(start, _position - start);
            return new Token(TokenType.String, value, start);
        }

        private Token TokenizeOperator()
        {
            var start = _position;
            Advance(); // Move past the operator
            var value = _input.Substring(start, _position - start);
            return new Token(TokenType.Operator, value, start);
        }

        private Token TokenizeSquareBracketOpen()
        {
            var value = CurrentChar.ToString();
            var position = _position; // Store the position
            Advance();
            return new Token(TokenType.SquareBracketOpen, value, position);
        }

        private Token TokenizeSquareBracketClose()
        {
            var value = CurrentChar.ToString();
            var position = _position; // Store the position
            Advance();
            return new Token(TokenType.SquareBracketClose, value, position);
        }

        private Token TokenizeParenthesisOpen()
        {
            var value = CurrentChar.ToString();
            var position = _position; // Store the position
            Advance();
            return new Token(TokenType.ParenthesisOpen, value, position);
        }

        private Token TokenizeParenthesisClose()
        {
            var value = CurrentChar.ToString();
            var position = _position; // Store the position
            Advance();
            return new Token(TokenType.ParenthesisClose, value, position);
        }

        private Token TokenizeCurlyBraceOpen()
        {
            var value = CurrentChar.ToString();
            var position = _position; // Store the position
            Advance();
            return new Token(TokenType.CurlyBraceOpen, value, position);
        }

        private Token TokenizeCurlyBraceClose()
        {
            var value = CurrentChar.ToString();
            var position = _position; // Store the position
            Advance();
            return new Token(TokenType.CurlyBraceClose, value, position);
        }

        private Token TokenizeComma()
        {
            var value = CurrentChar.ToString();
            var position = _position; // Store the position
            Advance();
            return new Token(TokenType.Comma, value, position);
        }

        private Token TokenizeSemicolon()
        {
            var value = CurrentChar.ToString();
            var position = _position; // Store the position
            Advance();
            return new Token(TokenType.Semicolon, value, position);
        }

        private Token TokenizeColon()
        {
            var value = CurrentChar.ToString();
            var position = _position; // Store the position
            Advance();
            return new Token(TokenType.Colon, value, position);
        }

        private char Peek()
        {
            return _position + 1 < _input.Length ? _input[_position + 1] : '\0';
        }
    }
}