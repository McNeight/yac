// <copyright file="Program.cs" company="yac Contributors">
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
 * Assembles code for the RCA CDP1802 microprocessor writen
 * in Level I Assemby Language (as defined in the Operator
 * Manual for the RCA COSMAC Development System II.
 *
 * Usage: Asm1802 source
 *
 * (C) 2017, Daniel Quadros
 *
 */

using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;

namespace yac.a
{
    internal class Program
    {
        private static ILoggerFactory _loggerFactory;
        private static ILogger<Program> _logger;

        private static string SourceFile;               // source file name
        private static string[] source;                 // source file in memory
        private static List<Statement> lstStatements;   // parsed source file
        public static SymbolTable symtab;               // symbol table
        public static ushort pc;                        // location counter
        public static int pass;                         // pass number
        private static int errcount;                    // number of errors in pass2

        // Program entry point
        private static void Main(string[] args)
        {
            // 1. Create a LoggerFactory instance and configure logging providers.
            _loggerFactory = LoggerFactory.Create(builder =>
            {
                // Add the console logger provider.
                // This will output log messages to the console.
                builder.AddConsole();

                // Optionally, set the minimum log level for all providers.
                // Messages below this level will not be logged.
                builder.SetMinimumLevel(LogLevel.Debug);
            });

            // 2. Create an ILogger instance for a specific category.
            // The category is typically the name of the class where logging occurs.
            _logger = _loggerFactory.CreateLogger<Program>();

            try
            {
                Console.Out.WriteLine("COSMAC1802.Assembler v" +
                    Assembly.GetExecutingAssembly().GetName().Version.Major.ToString() + "." +
                    Assembly.GetExecutingAssembly().GetName().Version.Minor.ToString("D2"));
                Console.Out.WriteLine("Copyright © 2017, 2022 Daniel Quadros, https://dqsoft.blogspot.com");
                Console.Out.WriteLine("Copyright © 2025 Neil McNeight");
                Console.Out.WriteLine();
                if (Init(args))
                {
                    PreProcess();
                    Pass1();
                    Pass2();
                    Info();
                }
            }
            catch (Exception ex)
            {
                Console.Out.WriteLine("FATAL ERROR: " + ex.Message);
            }
        }

        // Initialization
        // Returns false in case of a serious error
        private static bool Init(string[] args)
        {
            // Check parameters
            if (!ParseParam(args))
            {
                Console.Out.WriteLine("usage: ASM1802 source");
                return false;
            }

            // Load source file
            if (!File.Exists(SourceFile))
            {
                Console.Out.WriteLine("ERROR: " + SourceFile + " not found");
                return false;
            }
            source = File.ReadAllLines(SourceFile);

            // Create symbol table
            symtab = new SymbolTable();

            errcount = 0;

            return true;
        }

        // Parses parameters - returns false if invalid
        // For now accepts only the source file name
        private static bool ParseParam(string[] args)
        {
            if (args.Length < 1)
            {
                return false;
            }
            if (Path.HasExtension(args[0]))
            {
                SourceFile = args[0];
            }
            else
            {
                SourceFile = args[0] + ".asm";
            }
            return true;
        }

        // Breaks lines into statements
        private enum PreProcState
        {
            TEXT, STRING, PERIOD
        };

        private static void PreProcess()
        {
            int linenum = 1;
            PreProcState state;
            lstStatements = new List<Statement>();
            foreach (string line in source)
            {
                int pos = 0;
                int start = 0;
                state = PreProcState.TEXT;
                while (pos < line.Length)
                {
                    switch (state)
                    {
                        case PreProcState.TEXT:
                            if (line[pos] == '\'')
                            {
                                state = PreProcState.STRING;
                            }
                            else if (line[pos] == '.')
                            {
                                state = PreProcState.PERIOD;
                            }
                            else if (line[pos] == ';')
                            {
                                string text = line.Substring(start, pos - start);
                                lstStatements.Add(new Statement(text, linenum));
                                start = pos + 1;
                            }
                            pos++;
                            break;

                        case PreProcState.PERIOD:
                            if (line[pos] == '.')
                            {
                                string text = line.Substring(start, pos - start - 1);
                                lstStatements.Add(new Statement(text, linenum));
                                start = pos = line.Length;
                            }
                            else
                            {
                                state = PreProcState.TEXT;
                                pos++;
                            }
                            break;

                        case PreProcState.STRING:
                            if (line[pos] == '\'')
                            {
                                state = PreProcState.TEXT;
                            }
                            pos++;
                            break;
                    }
                }
                if (start < pos)
                {
                    string text = line.Substring(start, pos - start);
                    lstStatements.Add(new Statement(text, linenum));
                }
                linenum++;
            }
        }

        // First Pass
        private static void Pass1()
        {
            bool ended = false;
            pc = 0;
            pass = 1;

            foreach (Statement st in lstStatements)
            {
                st.Parse();
                if (st.Type != Statement.StatementType.ERROR)
                {
                    // Treat symbol definitions
                    if (st.Label != "")
                    {
                        Symbol symb = symtab.Lookup(st.Label);
                        if (symb != null)
                        {
                            symb.MarkDup();
                        }
                        else if (st.Type == Statement.StatementType.EQU)
                        {
                            symtab.Add(st.Label, st.Value);
                            continue;   // that is all in this statement
                        }
                        else
                        {
                            symtab.Add(st.Label, pc);
                        }
                    }

                    // Treat directives
                    if (st.Type == Statement.StatementType.ORG)
                    {
                        // change PC
                        pc = st.Value;
                    }
                    else if (st.Type == Statement.StatementType.PAGE)
                    {
                        // Advance PC to start of next page
                        pc = (ushort)((pc + 0x100) & 0xFF00);
                    }
                    else if (st.Type == Statement.StatementType.END)
                    {
                        // end of source program
                        ended = true;   // ignote all text after END
                        break;
                    }
                    else
                    {
                        // Normal statement
                        pc += (ushort)st.Size;
                    }
                }
            }
            if (!ended)
            {
                Console.Out.WriteLine("Missing END directive");
            }
        }

        // Second pass
        private static void Pass2()
        {
            List<string> lstErrors = new List<string>();
#if NETSTANDARD || NET
            byte[] objcode = [];
#elif NET40_OR_GREATER
            byte[] objcode = Array.Empty<byte>();
#else
            byte[] objcode = new byte[0];
#endif
            int sline = 0;
            ushort linepc = 0;
            pc = 0;
            pass = 2;

            foreach (Statement st in lstStatements)
            {
                while (st.LineNum != sline)
                {
                    if (sline != 0)
                    {
                        // List previous line
                        ListLine(sline, linepc, objcode, lstErrors);
                    }
                    sline++;
                    errcount += lstErrors.Count;
                    lstErrors = new List<string>();
                    linepc = pc;
#if NETSTANDARD || NET
                    objcode = [];
#elif NET40_OR_GREATER
                    objcode = Array.Empty<byte>();
#else
                    objcode = new byte[0];
#endif
                }

                st.Parse();

                if (st.Type == Statement.StatementType.ERROR)
                {
                    // record error
                    lstErrors.Add(Statement.MsgError(st.Error));
                }
                else
                {
                    // Treat symbol definitions
                    if (st.Label != "")
                    {
                        Symbol symb = symtab.Lookup(st.Label);
                        if (symb.Duplicate)
                        {
                            lstErrors.Add(Statement.MsgError(Statement.StatementError.PreviouslyDefinedSymbol));
                        }
                        else if (st.Type == Statement.StatementType.EQU)
                        {
                            // update value
                            symb.Value = st.Value;
                            continue;   // that is all in this statement
                        }
                    }

                    // Treat directives
                    if (st.Type == Statement.StatementType.ORG)
                    {
                        // change PC
                        pc = st.Value;
                    }
                    else if (st.Type == Statement.StatementType.PAGE)
                    {
                        // Advance PC to start of next page
                        pc = (ushort)((pc + 0x100) & 0xFF00);
                    }
                    else if (st.Type == Statement.StatementType.END)
                    {
                        // end of source program
                        break;
                    }
                    else
                    {
                        // Normal statement or DC directive
                        // generate object code
                        byte[] code = st.Generate();
                        if (code.Length > 0)
                        {
                            int oldsize = objcode.Length;
                            Array.Resize<byte>(ref objcode, objcode.Length + code.Length);
                            Array.Copy(code, 0, objcode, oldsize, code.Length);
                        }
                        pc += (ushort)st.Size;
                    }
                }
            }
            // List last line
            ListLine(sline, linepc, objcode, lstErrors);
        }

        // Prints source line
        private static void ListLine(int linenum, ushort pc, byte[] objcode, List<string> lstErrors)
        {
            int off = 0;
            int nb;

            do
            {
                Console.Write((pc + off).ToString("X4"));
                Console.Write(" ");
                for (nb = 0; (off < objcode.Length) && (nb < 7); nb++, off++)
                {
                    Console.Write(objcode[off].ToString("X2"));
                }
                Console.Write(";");
                while (nb < 7)
                {
                    Console.Write("  ");
                    nb++;
                }
                Console.Write(linenum.ToString(" "));
                Console.Write(linenum.ToString("D4"));
                if (off < 8)
                {
                    Console.Write(" ");
                    Console.Write(source[linenum - 1]);
                }
                Console.WriteLine();
            } while (off < objcode.Length);

            foreach (string errmsg in lstErrors)
            {
                System.Console.Out.WriteLine(">>> " + errmsg);
            }
        }

        // Prints information about the program
        private static void Info()
        {
            Console.Out.WriteLine(errcount.ToString() + " errors");
            Console.Out.WriteLine();
            symtab.Print();
        }
    }
}
