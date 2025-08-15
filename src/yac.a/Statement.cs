// <copyright file="Statement.cs" company="yac Contributors">
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

using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using System.Text.RegularExpressions;

namespace yac.a
{
    /// <summary>
    /// This module treats a source code statement.
    /// </summary>
    /// <remarks>
    /// [Label:] [Mnemonic [operand] [datalist]] [,datalist]
    /// </remarks>
    public class Statement
    {
        // Statement types
        public enum StatementType
        { NOP, DC, EQU, ORG, PAGE, END, INSTR, ERROR };

        /// <summary>
        /// CRA error codes and their meanings.
        /// </summary>
        public enum StatementError
        {
            /// <summary>
            /// No error found.
            /// </summary>
            None = 0,

            /// <summary>
            /// Unrecognized mnemonic or missing comma.
            /// </summary>
            /// <remarks>
            /// The body of a statement (other than EQUATE) must begin with either a valid operation mnemonic or DC, ORG, PAGE, END, or a comma.
            /// </remarks>
            UnrecognizedMnemonic = 1,

            /// <summary>
            /// Previously defined symbol.
            /// </summary>
            /// <remarks>
            /// An attempt has been made to define a symbol which already has an entry (and a value) into the symbol table.
            /// </remarks>
            PreviouslyDefinedSymbol = 2,

            /// <summary>
            /// Invalid character within binary constant.
            /// </summary>
            /// <remarks>
            /// COSMAC1802 is in the process of evaluating a binary constant and has found a character other than 0/1 or the trailing single quote (which may be missing).
            /// </remarks>
            InvalidBinaryConstant = 4,

            /// <summary>
            /// Expected hex or decimal constant here.
            /// </summary>
            MISSING_CONST = 6,  // A constant was expected

            /// <summary>
            /// Undefined symbol.
            /// </summary>
            /// <remarks>
            /// COSMAC1802 encountered a symbol reference and wants to use its value, but does not find it listed in the symbol table.
            /// </remarks>
            UndefinedSymbol = 7,

            /// <summary>
            /// Expected expression here.
            /// </summary>
            /// <remarks>
            /// COSMAC1802 determined that an expression was to follow next and did not find leading characters which were proper.
            /// </remarks>
            ExpectedExpression = 8,

            /// <summary>
            /// Invalid character within hex constant.
            /// </summary>
            /// <remarks>
            /// COSMAC1802 is in the process of evaluating a hex constant and has found an invalid character. (This error code may be caused by an uneven number of hex digits.)
            /// </remarks>
            InvalidHexConstant = 9,

            /// <summary>
            /// Missing trailing quote in text constant.
            /// </summary>
            /// <remarks>
            /// Note that the error marker "?" will not appear because this error is always detected at the end of a line.
            /// </remarks>
            MissingTrailingQuote = 10,

            /// <summary>
            /// Period error.
            /// </summary>
            /// <remarks>
            /// Either illegal use of a single period or a missing period beginning a comment.
            /// </remarks>
            PeriodError = 11,

            /// <summary>
            /// Leading character error.
            /// </summary>
            /// <remarks>
            /// At the beginning of a statement, a leading alphabetic or comma was not found.
            /// </remarks>
            BAD_START = 12,

            /// <summary>
            /// Branch out of page.
            /// </summary>
            INV_BRANCH = 14,

            /// <summary>
            /// Invalid register number.
            /// </summary>
            INV_REG = 15,

            /// <summary>
            /// Device number out of range.
            /// </summary>
            INV_DEV = 16,

            /// <summary>
            /// Invalid decimal constant.
            /// </summary>
            InvalidDecimalConstant,

            /// <summary>
            /// Missing closing parentheses.
            /// </summary>
            MISSING_PAREN,

            /// <summary>
            /// Anything else we cannot accept.
            /// </summary>
            InvalidSyntax,
        };

