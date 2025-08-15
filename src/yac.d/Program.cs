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
 * DISASM1802 - CDP1802 Disassembler
 *
 * Disassembles code for the RCA CDP1802 microprocessor
 *
 * Usage: Disasm1802 name
 *
 *        Binary code will be read from name.hex (Intel hex format)
 *        Optional information can be provided in nome.def
 *        Output will sent to the console
 *
 */

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reflection;

namespace yac.d
{
    internal class Program
    {
        private static string HexFile;
        private static string DefFile;
        private static byte[] memory;
        private static int start, end;
        private static List<MemoryArea> areas;
        private static SymbolTable st;
        // private static InstructionTables iset;

        private static void Main(string[] args)
        {
            try
            {
                Console.Out.WriteLine("DISASM1802 v" +
                    Assembly.GetExecutingAssembly().GetName().Version.Major.ToString() + "." +
                    Assembly.GetExecutingAssembly().GetName().Version.Minor.ToString("D2"));
                Console.Out.WriteLine("(C) 2022, Daniel Quadros https://dqsoft.blogspot.com");
                Console.Out.WriteLine();
                if (Init(args))
                {
                    Disasm(false);      // get labels
                    Disasm(true);       // generate output
                }
            }
            catch (Exception ex)
            {
                Console.Out.WriteLine("FATAL ERROR: " + ex.Message);
            }
            Console.In.Read();
        }

        // Initialization
        // Returns false in case of a serious error
        private static bool Init(string[] args)
        {
            // Check parameters
            if (!ParseParam(args))
            {
                Console.Out.WriteLine("usage: DISASM1802 name");
                return false;
            }

            // Load hex file
            if (!LoadHex(HexFile))
            {
                Console.Out.WriteLine("Error loading " + HexFile);
                return false;
            }

            // Define area and symbols
            areas = new List<MemoryArea>();
            st = new SymbolTable();
            if (File.Exists(DefFile))
            {
                // Load from file
                if (!LoadDef(DefFile))
                {
                    Console.Out.WriteLine("Error loading " + DefFile);
                    return false;
                }
            }
            else
            {
                // No definiton file, assume all is code, no symbols
                areas.Add(new MemoryArea(MemoryArea.MemoryType.CODE, start, end));
            }

            return true;
        }

        // Parses parameters - returns false if invalid
        // For now accepts only the base name for the input files
        private static bool ParseParam(string[] args)
        {
            if (args.Length < 1)
            {
                return false;
            }
            HexFile = args[0];
            if (!Path.HasExtension(HexFile))
            {
                HexFile = HexFile + ".hex";
            }
            DefFile = Path.ChangeExtension(HexFile, ".def");
            return true;
        }

        // Load hex file into memory array
        // Intel Hex format
        // :llaaaattdd...ddcc
        // ll = length, aaaa = address, tt = Type, dd = hexadecimal data, cc = checksum
        // return false if error
        private static bool LoadHex(string file)
        {
            try
            {
                if (!File.Exists(file))
                {
                    Console.Out.WriteLine("ERROR: " + file + " not found");
                    return false;
                }
                memory = new byte[64 * 1024];
                start = 0xFFFF;
                end = 0;
                using (StreamReader sr = new StreamReader(file))
                {
                    while (!sr.EndOfStream)
                    {
                        string line = sr.ReadLine();
                        if ((line != null) && (line.Length > 9) && (line[0] == ':') && (line.Substring(7, 2) == "00"))
                        {
                            int len = int.Parse(line.Substring(1, 2), NumberStyles.HexNumber);
                            int addr = int.Parse(line.Substring(3, 4), NumberStyles.HexNumber);
                            if (addr < start)
                            {
                                start = addr;
                            }
                            for (int i = 0; i < len; i++)
                            {
                                memory[addr] = byte.Parse(line.Substring(9 + 2 * i, 2), NumberStyles.HexNumber);
                                addr++;
                            }
                            if (addr > end)
                            {
                                end = addr;
                            }
                        }
                    }
                }
                Console.Out.WriteLine(file + " loaded: start=0x" + start.ToString("X4") + ", end=0x" + end.ToString("X4"));
                return true;
            }
            catch (Exception ex)
            {
                Console.Out.WriteLine("FATAL ERROR: " + ex.Message);
                return false;
            }
        }

