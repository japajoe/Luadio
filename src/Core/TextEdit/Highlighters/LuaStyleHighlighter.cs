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

using System;

namespace ImGuiColorTextEditNet
{
    public class LuaStyleHighlighter : ISyntaxHighlighter
    {
        static readonly object DefaultState = new();
        static readonly object MultiLineCommentState = new();
        readonly SimpleTrie<Identifier> _identifiers;

        record Identifier(PaletteIndex Color)
        {
            public string Declaration = "";
        }

        public LuaStyleHighlighter()
        {
            var language = Lua();

            _identifiers = new SimpleTrie<Identifier>();
            if (language.Keywords != null)
                foreach (var keyword in language.Keywords)
                    _identifiers.Add(keyword, new Identifier(PaletteIndex.Keyword));

            if (language.Identifiers != null)
            {
                foreach (var name in language.Identifiers)
                {
                    var identifier = new Identifier(PaletteIndex.KnownIdentifier)
                    {
                        Declaration = "Built-in function"
                    };
                    _identifiers.Add(name, identifier);
                }
            }
        }

        public bool AutoIndentation => true;
        public int MaxLinesPerFrame => 1000;

        public string? GetTooltip(string id)
        {
            var info = _identifiers.Get(id);
            return info?.Declaration;
        }

        public object Colorize(Span<Glyph> line, object? state)
        {
            for (int i = 0; i < line.Length;)
            {
                int result = Tokenize(line[i..], ref state);
                Util.Assert(result != 0);

                if (result == -1)
                {
                    line[i] = new Glyph(line[i].Char, PaletteIndex.Default);
                    i++;
                }
                else i += result;
            }

            return state ?? DefaultState;
        }

        int Tokenize(Span<Glyph> span, ref object? state)
        {
            int i = 0;

            // Skip leading whitespace
            while (i < span.Length && span[i].Char is ' ' or '\t')
                i++;

            if (i > 0)
                return i;

            int result;
            if ((result = TokenizeMultiLineComment(span, ref state)) != -1) return result;
            if ((result = TokenizeSingleLineComment(span)) != -1) return result;
            if ((result = TokenizeLuaString(span)) != -1) return result;
            if ((result = TokenizeLuaIdentifier(span)) != -1) return result;
            if ((result = TokenizeLuaNumber(span)) != -1) return result;
            if ((result = TokenizeLuaPunctuation(span)) != -1) return result;
            return -1;
        }

        static int TokenizeMultiLineComment(Span<Glyph> span, ref object? state)
        {
            int i = 0;
            if (state != MultiLineCommentState && (span[i].Char != '-' || 1 >= span.Length || span[1].Char != '-'))
                return -1;

            state = MultiLineCommentState;
            for (; i < span.Length; i++)
            {
                span[i] = new Glyph(span[i].Char, PaletteIndex.MultiLineComment);
                if (span[i].Char == '-' && i + 1 < span.Length && span[i + 1].Char == '-')
                {
                    i++;
                    span[i] = new Glyph(span[i].Char, PaletteIndex.MultiLineComment);
                    state = DefaultState;
                    return i;
                }
            }

            return i;
        }

        static int TokenizeSingleLineCommentOriginal(Span<Glyph> span)
        {
            if (span[0].Char != '-' || 1 >= span.Length || span[1].Char != '-')
                return -1;

            for (int i = 0; i < span.Length; i++)
                span[i] = new Glyph(span[i].Char, PaletteIndex.Comment);

            return span.Length;
        }

        static int TokenizeSingleLineComment(Span<Glyph> span)
        {
            if(span.Length < 2)
                return -1;

            if(span[0].Char == '-' && span[1].Char == ' ' || char.IsDigit(span[1].Char))
                return -1;

            if(span[0].Char != '-' && span[1].Char != '-')
                return -1;

            for (int i = 0; i < span.Length; i++)
                span[i] = new Glyph(span[i].Char, PaletteIndex.Comment);

            return span.Length;
        }

        static int TokenizeLuaString(Span<Glyph> input)
        {
            if (input[0].Char != '"' && input[0].Char != '\'')
                return -1; // No opening quotes

            char quote = input[0].Char;

            for (int i = 1; i < input.Length; i++)
            {
                var c = input[i].Char;

                // handle end of string
                if (c == quote)
                {
                    for (int j = 0; j <= i; j++)
                        input[j] = new Glyph(input[j].Char, PaletteIndex.String);

                    return i + 1;
                }

                // handle escape character for "
                if (c == '\\' && i + 1 < input.Length && input[i + 1].Char == quote)
                    i++;
            }

            return -1; // No closing quotes
        }

        int TokenizeLuaIdentifier(Span<Glyph> input)
        {
            int i = 0;

            var c = input[i].Char;
            if (!char.IsLetter(c) && c != '_')
                return -1;

            i++;

            for (; i < input.Length; i++)
            {
                c = input[i].Char;
                if (c != '_' && !char.IsLetterOrDigit(c))
                    break;
            }

            var info = _identifiers.Get<Glyph>(input[..i], x => x.Char);

            for (int j = 0; j < i; j++)
                input[j] = new Glyph(input[j].Char, info?.Color ?? PaletteIndex.Identifier);

            return i;
        }

        static int TokenizeLuaNumber(Span<Glyph> input)
        {
            int i = 0;
            char c = input[i].Char;

            bool startsWithNumber = char.IsNumber(c);

            if (c != '+' && c != '-' && !startsWithNumber)
                return -1;

            i++;

            bool hasNumber = startsWithNumber;
            while (i < input.Length && char.IsNumber(input[i].Char))
            {
                hasNumber = true;
                i++;
            }

            if (!hasNumber)
                return -1;

            bool isFloat = false;

            if (i < input.Length && input[i].Char == '.')
            {
                isFloat = true;

                i++;
                while (i < input.Length && char.IsNumber(input[i].Char))
                    i++;
            }

            if (!isFloat)
            {
                // integer size type
                while (i < input.Length && input[i].Char is 'u' or 'U' or 'l' or 'L')
                    i++;
            }

            return i;
        }

        static int TokenizeLuaPunctuation(Span<Glyph> input)
        {
            switch (input[0].Char)
            {
                case '[': case ']':
                case '{': case '}':
                case '(': case ')':
                case '-': case '+':
                case '<': case '>':
                case '?': case ':': case ';':
                case '!': case '%': case '^':
                case '&': case '|':
                case '*': case '/':
                case '=':
                case '~':
                case ',': case '.':
                    input[0] = new Glyph(input[0].Char, PaletteIndex.Punctuation);
                    return 1;
                default:
                    return -1;
            }
        }

        static LanguageDefinition Lua() => new("Lua")
        {
            Keywords = new[]{
                "and", "break", "do", "else", "elseif", "end", "false", "for", "function", "goto", "if", "in", "local", "nil", "not", "or", "repeat", "return", "then", "true", "until", "while"
            },
            Identifiers = new[]{
                "assert", "collectgarbage", "dofile", "error", "getmetatable", "ipairs", "load", "loadfile", "next", "pairs", "pcall", "print", "rawequal", "rawget", "rawlen", "rawset", "require", "select", "setmetatable", "tonumber", "tostring", "type", "xpcall",
                "_G", "_VERSION", "coroutine", "debug", "io", "math", "os", "package", "string", "table", "utf8", 
                "Checkbox",
                "DragFloat",
                "DragInt",
                "InputFloat",
                "InputInt",
                "SliderFloat",
                "SliderInt"
            }
        };
    }
}