        public static string MsgError(StatementError err)
        {
            switch (err)
            {
                case StatementError.UnrecognizedMnemonic: return "Invalid mnemonic or missing comma";
                case StatementError.PreviouslyDefinedSymbol: return "Previously defined symbol";
                case StatementError.InvalidBinaryConstant: return "Invalid binary constant";
                case StatementError.MISSING_CONST: return "A constant was expected";
                case StatementError.UndefinedSymbol: return "Undefined symbol";
                case StatementError.ExpectedExpression: return "An expression was expected";
                case StatementError.InvalidHexConstant: return "Invalid hex constant";
                case StatementError.MissingTrailingQuote: return "Missing end quote";
                case StatementError.PeriodError: return "Invalid \'.\'";
                case StatementError.BAD_START: return " Invalid char at start of statement";
                case StatementError.INV_BRANCH: return "Branch out of page";
                case StatementError.INV_REG: return "Invalid register number";
                case StatementError.INV_DEV: return "Invalid device number";
                case StatementError.InvalidDecimalConstant: return "Invalid decimal constant";
                case StatementError.MISSING_PAREN: return "Missing closing parentheses";
                case StatementError.InvalidSyntax: return "Syntax error";
                case StatementError.None: return "No error";
                default:
                    break;
            }
            return "Unknown error";
        }

        // Value sizes
        // CRA has a few quirks about sizes
        public enum ValueSize
        { ONE, TWO, FORCE_TWO };

        /// <summary>
        /// This fields are determined in the first pass.
        /// </summary>
        public StatementType Type { get; private set; } = StatementType.NOP;

        private StatementError _error = StatementError.None;

        public StatementError Error
        {
            get { return _error; }
            set
            {
                _error = value;
                if (value != StatementError.None)
                {
                    Type = StatementType.ERROR;
                }
            }
        }

        public string Label { get; private set; }

        private Instruction inst;
        private ushort operand = 0;
        private ValueSize opersize = ValueSize.ONE;
        private List<byte> datalist;

        public int Size { get; private set; }

        public int LineNum { get; }

        private readonly string text;

        /// <summary>
        /// Final result of value will be determined on second pass.
        /// </summary>
        public ushort Value { get; private set; }

        private ValueSize vs = ValueSize.ONE;

        // Object code, determined at second pass
        private byte[] code;

        // Constructors
        public Statement()
        {
            datalist = new List<byte>();
#if NETSTANDARD || NET
            code = [];
#elif NET40_OR_GREATER
            code = Array.Empty<byte>();
#else
            code = new byte[0];
#endif
        }

        public Statement(string txt, int ln)
        {
            LineNum = ln;
            text = txt;
            datalist = new List<byte>();
            code = new byte[0];
        }

