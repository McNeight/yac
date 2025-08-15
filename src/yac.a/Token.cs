// <copyright file="Token.cs" company="yac Contributors">
// MIT License
//
// Copyright © 2014 Rasmus Svensson
// Copyright © 2017, 2022 Daniel Quadros
// Copyright © 2025 Neil McNeight
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.
// </copyright>

/*
 * ASM1802 - CDP1802 Level I Assembler
 *
 * This contains the token parsing code
 *
 * (C) 2017, Daniel Quadros
 *
 */

using System.Text;
using System.Text.RegularExpressions;

namespace yac.a
{
    internal class Token
    {
        // Token types
        public enum TokenType
        {
            ERROR,
            EMPTY,      // no token
            STRING,     // T'c..c'
            BCONST,     // B'b..b'
            HCONST,     // #h..h or X'h..h'
            DCONST,     // D'd..d' or d..d
            TEXT        // la..a will be classified latter as
                        //   symbol, register, directive, instruction or hex const
        };

        public TokenType Type { get; set; }

        public string Text { get; private set; }

        public Statement.StatementError Error { get; private set; } = Statement.StatementError.None;

        // Constructor
        public Token(TokenType type, string text)
        {
            Type = type;
            Text = text;
        }

        // Get next token from line
        public static Token Parse(string line, ref int pos)
        {
            StringBuilder sbText = new StringBuilder();
            Token tk = new Token(TokenType.EMPTY, "");
            bool insideQuotes = false;

            // Extract token text
            while (pos < line.Length)
            {
                char c = line[pos++];
                if (insideQuotes && (c == '\''))
                {
                    if ((pos < line.Length) && (line[pos] == '\''))
                    {
                        // '' -> '
                        sbText.Append(c);
                        pos++;
                        continue;
                    }
                    else
                    {
                        insideQuotes = false;
                        break;
                    }
                }
                if (insideQuotes)
                {
                    sbText.Append(c);
                }
                else if (c == '\'')
                {
                    sbText.Append(c);
                    insideQuotes = true;
                }
                else if (char.IsLetterOrDigit(c) ||
                         ((sbText.Length == 0) && (c == '#')))
                {
                    sbText.Append(c);
                }
                else
                {
                    pos--;  // last char is not part of the token
                    break;
                }
            }
            tk.Text = sbText.ToString();
            if (insideQuotes)
            {
                tk.Type = TokenType.ERROR;
                tk.Error = Statement.StatementError.MissingTrailingQuote;
                return tk;
            }

            // Found out what kind of token
            if (tk.Text.Length == 0)
            {
                return tk;
            }
            if (tk.Text[0] == '#')
            {
                tk.CheckHex(1);
            }
            else if (char.IsLetter(tk.Text[0]))
            {
                tk.Text = tk.Text.ToUpper();
                if ((tk.Text.Length > 1) && (tk.Text[1] == '\''))
                {
                    switch (tk.Text[0])
                    {
                        case 'B':
                            tk.CheckBin();
                            break;

                        case 'D':
                            tk.CheckDec(2);
                            break;

                        case 'T':
                            tk.CheckString();
                            break;

                        case 'X':
                            tk.CheckHex(2);
                            break;

                        default:
                            tk.Type = TokenType.ERROR;
                            tk.Error = Statement.StatementError.InvalidSyntax;
                            break;
                    }
                }
                else
                {
                    tk.CheckAlpha();
                }
            }
            else
            {
                // Must be a decimal number
                tk.CheckDec(0);
            }

            return tk;
        }

        // Check that token has only alphanumeric chars
        private void CheckAlpha()
        {
            Regex rx = new Regex("^[0-9A-Z]+$");
            if (rx.Match(Text).Success)
            {
                Type = TokenType.TEXT;
            }
            else
            {
                Type = TokenType.ERROR;
                Error = Statement.StatementError.InvalidSyntax;
            }
        }

        // Check if valid binary contant
        private void CheckBin()
        {
            Regex rx = new Regex("^B\'[01]{1,8}$");
            if (rx.Match(Text).Success)
            {
                Type = TokenType.BCONST;
                Text = Text.Substring(2);
            }
            else
            {
                Type = TokenType.ERROR;
                Error = Statement.StatementError.InvalidBinaryConstant;
            }
        }

        // Check if valid decimal contant
        private void CheckHex(int start)
        {
            Regex rx = new Regex("^[0-9A-F]{1,4}$");
            string txt = Text.Substring(start);
            if (rx.Match(txt).Success)
            {
                Type = TokenType.HCONST;
                Text = txt;
            }
            else
            {
                Type = TokenType.ERROR;
                Error = Statement.StatementError.InvalidHexConstant;
            }
        }

        // Check if valid hexdecimal contant
        private void CheckDec(int start)
        {
            Regex rx = new Regex("^[0-9]{1,5}$");
            string txt = Text.Substring(start);
            if (rx.Match(txt).Success && (int.Parse(txt) <= 0xFFFF))
            {
                Type = TokenType.DCONST;
                Text = txt;
            }
            else
            {
                Type = TokenType.ERROR;
                Error = Statement.StatementError.InvalidDecimalConstant;
            }
        }

        // Check if valid string
        private void CheckString()
        {
            if (Text.Length > 2)
            {
                // T'xxx
                Type = TokenType.STRING;
                Text = Text.Substring(2);
            }
            else
            {
                // T'
                Type = TokenType.ERROR;
                Error = Statement.StatementError.MissingTrailingQuote;
            }
        }
    }
}