        // Load area and symbol definitions
        // Definition file lines format:
        //   CODE start end
        //   DATA start end
        //   addr name
        private static bool LoadDef(string file)
        {
            try
            {
                using (StreamReader sr = new StreamReader(file))
                {
                    while (!sr.EndOfStream)
                    {
                        string line = sr.ReadLine();
                        if (line != null)
                        {
                            string[] fields = line.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                            if (fields.Length > 2)
                            {
                                if (fields[0] == "CODE")
                                {
                                    areas.Add(new MemoryArea(MemoryArea.MemoryType.CODE, int.Parse(fields[1], NumberStyles.HexNumber),
                                        int.Parse(fields[2], NumberStyles.HexNumber)));
                                }
                                else if (fields[0] == "DATA")
                                {
                                    areas.Add(new MemoryArea(MemoryArea.MemoryType.DATA, int.Parse(fields[1], NumberStyles.HexNumber),
                                        int.Parse(fields[2], NumberStyles.HexNumber)));
                                }
                            }
                            else if (fields.Length > 1)
                            {
                                st.Add(fields[1], int.Parse(fields[0], NumberStyles.HexNumber));
                            }
                        }
                    }
                }
                areas.Sort();
                Console.Out.WriteLine(file + " loaded: " + areas.Count + " areas, " + st.Count + " symbols");
                return true;
            }
            catch (Exception ex)
            {
                Console.Out.WriteLine("FATAL ERROR: " + ex.Message);
                return false;
            }
        }

        // Main disassembly rotine
        private static void Disasm(bool emit)
        {
            int iArea = 0;
            MemoryArea area = areas[0];

            string data = "";       // buffer for outputing data bytes

            for (int addr = start; addr < end; addr++)
            {
                if (emit && st.HasName(addr))
                {
                    if (data.Length > 0)
                    {
                        Console.Out.WriteLine("    " + data);
                        data = "";
                    }
                    Console.Out.WriteLine(st.GetName(addr) + ":");
                }

                MemoryArea.MemoryType mtype = MemoryArea.MemoryType.DATA;   // assume DATA if out of an area
                if (addr >= area.start)
                {
                    int oldArea = iArea;
                    while ((iArea < (areas.Count - 1)) && (addr >= area.end))
                    {
                        iArea++;
                        area = areas[iArea];
                    }
                    if ((data.Length > 0) && (oldArea != iArea))
                    {
                        if (emit)
                        {
                            Console.Out.WriteLine("    " + data);
                        }
                        data = "";
                    }
                }

                if ((addr >= area.start) && (addr < area.end))
                {
                    mtype = area.Type;
                }

                if (mtype == MemoryArea.MemoryType.CODE)
                {
                    int reg, dev, operand, target;
                    string name;
                    byte code = memory[addr];
                    Instruction i = InstructionTables.Lookup(code);
                    switch (i.Operand)
                    {
                        case OperandType.NONE:
                            if (emit)
                            {
                                Console.Out.WriteLine("    " + i.Mnemonic);
                            }
                            break;

                        case OperandType.REG0:
                        case OperandType.REG1:
                            reg = code & 0x0F;
                            if (emit)
                            {
                                Console.Out.WriteLine("    " + i.Mnemonic.PadRight(5) + "R" + reg.ToString());
                            }
                            break;

                        case OperandType.IODEV:
                            dev = code & 0x07;
                            if (emit)
                            {
                                Console.Out.WriteLine("    " + i.Mnemonic.PadRight(5) + dev.ToString());
                            }
                            break;

                        case OperandType.EXPR:
                            operand = memory[++addr];
                            if (emit)
                            {
                                Console.Out.WriteLine("    " + i.Mnemonic.PadRight(5) + "#" + operand.ToString("X2"));
                            }
                            break;

                        case OperandType.ADDR8:
                            target = (addr & 0xFF00) + memory[++addr];
                            name = st.GetName(target);
                            if (!st.HasName(target))
                            {
                                st.Add(name, target);
                            }
                            if (emit)
                            {
                                Console.Out.WriteLine("    " + i.Mnemonic.PadRight(5) + name);
                            }
                            break;

                        case OperandType.ADDR16:
                            target = (memory[++addr] << 16) + memory[++addr];
                            name = st.GetName(target);
                            if (!st.HasName(target))
                            {
                                st.Add(name, target);
                            }
                            if (emit)
                            {
                                Console.Out.WriteLine("    " + i.Mnemonic.PadRight(5) + name);
                            }
                            break;
                    }
                }
                else if (emit)
                {
                    data += ", #" + memory[addr].ToString("X2");
                    if (data.Length > 60)
                    {
                        Console.Out.WriteLine("    " + data);
                        data = "";
                    }
                }
            }
            if (emit)
            {
                if (data.Length > 0)
                {
                    Console.Out.WriteLine("    " + data);
                    data = "";
                }
                Console.Out.WriteLine("    END");
            }
        }
    }
}