        // Parse the statement, comment and ';' already stripped
        public void Parse()
        {
            int pos = 0;

            // Clear code
            Size = 0;
            datalist = new List<byte>();

            // Skip leading spaces
            if (SkipSpace(ref pos))
            {
                return;         // empty statement
            }

            // Check for datalist only
            if (text[pos] == ',')
            {
                pos++;
                ParseDatalist(ref pos);
                return;         // datalist
            }

            // Normal statement
            if (!char.IsLetter(text[pos]))
            {
                Error = StatementError.BAD_START;
                return;
            }
            Token tk1 = Token.Parse(text, ref pos);
            if (tk1.Type != Token.TokenType.TEXT)
            {
                Error = StatementError.BAD_START;
                return;
            }
            if (pos < text.Length)
            {
                if (text[pos] == ':')
                {
                    // Label
                    Label = tk1.Text;
                    pos++;
                    SkipSpace(ref pos);
                    if (pos >= text.Length)
                    {
                        Type = StatementType.NOP;
                        return;
                    }
                    // Check what is next
                    if (text[pos] == ',')
                    {
                        pos++;
                        ParseDatalist(ref pos);
                        return;
                    }
                    tk1 = Token.Parse(text, ref pos);
                    if (tk1.Type != Token.TokenType.TEXT)
                    {
                        Error = StatementError.UnrecognizedMnemonic;
                        return;
                    }
                }
                else
                {
                    SkipSpace(ref pos);
                    if ((pos < text.Length) && (text[pos] == '='))
                    {
                        // Equate
                        pos++;
                        Type = StatementType.EQU;
                        Label = tk1.Text;
                        StatementError err = EvalExpr(ref pos);
                        if (err != StatementError.None)
                        {
                            Error = err;
                        }
                        return;
                    }
                }
            }

            // Check for directive
            SkipSpace(ref pos);
            if (tk1.Text == "DC")
            {
                Type = StatementType.DC;
                ParseDatalist(ref pos);
            }
            else if (tk1.Text == "END")
            {
                Token tk2 = Token.Parse(text, ref pos);

                // Does not have parameters
                if (tk2.Type == Token.TokenType.EMPTY)
                {
                    Type = StatementType.END;
                }
                else
                {
                    Error = StatementError.InvalidSyntax;
                }
            }
            else if (tk1.Text == "ORG")
            {
                Type = StatementType.ORG;
                Error = EvalExpr(ref pos);
            }
            else if (tk1.Text == "PAGE")
            {
                Token tk2 = Token.Parse(text, ref pos);

                // Does not have parameters
                if (tk2.Type == Token.TokenType.EMPTY)
                {
                    Type = StatementType.PAGE;
                }
                else
                {
                    Error = StatementError.InvalidSyntax;
                }
            }
            else
            {
                // Must be an instruction mnemonic
                inst = InstructionTables.Lookup(tk1.Text);
                if (inst == null)
                {
                    Error = StatementError.UnrecognizedMnemonic;
                    return;
                }
                Type = StatementType.INSTR;
                Size = inst.Size;

                // Check operand
                StatementError err;
                switch (inst.Operand)
                {
                    case OperandType.NONE:
                        break;

                    case OperandType.REG0:
                        ParseReg(ref pos);
                        break;

                    case OperandType.REG1:
                        ParseReg(ref pos);
                        if ((Error == StatementError.None) && (operand == 0))
                        {
                            Error = StatementError.INV_REG;
                        }
                        break;

                    case OperandType.IODEV:
                        ParseIODev(ref pos);
                        break;

                    case OperandType.EXPR:
                        err = EvalExpr(ref pos);
                        if (err == StatementError.None)
                        {
                            if (vs == ValueSize.FORCE_TWO)
                            {
                                // CRA Feature or Bug?
                                // LDI A(LABEL) generates three bytes
                                operand = Value;
                                opersize = ValueSize.TWO;
                                Size++;
                            }
                            else
                            {
                                operand = (ushort)(Value & 0xFF);
                                opersize = ValueSize.ONE;
                            }
                        }
                        else
                        {
                            Error = err;
                        }
                        break;

                    case OperandType.ADDR8:
                        err = EvalExpr(ref pos);
                        if (err == StatementError.None)
                        {
                            if (((a.Program.pc + 1) & 0xFF00) == (Value & 0xFF00))
                            {
                                operand = (ushort)(Value & 0xFF);
                            }
                            else if (a.Program.pass == 2)
                            {
                                Error = StatementError.INV_BRANCH;
                            }
                        }
                        else
                        {
                            Error = err;
                        }
                        break;

                    case OperandType.ADDR16:
                        err = EvalExpr(ref pos);
                        if (err == StatementError.None)
                        {
                            operand = Value;
                            opersize = ValueSize.TWO;
                        }
                        else
                        {
                            Error = err;
                        }
                        break;
                }

                // Check datalist after instruction
                if (!SkipSpace(ref pos))
                {
                    if (text[pos] == ',')
                    {
                        pos++;
                        ParseDatalist(ref pos);
                    }
                    else
                    {
                        Error = StatementError.InvalidSyntax;
                    }
                }
            }
        }

        // Parse a register parameter
        // Updates type, error and operand
        private void ParseReg(ref int pos)
        {
            Token tk = Token.Parse(text, ref pos);
            switch (tk.Type)
            {
                case Token.TokenType.BCONST:
                case Token.TokenType.DCONST:
                case Token.TokenType.HCONST:
                    operand = (ushort)(EvalConst(tk) & 0xF);
                    break;

                case Token.TokenType.TEXT:
                    Symbol symb = a.Program.symtab.Lookup(tk.Text);
                    if (symb != null)
                    {
                        operand = (ushort)(symb.Value & 0xF);
                    }
                    else if ((tk.Text.Length == 2) && (tk.Text[0] == 'R'))
                    {
                        char r = tk.Text[1];
                        if ((r >= '0') && (r <= '9'))
                        {
                            operand = (ushort)(r - '0');
                        }
                        else if ((r >= 'A') && (r <= 'F'))
                        {
                            operand = (ushort)(r - 'A' + 10);
                        }
                        else
                        {
                            Error = StatementError.INV_REG;
                        }
                    }
                    else
                    {
                        Regex rx = new Regex("^[0-9A-F]{1,4}$");
                        if (rx.Match(tk.Text).Success)
                        {
                            // Unprefixed hex constant
                            tk.Type = Token.TokenType.HCONST;
                            operand = (ushort)(EvalConst(tk) & 0xF);
                        }
                        else if (a.Program.pass == 2)
                        {
                            Error = StatementError.UndefinedSymbol;
                        }
                    }

                    break;

                default:
                    Error = StatementError.InvalidSyntax;
                    break;
            }
        }

        // Parse a IO device parameter
        // Updates type, error and operand
        private void ParseIODev(ref int pos)
        {
            Token tk = Token.Parse(text, ref pos);
            switch (tk.Type)
            {
                case Token.TokenType.BCONST:
                case Token.TokenType.DCONST:
                case Token.TokenType.HCONST:
                    operand = (ushort)(EvalConst(tk) & 0x7);
                    break;

                case Token.TokenType.TEXT:
                    Symbol symb = a.Program.symtab.Lookup(tk.Text);
                    if (symb != null)
                    {
                        operand = (ushort)(symb.Value & 0x7);
                    }
                    else
                    {
                        Regex rx = new Regex("^[0-9A-F]{1,4}$");
                        if (rx.Match(tk.Text).Success)
                        {
                            // Unprefixed hex constant
                            tk.Type = Token.TokenType.HCONST;
                            operand = (ushort)(EvalConst(tk) & 0x7);
                        }
                        else if (a.Program.pass == 2)
                        {
                            Error = StatementError.UndefinedSymbol;
                        }
                    }
                    break;

                default:
                    Error = StatementError.InvalidSyntax;
                    break;
            }
        }

        // Parses a datalist
        private void ParseDatalist(ref int pos)
        {
            Token tk;
            while (!SkipSpace(ref pos))
            {
                int aux = pos;
                tk = Token.Parse(text, ref aux);
                if (tk.Type == Token.TokenType.STRING)
                {
                    foreach (char c in tk.Text)
                    {
                        datalist.Add((byte)c);
                    }
                    pos = aux;
                }
                else
                {
                    StatementError err = EvalExpr(ref pos);
                    if (err == StatementError.None)
                    {
                        if (vs != ValueSize.ONE)
                        {
                            datalist.Add((byte)(Value >> 8));
                        }
                        datalist.Add((byte)(Value & 0xFF));
                    }
                    else
                    {
                        Error = err;
                        break;
                    }
                }

                if (!SkipSpace(ref pos))
                {
                    if (text[pos] != ',')
                    {
                        Error = StatementError.InvalidSyntax;
                    }
                    else
                    {
                        pos++;
                    }
                }
            }

            Size += (ushort)datalist.Count;
        }

        // Evaluate expression, put result in _value
        private enum ExprMode
        { NORMAL, ADDR, ADDR_LOW, ADDR_HIGH };

        private StatementError EvalExpr(ref int pos)
        {
            ExprMode mode = ExprMode.NORMAL;
            StatementError err = StatementError.None;
            int nbytes = 1;

            // check for A(sexpr), A.0(sexpr) and A.1(sexpr)
            if (SkipSpace(ref pos))
            {
                return StatementError.ExpectedExpression;
            }
            if ((char.ToUpper(text[pos]) == 'A') && (pos < (text.Length - 1)))
            {
                int aux = pos + 1;

                if (text[aux] == '.')
                {
                    if (aux < text.Length)
                    {
                        aux++;
                        if (text[aux] == '0')
                        {
                            aux++;
                            mode = ExprMode.ADDR_LOW;
                        }
                        else if (text[aux] == '1')
                        {
                            aux++;
                            mode = ExprMode.ADDR_HIGH;
                        }
                        else
                        {
                            return StatementError.PeriodError;
                        }
                    }
                    else
                    {
                        return StatementError.PeriodError;
                    }
                }

                if (SkipSpace(ref aux) || (text[aux] != '('))
                {
                    if (mode != ExprMode.NORMAL)
                    {
                        return StatementError.PeriodError;
                    }
                }
                else
                {
                    // all correct (for now)
                    pos = aux + 1;
                    switch (mode)
                    {
                        case ExprMode.NORMAL:
                            mode = ExprMode.ADDR;
                            Value = EvalSimpleExpr(text, ref pos, ref err, ref nbytes);
                            vs = ValueSize.FORCE_TWO;
                            break;

                        case ExprMode.ADDR_LOW:
                            Value = (ushort)(EvalSimpleExpr(text, ref pos, ref err, ref nbytes) & 0xFF);
                            vs = ValueSize.ONE;
                            break;

                        case ExprMode.ADDR_HIGH:
                            Value = (ushort)(EvalSimpleExpr(text, ref pos, ref err, ref nbytes) >> 8);
                            vs = ValueSize.ONE;
                            break;
                    }
                    if (err != StatementError.None)
                    {
                        // something went wrong parsing simple expression
                        return err;
                    }

                    // now find the closing parentheses
                    if (SkipSpace(ref pos) || (text[pos] != ')'))
                    {
                        return StatementError.MISSING_PAREN;
                    }
                    else
                    {
                        pos++;
                        return StatementError.None;
                    }
                }
            }

            Value = EvalSimpleExpr(text, ref pos, ref err, ref nbytes);
            vs = (nbytes == 1) ? ValueSize.ONE : ValueSize.TWO;
            return err;
        }

        // Evaluate simple expression
        private ushort EvalSimpleExpr(string text, ref int pos, ref StatementError err, ref int nbytes)
        {
            ushort val = 0;

            if (SkipSpace(ref pos))
            {
                err = StatementError.ExpectedExpression;
            }
            else
            {
                if (text[pos] == '*')
                {
                    val = a.Program.pc;
                    pos++;
                    nbytes = 2;
                }
                else
                {
                    Token tk = Token.Parse(text, ref pos);
                    switch (tk.Type)
                    {
                        case Token.TokenType.BCONST:
                        case Token.TokenType.DCONST:
                        case Token.TokenType.HCONST:
                            val = EvalConst(tk);
                            if (val > 0xFF)
                            {
                                nbytes = 2;
                            }
                            return val;

                        case Token.TokenType.STRING:
                            return (ushort)tk.Text[0];

                        case Token.TokenType.TEXT:
                            Symbol symb = a.Program.symtab.Lookup(tk.Text);
                            if (symb == null)
                            {
                                // Special case: accept unprefixed hex constants
                                //               starting with a letter
                                Regex rx = new Regex("^[0-9A-F]{1,4}$");
                                if (rx.Match(tk.Text).Success)
                                {
                                    tk.Type = Token.TokenType.HCONST;
                                    val = EvalConst(tk);
                                    if (val > 0xFF)
                                    {
                                        nbytes = 2;
                                    }
                                    return val;
                                }
                                else
                                {
                                    if (a.Program.pass == 2)
                                    {
                                        // Error if pass 2
                                        err = StatementError.UndefinedSymbol;
                                    }
                                    else
                                    {
                                        val = 0;
                                    }
                                }
                            }
                            else
                            {
                                val = symb.Value;
                            }
                            break;

                        default:
                            err = StatementError.ExpectedExpression;
                            return 0;
                    }
                }

                // check for +/- const
                if (!SkipSpace(ref pos) &&
                    ((text[pos] == '+') || (text[pos] == '-')))
                {
                    nbytes = 2;
                    char oper = text[pos++];
                    SkipSpace(ref pos);
                    Token tk = Token.Parse(text, ref pos);
                    switch (tk.Type)
                    {
                        case Token.TokenType.BCONST:
                        case Token.TokenType.DCONST:
                        case Token.TokenType.HCONST:
                            if (oper == '+')
                            {
                                val += EvalConst(tk);
                            }
                            else
                            {
                                val -= EvalConst(tk);
                            }
                            break;

                        default:
                            err = StatementError.MISSING_CONST;
                            return 0;
                    }
                }
            }
            return val;
        }

        // Generates object code (Pass2)
        public byte[] Generate()
        {
            int pos = 0;
            code = new byte[Size];

            if (Type == StatementType.INSTR)
            {
                switch (inst.Operand)
                {
                    case OperandType.NONE:
                        code[pos++] = inst.Opcode;
                        break;

                    case OperandType.REG0:
                    case OperandType.REG1:
                    case OperandType.IODEV:
                        code[pos++] = (byte)(inst.Opcode + operand);
                        break;

                    case OperandType.EXPR:
                        code[pos++] = inst.Opcode;
                        if (opersize == ValueSize.TWO)
                        {
                            // CRA Feature or bug?
                            code[pos++] = (byte)(operand >> 8);
                        }
                        code[pos++] = (byte)(operand & 0xFF);
                        break;

                    case OperandType.ADDR8:
                        code[pos++] = inst.Opcode;
                        code[pos++] = (byte)operand;
                        break;

                    case OperandType.ADDR16:
                        code[pos++] = inst.Opcode;
                        code[pos++] = (byte)(operand >> 8);
                        code[pos++] = (byte)(operand & 0xFF);
                        break;
                }
            }
            foreach (byte b in datalist)
            {
                code[pos++] = b;
            }
            return code;
        }

        // Skip space
        // return true if end of line
        private bool SkipSpace(ref int pos)
        {
            while ((pos < text.Length) && char.IsWhiteSpace(text[pos]))
            {
                pos++;
            }
            return pos == text.Length;
        }

        // Eval constant
        private static ushort EvalConst(Token tk)
        {
            ushort val = 0;

            switch (tk.Type)
            {
                case Token.TokenType.BCONST:
                    foreach (char c in tk.Text)
                    {
                        val = (ushort)((val << 1) + (c - '0'));
                    }
                    break;

                case Token.TokenType.DCONST:
                    foreach (char c in tk.Text)
                    {
                        val = (ushort)((val * 10) + (c - '0'));
                    }
                    break;

                case Token.TokenType.HCONST:
                    foreach (char c in tk.Text)
                    {
                        val = (ushort)(val << 4);
                        if (c <= '9')
                        {
                            val += (ushort)(c - '0');
                        }
                        else
                        {
                            val += (ushort)(c - 'A' + 10);
                        }
                    }
                    break;
            }
            return val;
        }
    }
}
